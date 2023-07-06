using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository basketRepository;
        private readonly DiscountGrpcService discountGrpcService;
        private readonly IMapper mapper;
        private readonly IPublishEndpoint publishEndpoint;

        public BasketController(IBasketRepository basketRepository, DiscountGrpcService discountGrpcService, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            this.basketRepository = basketRepository;
            this.discountGrpcService = discountGrpcService;
            this.mapper = mapper;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet("{username}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetBasket(string username)
        {
            return Ok(await basketRepository.GetBasket(username) ?? new ShoppingCart(username));           
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateBasket([FromBody] ShoppingCart basket)
        {
            foreach (var item in basket?.Items ?? new List<ShoppingCartItem>())
            {
                var coupon = await discountGrpcService.GetDiscount(item.ProductName);
                item.Price -= coupon.Amount;
                item.Price = item.Price < 0 ? 0 : item.Price;
            }
            return Ok(await basketRepository.UpdateBasket(basket));           
        }

        [HttpDelete]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBasket(string username)
        {
            await basketRepository.DeleteBasket(username);
            return Ok();           
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            // get existing basket with total price
            var basket = await basketRepository.GetBasket(basketCheckout.UserName);
            if (basket == null)
            {
                return BadRequest();
            }

            // send checkout event to rabbitmq
            var eventMessage = mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = (decimal)basket.TotalPrice;
            await publishEndpoint.Publish(eventMessage);

            // remove the basket
            await basketRepository.DeleteBasket(basket.UserName);

            return Accepted();
        }
    }
}
