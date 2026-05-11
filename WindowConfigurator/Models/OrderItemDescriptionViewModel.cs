using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Web.Models
{
    public class OrderItemDescriptionViewModel
    {
        public string Id { get; set; } = string.Empty;

        public static OrderItemDescriptionViewModel FromEntity(OrderItemEntity entity)
        {
            return new OrderItemDescriptionViewModel()
            {
                Id = entity.Id.ToString()
            };
        }

        public static IEnumerable<OrderItemDescriptionViewModel> FromEntities(IEnumerable<OrderItemEntity> entities)
        {
            var res = new List<OrderItemDescriptionViewModel>();
            foreach (var entity in entities)
            {
                res.Add(OrderItemDescriptionViewModel.FromEntity(entity));
            }
            return res;
        }
    }
}
