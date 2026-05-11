namespace WindowConfigurator.Web.Service
{
    public class WindowConfiguratorDataHelper
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private const string _appDataFolder = "AppData";

        public WindowConfiguratorDataHelper(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<string> ReadAllTextAsync(string relativePath)
        {
            var path = Path.Combine(_hostingEnvironment.ContentRootPath, _appDataFolder, relativePath);

            using (var reader = File.OpenText(path))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
