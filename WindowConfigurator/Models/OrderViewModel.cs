using WindowConfigurator.Data.Entities;
using WindowConfigurator.Models;

namespace WindowConfigurator.Web.Models
{
    public class OrderViewModel
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string SalesforceUserId { get; set; } = string.Empty;
        public string SalesForceOpportunityId { get; set; } = string.Empty;
        public List<OrderItemDescriptionViewModel> OrderItems { get; set; } = new List<OrderItemDescriptionViewModel>();

        public static OrderViewModel FromEntity(OrderEntity entity)
        {
            return new OrderViewModel()
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Address = entity.Address,
                SalesforceUserId = entity.SalesforceUserId,
                SalesForceOpportunityId = entity.SalesForceOpportunityId,
                OrderItems = OrderItemDescriptionViewModel.FromEntities(entity.OrderItems).ToList()
            };
        }

        public static IEnumerable<OrderViewModel> FromEntities(IEnumerable<OrderEntity> entities)
        {
            var res = new List<OrderViewModel>();
            foreach (var entity in entities)
            {
                res.Add(OrderViewModel.FromEntity(entity));
            }
            return res;
        }
    }
}
