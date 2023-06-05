using Discount.GRPC.Entities;

namespace Discount.GRPC.Repositories
{
    public interface IDiscountRepository
    {
        Task<bool> CreateDiscount(Coupon coupon);
        Task<bool> DeleteDiscount(string productName);
        Task<Coupon> GetDiscount(string productName);
        Task<bool> UpdateDiscount(Coupon coupon);
    }
}