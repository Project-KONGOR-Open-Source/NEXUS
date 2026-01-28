using System.Globalization;

using KONGOR.MasterServer.Services.Requester;
// For PhpSerialization

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class GuideHandler(MerrickContext databaseContext, ILogger<GuideHandler> logger) : IClientRequestHandler
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "[Guide] GetGuideList Acc={AccountId} Hero={HeroIdentifier}")]
    private partial void LogGetGuideList(string? accountId, string? heroIdentifier);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No Guides Were Found For Hero Identifier \"{HeroIdentifier}\"")]
    private partial void LogNoGuidesFound(string heroIdentifier);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Guide] GetGuide ID={GuideId}")]
    private partial void LogGetGuide(string? guideId);

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        string? functionName = Request.Query["f"].FirstOrDefault() ?? Request.Form["f"].FirstOrDefault();

        if (functionName == "get_guide")
        {
            return await GetGuide(Request);
        }
        else
        {
            return await GetGuideList(Request);
        }
    }

    private async Task<IActionResult> GetGuideList(HttpRequest Request)
    {
        string? accountID = Request.Form["account"];
        string? heroIdentifier = Request.Form["hero"];
        string? hostTime = Request.Form["hosttime"];

        LogGetGuideList(accountID, heroIdentifier);

        if (accountID is null || heroIdentifier is null || hostTime is null)
        {
            return new BadRequestObjectResult("Missing Parameters");
        }

        if (!int.TryParse(accountID, NumberStyles.Integer, CultureInfo.InvariantCulture, out int accountIdInt))
        {
            return new BadRequestObjectResult("Invalid Account ID");
        }

        if (!int.TryParse(hostTime, NumberStyles.Integer, CultureInfo.InvariantCulture, out int hostTimeInt))
        {
            return new BadRequestObjectResult("Invalid Host Time");
        }

        Account? account = await MerrickContext.Accounts
            .Include(account => account.Clan)
            .FirstOrDefaultAsync(account => account.ID.Equals(accountIdInt));

        if (account is null)
        {
            return new NotFoundObjectResult($@"Account With ID ""{accountID}"" Was Not Found");
        }

        List<HeroGuide> guides = MerrickContext.HeroGuides
            .Include(guide => guide.Author).ThenInclude(record => record.Clan)
            .Where(guide => guide.HeroIdentifier.Equals(heroIdentifier))
            .Where(guide =>
                guide.Public.Equals(true) || (guide.Public.Equals(false) && guide.Author.ID.Equals(account.ID)))
            .ToList();

        if (!guides.Any())
        {
            LogNoGuidesFound(heroIdentifier);
            return new NotFoundObjectResult($@"No Guides Were Found For Hero Identifier ""{heroIdentifier}""");
        }

        return new OkObjectResult(PhpSerialization.Serialize(new GuideListResponse(guides, account, hostTimeInt)));
    }

    private async Task<IActionResult> GetGuide(HttpRequest Request)
    {
        string? accountID = Request.Form["account"];
        string? heroIdentifier = Request.Form["hero"];
        string? hostTime = Request.Form["hosttime"];
        string? guideID = Request.Form["gid"];

        LogGetGuide(guideID);

        if (accountID is null || heroIdentifier is null || hostTime is null || guideID is null)
        {
            return new BadRequestObjectResult("Missing Parameters");
        }

        if (!int.TryParse(hostTime, NumberStyles.Integer, CultureInfo.InvariantCulture, out int hostTimeInt))
        {
            return new BadRequestObjectResult("Invalid Host Time");
        }

        if (guideID == "99999")
        {
            return new OkObjectResult(PhpSerialization.Serialize(new GuideResponseError(hostTimeInt)));
        }

        if (!int.TryParse(guideID, NumberStyles.Integer, CultureInfo.InvariantCulture, out int guideIdInt))
        {
            return new BadRequestObjectResult("Invalid Guide ID");
        }

        HeroGuide? guide = await MerrickContext.HeroGuides
            .Include(guide => guide.Author).ThenInclude(author => author.Clan)
            .FirstOrDefaultAsync(guide => guide.ID.Equals(guideIdInt));

        if (guide is null)
        {
            return new NotFoundObjectResult($"Guide ID {guideID} Was Not Found");
        }

        return new OkObjectResult(PhpSerialization.Serialize(new GuideResponseSuccess(guide, hostTimeInt)));
    }
}
