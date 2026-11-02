namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests;

/// <summary>
///     Tests for the administration endpoints that add, edit, list, and delete the message of the day's events.
/// </summary>
public sealed class MessageOfTheDayAdministrationTests(ZORGATHIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Only_Elevated_Roles_May_Manage_Events()
    {
        AuthorizeAttribute? authorisation = typeof(MessageOfTheDayAdministrationController).GetCustomAttribute<AuthorizeAttribute>();

        await Assert.That(authorisation?.Policy).IsEqualTo(UserRoles.RolesWithElevatedPrivileges);
    }

    [Test]
    public async Task A_Picture_Of_Another_Size_Is_Refused()
    {
        IActionResult response = await CreateController().CreateEvent(NewEvent() with { Picture = Convert.ToBase64String(PNGHeader(640, 480)) });

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    public async Task A_GIF_Is_Refused()
    {
        IActionResult response = await CreateController().CreateEvent(NewEvent() with { Picture = Convert.ToBase64String([.. "GIF89a"u8, 0x20, 0x03, 0x84, 0x03, 0x00, 0x00, 0x00]) });

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    public async Task An_Oversized_Picture_Is_Refused()
    {
        byte[] oversized = [.. PNGHeader(800, 900), .. new byte[ContentLimits.PictureMaximumBytes]];

        IActionResult response = await CreateController().CreateEvent(NewEvent() with { Picture = Convert.ToBase64String(oversized) });

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    public async Task A_Body_With_A_Script_Is_Refused()
    {
        IActionResult response = await CreateController().CreateEvent(NewEvent() with { Body = "<p>Hello.</p><script>alert(1)</script>" });

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    public async Task A_Body_Using_Every_Allowed_Element_Is_Accepted()
    {
        const string body = """<p>Line one.<br>Line two, with <strong>strong</strong>, <em>emphasised</em>, <b>bold</b>, <i>italic</i>, and <u>underlined</u> text.</p><ul><li>One</li></ul><ol><li>Two</li></ol><p><a href="https://discord.gg/example">Discord</a> and <a href="http://example.com/">a page</a>.</p>""";

        IActionResult response = await CreateController().CreateEvent(NewEvent() with { Body = body });

        await Assert.That(response).IsTypeOf<CreatedAtActionResult>();
    }

    [Test]
    public async Task A_Link_With_A_Script_Scheme_Is_Refused()
    {
        IActionResult response = await CreateController().CreateEvent(NewEvent() with { Body = """<p><a href="javascript:alert(1)">Sign Up</a></p>""" });

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    public async Task An_Expiry_Not_After_The_Published_Time_Is_Refused()
    {
        IActionResult response = await CreateController().CreateEvent(NewEvent() with { Published = DateTimeOffset.UtcNow.AddDays(2), Expires = DateTimeOffset.UtcNow.AddDays(1) });

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    public async Task An_Expiry_In_The_Past_Is_Refused()
    {
        IActionResult response = await CreateController().CreateEvent(NewEvent() with { Published = DateTimeOffset.UtcNow.AddDays(-2), Expires = DateTimeOffset.UtcNow.AddMinutes(-1) });

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    public async Task Editing_Keeps_The_Identity_And_The_Published_Time()
    {
        MessageOfTheDayAdministrationController controller = CreateController();

        MessageOfTheDayEventDTO created = await Created<MessageOfTheDayEventDTO>(await controller.CreateEvent(NewEvent()));

        IActionResult edited = await controller.UpdateEvent(created.ID, NewEvent() with { Title = "Weekend Event Extended", Published = null });

        List<MessageOfTheDayEventDTO> listed = await Listed(await controller.GetEvents());

        using (Assert.Multiple())
        {
            await Assert.That(edited).IsTypeOf<OkObjectResult>();
            await Assert.That(listed.Single(item => item.ID == created.ID).Title).IsEqualTo("Weekend Event Extended");
            await Assert.That(listed.Single(item => item.ID == created.ID).Published).IsEqualTo(created.Published);
        }
    }

    [Test]
    public async Task Deleting_Removes_The_Item()
    {
        MessageOfTheDayAdministrationController controller = CreateController();

        MessageOfTheDayEventDTO created = await Created<MessageOfTheDayEventDTO>(await controller.CreateEvent(NewEvent()));

        IActionResult deleted = await controller.DeleteEvent(created.ID);

        using (Assert.Multiple())
        {
            await Assert.That(deleted).IsTypeOf<NoContentResult>();
            await Assert.That((await Listed(await controller.GetEvents())).Select(item => item.ID)).DoesNotContain(created.ID);
            await Assert.That(await controller.DeleteEvent(created.ID)).IsTypeOf<NotFoundResult>();
        }
    }

    private MessageOfTheDayAdministrationController CreateController()
    {
        IServiceScope scope = webApplicationFactory.Services.CreateScope();

        return new MessageOfTheDayAdministrationController(scope.ServiceProvider.GetRequiredService<IDatabase>(), scope.ServiceProvider.GetRequiredService<ILogger<MessageOfTheDayAdministrationController>>());
    }

    private static async Task<T> Created<T>(IActionResult response)
    {
        await Assert.That(response).IsTypeOf<CreatedAtActionResult>();

        return (T) ((CreatedAtActionResult) response).Value!;
    }

    private static async Task<List<MessageOfTheDayEventDTO>> Listed(IActionResult response)
    {
        await Assert.That(response).IsTypeOf<OkObjectResult>();

        return (List<MessageOfTheDayEventDTO>) ((OkObjectResult) response).Value!;
    }

    private static SaveMessageOfTheDayEventDTO NewEvent()
        => new ("Weekend Event", "double experience, Saturday and Sunday", "<p>Every match gives <strong>double experience</strong>.</p>", Convert.ToBase64String(PNGHeader(800, 900)), DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(3));

    private static byte[] PNGHeader(int width, int height)
        => [0x89, .. "PNG"u8, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, .. "IHDR"u8,
            (byte) (width >> 24), (byte) (width >> 16), (byte) (width >> 8), (byte) width,
            (byte) (height >> 24), (byte) (height >> 16), (byte) (height >> 8), (byte) height, 0x08, 0x06, 0x00, 0x00, 0x00];
}
