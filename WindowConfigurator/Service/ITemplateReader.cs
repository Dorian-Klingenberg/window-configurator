namespace WindowConfigurator.Web.Service
{
    /// <summary>
    /// Reads a named template file from the AppData folder.
    /// Abstracted to allow test doubles in controller integration tests.
    /// </summary>
    public interface ITemplateReader
    {
        Task<string> ReadTemplateAsync(string filename);
    }
}
