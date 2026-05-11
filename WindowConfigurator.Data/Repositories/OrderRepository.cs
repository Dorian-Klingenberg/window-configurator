using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data.Repositories
{
    public class InMemoryOrderRepository : IOrderRepository
    {
        public List<OrderEntity> Orders { get; set; } = new List<OrderEntity>();

        public InMemoryOrderRepository()
        {
            Orders.Add(new OrderEntity()
            {
                Id = Guid.NewGuid(),
                FirstName = "Fred",
                LastName = "Flintstone",
                Address = "301 Cobblestone Way, Bedrock 70777",
                SalesForceOpportunityId = "",
                SalesforceUserId = "0058c000008YNK1AAO",
                OrderItems = new List<OrderItemEntity>()
                {
                    new OrderItemEntity { Id = Guid.NewGuid() },
                    new OrderItemEntity { Id = Guid.NewGuid() },
                    new OrderItemEntity { Id = Guid.NewGuid() }
                }
            });
            Orders.Add(new OrderEntity()
            {
                Id = Guid.NewGuid(),
                FirstName = "Barney",
                LastName = "Rubble",
                Address = "142 Boulder Avenue",
                SalesForceOpportunityId = "",
                SalesforceUserId = "0058c000008YNK1AAO"
            });
            Orders.Add(new OrderEntity()
            {
                Id = Guid.NewGuid(),
                FirstName = "Mysterio",
                LastName = "That's all, just Mysterio",
                Address = "890 Fifth Avenue",
                SalesForceOpportunityId = "",
                SalesforceUserId = "0058c000008HaPMAA0",
                OrderItems = new List<OrderItemEntity>()
                {
                    new OrderItemEntity { Id = Guid.NewGuid() },
                    new OrderItemEntity { Id = Guid.NewGuid() },
                    new OrderItemEntity { Id = Guid.NewGuid() }
                }
            }); 
            Orders.Add(new OrderEntity()
            {
                Id = Guid.NewGuid(),
                FirstName = "Stephen",
                LastName = "Strange",
                Address = "177A Bleecker Street",
                SalesForceOpportunityId = "",
                SalesforceUserId = "0058c000008HaPMAA0"
            });

        }

        public void Add(OrderEntity order)
        {
            Orders.Add(order);
        }

        public OrderEntity? GetById(Guid orderId)
        {
            return Orders.FirstOrDefault(o => o.Id == orderId);
        }

        public IEnumerable<OrderEntity> GetOrdersForSalesforceUser(string userId)
        {
            return Orders.Where(o => o.SalesforceUserId.Equals(userId));
        }
    }
}
