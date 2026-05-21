namespace WindowConfigurator.Web.Service
{
    public class WindowConfiguratorDataHelper : ITemplateReader
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private const string _appDataFolder = "AppData";

        public WindowConfiguratorDataHelper(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<string> ReadTemplateAsync(string filename)
        {
            var path = Path.Combine(_hostingEnvironment.ContentRootPath, _appDataFolder, filename);
            using var reader = File.OpenText(path);
            return await reader.ReadToEndAsync();
        }

        /// <summary>Legacy overload retained for non-template reads (e.g. priceInfo.json).</summary>
        public Task<string> ReadAllTextAsync(string relativePath)
            => ReadTemplateAsync(relativePath);
    }
}

