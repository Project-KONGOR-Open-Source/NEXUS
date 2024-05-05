namespace MERRICK.Database.Data;

public static class DataFiles
{
    private static readonly string BasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

    public static readonly string Guides = Path.Combine(BasePath, "Data", "HeroGuides.json");
}
