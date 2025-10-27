namespace MERRICK.DatabaseContext.Data;

public static class DataFiles
{
    private static readonly string BasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

    public static readonly string HeroGuides = Path.Combine(BasePath, "Data", "HeroGuides.json");
}
