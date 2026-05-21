using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data.Catalog
{
    public class CatalogService : ICatalogService
    {
        private static readonly IReadOnlyList<ProductLineCatalog> _catalog = new List<ProductLineCatalog>
        {
            new ProductLineCatalog
            {
                Key = "energysaver-2500",
                Name = "EnergySaver 2500",
                ManufacturerName = "All Weather Windows",
                ItemTemplateFile = "energySaverItemTemplate.json",
                SectionTemplateFile = "energySaverSectionTemplate.json"
            },
            new ProductLineCatalog
            {
                Key = "apex",
                Name = "Apex",
                ManufacturerName = "All Weather Windows",
                ItemTemplateFile = "apexItemTemplate.json",
                SectionTemplateFile = "apexSectionTemplate.json"
            },
            new ProductLineCatalog
            {
                Key = "carriage",
                Name = "Carriage",
                ManufacturerName = "All Weather Windows",
                ItemTemplateFile = "carriageItemTemplate.json",
                SectionTemplateFile = "carriageSectionTemplate.json"
            }
        };

        public ProductLineCatalog? GetProductLine(string key)
            => _catalog.FirstOrDefault(p => p.Key == key);

        public IReadOnlyList<ProductLineCatalog> ListAll()
            => _catalog;

        public IReadOnlyList<ProductLineCatalog> GetAllowedForTenant(TenantEntity tenant)
        {
            if (tenant.AllowedProductLineKeys.Count == 0)
                return _catalog;

            return _catalog
                .Where(p => tenant.AllowedProductLineKeys.Contains(p.Key))
                .ToList();
        }
    }
}
