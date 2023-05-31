using Dapper;
using Discount.API.Entities;
using Npgsql;

namespace Discount.API.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly string _connectionString;

        public DiscountRepository(IConfiguration configuration)
        {
            _connectionString = configuration?.GetValue<string>("DatabaseSettings:ConnectionString") ?? throw new ArgumentNullException(nameof(configuration));
        }

        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<Coupon> GetDiscount(string productName)
        {
            using var connection = CreateConnection();

            var coupon = await connection
                .QueryFirstOrDefaultAsync<Coupon>(
                "select * from Coupon where ProductName = @ProductName",
                new { ProductName = productName });

            if (coupon == null)
            {
                return new Coupon
                {
                    ProductName = "No Discount",
                    Amount = 0,
                    Description = "None"
                };
            }

            return coupon;
        }

        public async Task<bool> CreateDiscount(Coupon coupon)
        {
            using var connection = CreateConnection();

            var created = await connection
                .ExecuteAsync("insert into Coupon (ProductName, Description, Amount) values (@ProductName, @Description, @Amount)",
                new
                {
                    ProductName = coupon.ProductName,
                    Description = coupon.Description,
                    Amount = coupon.Amount
                });

            return created != 0;
        }

        public async Task<bool> UpdateDiscount(Coupon coupon)
        {
            using var connection = CreateConnection();

            var updated = await connection
                .ExecuteAsync("update Coupon set ProductName=@ProductName, Description=@Description, Amount=@Amount where Id=@Id",
                new
                {
                    ProductName = coupon.ProductName,
                    Description = coupon.Description,
                    Amount = coupon.Amount,
                    Id = coupon.Id
                });

            return updated != 0;
        }

        public async Task<bool> DeleteDiscount(string productName)
        {
            using var connection = CreateConnection();

            var deleted = await connection
                .ExecuteAsync("delete from Coupon where ProductName=@ProductName",
                new { ProductName = productName });

            return deleted != 0;
        }
    }
}
