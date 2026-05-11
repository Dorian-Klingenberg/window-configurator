using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowConfigurator.Data.Entities
{
    public class OrderEntity
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set;} = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string SalesforceUserId { get; set; } = string.Empty;
        public string SalesForceOpportunityId { get; set; } = string.Empty;
        public List<OrderItemEntity> OrderItems { get; set; } = new List<OrderItemEntity>();
    }
}
