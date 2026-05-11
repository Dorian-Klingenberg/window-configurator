using Microsoft.AspNetCore.Mvc;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Repositories;
using WindowConfigurator.Web.Models;
using WindowConfigurator.Web.Service;

namespace WindowConfigurator.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }
        public async Task<IActionResult> Index()
        {
            return View(OrderViewModel.FromEntities(orderRepository.GetOrdersForSalesforceUser("0058c000008HaPMAA0")));
        }

        public IActionResult Edit(Guid orderId)
        {
            var order = orderRepository.GetOrdersForSalesforceUser("0058c000008HaPMAA0").Where(o => o.Id.Equals(orderId)).FirstOrDefault();
            if (order == null) return NotFound();
            return View(OrderViewModel.FromEntity(order));
        }
    }
}
