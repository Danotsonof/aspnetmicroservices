using Discount.GRPC.Protos;

namespace Basket.API.GrpcServices
{
    public class DiscountGrpcService
    {
        private readonly Discount.GRPC.Protos.Discount.DiscountClient _discountClient;

        public DiscountGrpcService(Discount.GRPC.Protos.Discount.DiscountClient discountClient)
        {
            _discountClient = discountClient ?? throw new ArgumentNullException(nameof(discountClient));
        }

        public async Task<CouponModel> GetDiscount(string productName)
        {
            var discountRequest = new GetDiscountRequest { ProductName = productName };
            return await _discountClient.GetDiscountAsync(discountRequest);
        }
    }
}
