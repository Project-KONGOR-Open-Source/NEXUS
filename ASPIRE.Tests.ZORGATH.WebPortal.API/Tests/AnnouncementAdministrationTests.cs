namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests;

/// <summary>
///     Tests for the administration endpoints that add, edit, list, and delete the Project KONGOR Announcement Board's announcements.
/// </summary>
public sealed class AnnouncementAdministrationTests(ZORGATHIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Only_Elevated_Roles_May_Manage_Announcements()
    {
        AuthorizeAttribute? authorisation = typeof(AnnouncementAdministrationController).GetCustomAttribute<AuthorizeAttribute>();

        await Assert.That(authorisation?.Policy).IsEqualTo(UserRoles.RolesWithElevatedPrivileges);
    }

    [Test]
    public async Task A_Body_With_A_Script_Is_Refused()
    {
        IActionResult response = await CreateController().CreateAnnouncement(NewAnnouncement() with { Body = """<p onclick="alert(1)">Hello.</p>""" });

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    public async Task An_Expiry_In_The_Past_Is_Refused()
    {
        IActionResult response = await CreateController().CreateAnnouncement(NewAnnouncement() with { Published = DateTimeOffset.UtcNow.AddDays(-2), Expires = DateTimeOffset.UtcNow.AddMinutes(-1) });

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    public async Task Editing_Keeps_The_Identity_And_The_Published_Time()
    {
        AnnouncementAdministrationController controller = CreateController();

        AnnouncementDTO created = await Created(await controller.CreateAnnouncement(NewAnnouncement()));

        IActionResult edited = await controller.UpdateAnnouncement(created.ID, NewAnnouncement() with { Title = "Server Maintenance Postponed", Published = null });

        List<AnnouncementDTO> listed = await Listed(await controller.GetAnnouncements());

        using (Assert.Multiple())
        {
            await Assert.That(edited).IsTypeOf<OkObjectResult>();
            await Assert.That(listed.Single(item => item.ID == created.ID).Title).IsEqualTo("Server Maintenance Postponed");
            await Assert.That(listed.Single(item => item.ID == created.ID).Published).IsEqualTo(created.Published);
        }
    }

    [Test]
    public async Task A_Ninth_Unexpired_Announcement_Is_Refused()
    {
        AnnouncementAdministrationController controller = CreateController();

        List<AnnouncementDTO> created = [];

        for (int count = 0; count < ContentLimits.UnexpiredAnnouncementsMaximum; count++)
            created.Add(await Created(await controller.CreateAnnouncement(NewAnnouncement() with { Title = $"Announcement {count + 1}" })));

        IActionResult ninth = await controller.CreateAnnouncement(NewAnnouncement() with { Title = "One Too Many" });

        // Editing One Of The Eight Is Not A Ninth
        IActionResult edited = await controller.UpdateAnnouncement(created.First().ID, NewAnnouncement() with { Title = "Announcement One, Edited" });

        using (Assert.Multiple())
        {
            await Assert.That(ninth).IsTypeOf<ConflictObjectResult>();
            await Assert.That(edited).IsTypeOf<OkObjectResult>();
        }
    }

    [Test]
    public async Task Deleting_Removes_The_Item()
    {
        AnnouncementAdministrationController controller = CreateController();

        AnnouncementDTO created = await Created(await controller.CreateAnnouncement(NewAnnouncement()));

        IActionResult deleted = await controller.DeleteAnnouncement(created.ID);

        using (Assert.Multiple())
        {
            await Assert.That(deleted).IsTypeOf<NoContentResult>();
            await Assert.That((await Listed(await controller.GetAnnouncements())).Select(item => item.ID)).DoesNotContain(created.ID);
            await Assert.That(await controller.DeleteAnnouncement(created.ID)).IsTypeOf<NotFoundResult>();
        }
    }

    private AnnouncementAdministrationController CreateController()
    {
        IServiceScope scope = webApplicationFactory.Services.CreateScope();

        return new AnnouncementAdministrationController(scope.ServiceProvider.GetRequiredService<IDatabase>(), scope.ServiceProvider.GetRequiredService<ILogger<AnnouncementAdministrationController>>());
    }

    private static async Task<AnnouncementDTO> Created(IActionResult response)
    {
        await Assert.That(response).IsTypeOf<CreatedAtActionResult>();

        return (AnnouncementDTO) ((CreatedAtActionResult) response).Value!;
    }

    private static async Task<List<AnnouncementDTO>> Listed(IActionResult response)
    {
        await Assert.That(response).IsTypeOf<OkObjectResult>();

        return (List<AnnouncementDTO>) ((OkObjectResult) response).Value!;
    }

    private static SaveAnnouncementDTO NewAnnouncement()
        => new ("Server Maintenance", "<p>The servers restart on <strong>Sunday morning</strong>.</p><ul><li>Expect a short outage.</li></ul>", DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(3));
}
