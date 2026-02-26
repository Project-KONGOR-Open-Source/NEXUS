using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using TUnit.Core;

namespace ASPIRE.Tests.Lua;

public class LuaIntegrationTests : LuaTestBase
{
    public LuaIntegrationTests()
    {
        // Load all scripts to verify they load without crashing
        LoadAllScripts();
    }

    [Test]
    public async Task LoadAllScripts_ShouldLoadWithoutCriticalErrors()
    {
        object? mainTable = LuaState["Main"];
        await Assert.That(mainTable).IsNotNull();
    }

    [Test]
    public async Task SubmitForm_ShouldSendRequestToServer()
    {
        LuaState.DoString("SubmitForm('MyTestForm', 'f', 'test_submit', 'test_key', 'test_value')");

        await Assert.That(LastResponse).IsNotNull();
        await Assert.That(LastResponse.StatusCode).IsNotEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ClickButton_ShouldExecuteBoundCallback()
    {
        // Register a widget with a callback
        LuaState.DoString(@"
            local w = GetWidget('TestButton')
            w:SetCallback('onclick', function()
                SubmitForm('ClickedForm', 'f', 'clicked_f')
            end)
        ");

        // Verify that the callback is registered in C#
        await Assert.That(WidgetCallbacks.ContainsKey("TestButton")).IsTrue();

        // Act: Click the button
        ClickButton("TestButton");

        // Assert: SubmitForm should have been called
        await Assert.That(LastResponse).IsNotNull();
        // The last request should be for 'ClickedForm' logic.
        // We can inspect LastResponseContent if needed, or just that it fired.

        // Since we didn't mock the response for 'clicked_f', it might be anything.
        // But verifying LastResponse is not null after ClickButton implies the Lua callback ran.
        // To be safer, we can check if the response status is not 404 (client_requester exists).
        await Assert.That(LastResponse.StatusCode).IsNotEqualTo(HttpStatusCode.NotFound);
    }
}
