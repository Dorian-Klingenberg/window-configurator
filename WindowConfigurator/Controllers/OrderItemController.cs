using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WindowConfigurator.Models;
using WindowConfigurator.Web.Service;

namespace WindowConfigurator.Controllers
{
    public class OrderItemController : Controller
    {
        private readonly WindowConfiguratorDataHelper windowConfiguratorDataHelper;

        public OrderItemController(WindowConfiguratorDataHelper windowConfiguratorDataHelper)
        {
            this.windowConfiguratorDataHelper = windowConfiguratorDataHelper;
        }

        public IActionResult Index(string id)
        {
            if(string.IsNullOrEmpty(id)) return NotFound();
            return View(new OrderItemViewModel() { Id = id });
        }

        [HttpPost]
        public IActionResult Post(string id)
        {
            return Json(new { Id = id });
        }

        [HttpGet]
        public async Task<IActionResult> GetOverview(string id)
        {
            var json = await windowConfiguratorDataHelper.ReadAllTextAsync("energySaverItemTemplate.json");
            return Ok(json);
            //return Json(new
            //{
            //    Id = id,
            //    FrameWidth = "44 1/2",
            //    FrameHeight = "33",
            //    Units = "Imperial"
            //});
        }

        [HttpGet]
        public async Task<IActionResult> PriceInfo(string id)
        {
            var json = await windowConfiguratorDataHelper.ReadAllTextAsync("priceInfo.json");
            return Ok(json);
        }

        [HttpGet]
        public async Task<IActionResult> SectionTemplate(string templateName)
        {
            var json = await windowConfiguratorDataHelper.ReadAllTextAsync("energySaverSectionTemplate.json");
            return Ok(json);
        }
    }
}
