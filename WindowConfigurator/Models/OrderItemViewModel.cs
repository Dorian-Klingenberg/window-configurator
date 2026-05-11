using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Models
{
    public class OrderItemViewModel
    {
        public string Id { get; set; } = string.Empty;

        public static OrderItemViewModel FromEntity(OrderItemEntity entity)
        {
            return new OrderItemViewModel()
            {
                Id = entity.Id.ToString()
            };
        }

        public static IEnumerable<OrderItemViewModel> FromEntities(IEnumerable<OrderItemEntity> entities)
        {
            var res = new List<OrderItemViewModel>();
            foreach (var entity in entities)
            {
                res.Add(OrderItemViewModel.FromEntity(entity));
            }
            return res;
        }
    }
}
