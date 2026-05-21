using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Tests.Catalog
{
    public class CatalogServiceTests
    {
        private readonly ICatalogService _catalog = new CatalogService();

        [Fact]
        public void GetProductLine_EnergySaver_ReturnsCorrectEntry()
        {
            var result = _catalog.GetProductLine("energysaver-2500");
            Assert.NotNull(result);
            Assert.Equal("energysaver-2500", result.Key);
            Assert.Equal("EnergySaver 2500", result.Name);
            Assert.Equal("All Weather Windows", result.ManufacturerName);
            Assert.Equal("energySaverItemTemplate.json", result.ItemTemplateFile);
            Assert.Equal("energySaverSectionTemplate.json", result.SectionTemplateFile);
        }

        [Fact]
        public void GetProductLine_Apex_ReturnsCorrectEntry()
        {
            var result = _catalog.GetProductLine("apex");
            Assert.NotNull(result);
            Assert.Equal("apex", result.Key);
            Assert.Equal("Apex", result.Name);
            Assert.Equal("apexItemTemplate.json", result.ItemTemplateFile);
            Assert.Equal("apexSectionTemplate.json", result.SectionTemplateFile);
        }

        [Fact]
        public void GetProductLine_Carriage_ReturnsCorrectEntry()
        {
            var result = _catalog.GetProductLine("carriage");
            Assert.NotNull(result);
            Assert.Equal("carriage", result.Key);
            Assert.Equal("Carriage", result.Name);
            Assert.Equal("carriageItemTemplate.json", result.ItemTemplateFile);
            Assert.Equal("carriageSectionTemplate.json", result.SectionTemplateFile);
        }

        [Fact]
        public void GetProductLine_UnknownKey_ReturnsNull()
        {
            var result = _catalog.GetProductLine("nonexistent-product");
            Assert.Null(result);
        }

        [Fact]
        public void ListAll_ReturnsAllThreeProductLines()
        {
            var all = _catalog.ListAll();
            Assert.Equal(3, all.Count);
            Assert.Contains(all, p => p.Key == "energysaver-2500");
            Assert.Contains(all, p => p.Key == "apex");
            Assert.Contains(all, p => p.Key == "carriage");
        }

        [Fact]
        public void GetAllowedForTenant_FiltersToAllowedKeys()
        {
            var tenant = new TenantEntity
            {
                AllowedProductLineKeys = new List<string> { "apex", "carriage" }
            };

            var allowed = _catalog.GetAllowedForTenant(tenant);
            Assert.Equal(2, allowed.Count);
            Assert.Contains(allowed, p => p.Key == "apex");
            Assert.Contains(allowed, p => p.Key == "carriage");
            Assert.DoesNotContain(allowed, p => p.Key == "energysaver-2500");
        }

        [Fact]
        public void GetAllowedForTenant_EmptyAllowedKeys_ReturnsAll()
        {
            var tenant = new TenantEntity
            {
                AllowedProductLineKeys = new List<string>()
            };

            var allowed = _catalog.GetAllowedForTenant(tenant);
            Assert.Equal(3, allowed.Count);
        }
    }
}
