namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Messaging;

/// <summary>
///     Pure-logic tests for the message-get response serialisation, verifying the full-message shape (including the body and metadata) that the game client parses.
/// </summary>
public sealed class MessageGetResponseTests_Unit
{
    [Test]
    public async Task Serialises_The_Full_Message_In_The_Shape_The_Client_Expects()
    {
        MessageGetResponse response = new ()
        {
            Data = new MessageDetail
            (
                subject: "Subject",
                body: "Body",
                image: "image.tga",
                read: false,
                expiration: 0,
                sent: 1700000000,
                crc: "abc123",
                deletable: true,
                metadata: new Dictionary<string, string> { ["body"] = "Body" }
            )
        };

        string serialised = PhpSerialization.Serialize(response);

        const string expected = @"a:2:{s:7:""success"";b:1;s:4:""data"";a:9:{s:7:""subject"";s:7:""Subject"";s:4:""body"";s:4:""Body"";s:5:""image"";s:9:""image.tga"";s:4:""read"";b:0;s:10:""expiration"";i:0;s:4:""sent"";i:1700000000;s:3:""crc"";s:6:""abc123"";s:9:""deletable"";b:1;s:4:""meta"";a:1:{s:4:""body"";s:4:""Body"";}}}";

        await Assert.That(serialised).IsEqualTo(expected);
    }
}
