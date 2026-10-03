namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Messaging;

/// <summary>
///     Pure-logic tests for the message-delete response serialisation, verifying the shape the game client parses.
/// </summary>
public sealed class MessageDeleteResponseTests_Unit
{
    [Test]
    public async Task Serialises_The_Deletion_Outcome_In_The_Shape_The_Client_Expects()
    {
        string deleted = PhpSerialization.Serialize(new MessageDeleteResponse { Success = true });
        string notDeleted = PhpSerialization.Serialize(new MessageDeleteResponse { Success = false });

        using (Assert.Multiple())
        {
            await Assert.That(deleted).IsEqualTo(@"a:1:{s:7:""success"";b:1;}");
            await Assert.That(notDeleted).IsEqualTo(@"a:1:{s:7:""success"";b:0;}");
        }
    }
}
