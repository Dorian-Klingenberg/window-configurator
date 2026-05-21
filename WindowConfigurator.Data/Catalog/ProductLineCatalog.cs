namespace WindowConfigurator.Data.Catalog
{
    public class ProductLineCatalog
    {
        /// <summary>Lowercase kebab-case key. E.g. "energysaver-2500", "apex", "carriage".</summary>
        public string Key { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string ManufacturerName { get; set; } = string.Empty;

        /// <summary>Filename (not path) of the item template JSON in AppData.</summary>
        public string ItemTemplateFile { get; set; } = string.Empty;

        /// <summary>Filename (not path) of the section template JSON in AppData.</summary>
        public string SectionTemplateFile { get; set; } = string.Empty;
    }
}
