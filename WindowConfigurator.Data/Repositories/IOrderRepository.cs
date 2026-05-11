using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data.Repositories
{
    public interface IOrderRepository
    {
        IEnumerable<OrderEntity> GetOrdersForSalesforceUser(string userId);
        OrderEntity? GetById(Guid orderId);
        void Add(OrderEntity order);
    }
}