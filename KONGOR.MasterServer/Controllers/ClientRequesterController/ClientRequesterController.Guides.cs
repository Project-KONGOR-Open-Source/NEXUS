namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> GetGuideList()
    {
        string? accountID = Request.Form["account"];

        if (accountID is null)
            return BadRequest(@"Missing Value For Form Parameter ""account""");

        string? heroIdentifier = Request.Form["hero"];

        if (heroIdentifier is null)
            return BadRequest(@"Missing Value For Form Parameter ""hero""");

        string? hostTime = Request.Form["hosttime"];

        if (hostTime is null)
            return BadRequest(@"Missing Value For Form Parameter ""hosttime""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.Clan)
            .FirstOrDefaultAsync(account => account.ID.Equals(int.Parse(accountID)));

        if (account is null)
            return NotFound($@"Account With ID ""{accountID}"" Was Not Found");

        List<HeroGuide> guides = MerrickContext.HeroGuides
            .Include(guide => guide.Author).ThenInclude(record => record.Clan)
            .Where(guide => guide.HeroIdentifier.Equals(heroIdentifier))
            .Where(guide => guide.Public.Equals(true) || guide.Public.Equals(false) && guide.Author.ID.Equals(account.ID))
            .ToList();

        if (guides.None())
        {
            logger.LogError($@"No Guides Were Found For Hero Identifier ""{heroIdentifier}""");

            return NotFound($@"No Guides Were Found For Hero Identifier ""{heroIdentifier}""");
        }

        return Ok(PhpSerialization.Serialize(new GuideListResponse(guides, account, int.Parse(hostTime))));
    }

    private async Task<IActionResult> GetGuide()
    {
        string? accountID = Request.Form["account"];

        if (accountID is null)
            return BadRequest(@"Missing Value For Form Parameter ""account""");

        string? heroIdentifier = Request.Form["hero"];

        if (heroIdentifier is null)
            return BadRequest(@"Missing Value For Form Parameter ""hero""");

        string? hostTime = Request.Form["hosttime"];

        if (hostTime is null)
            return BadRequest(@"Missing Value For Form Parameter ""hosttime""");

        string? guideID = Request.Form["gid"];

        switch (guideID)
        {
            case null: return BadRequest(@"Missing Value For Form Parameter ""gid""");

            // A call for guide 99999 for the currently-selected hero is always made by the client when opening the guides for the first time every session. This call always returns an error response.
            case "99999": return Ok(PhpSerialization.Serialize(new GuideResponseError(int.Parse(hostTime))));
        }

        HeroGuide? guide = await MerrickContext.HeroGuides
            .Include(guide => guide.Author).ThenInclude(author => author.Clan)
            .FirstOrDefaultAsync(guide => guide.ID.Equals(int.Parse(guideID)));

        if (guide is null)
            return NotFound($"Guide ID {guideID} Was Not Found");

        return Ok(PhpSerialization.Serialize(new GuideResponseSuccess(guide, int.Parse(hostTime))));
    }
}
