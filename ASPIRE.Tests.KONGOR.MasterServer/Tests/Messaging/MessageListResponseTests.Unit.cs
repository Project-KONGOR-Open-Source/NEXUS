namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Messaging;

/// <summary>
///     Pure-logic tests for the message-list response serialisation, verifying the manifest entry shape that the game client parses.
/// </summary>
public sealed class MessageListResponseTests_Unit
{
    [Test]
    public async Task Serialises_The_Message_Manifest_In_The_Shape_The_Client_Expects()
    {
        MessageListResponse response = new ()
        {
            MessageManifest =
            [
                new MessageManifestEntry(subject: "Subject", image: "image.tga", read: false, expiration: 0, sent: 1700000000, crc: "abc123", deletable: true)
            ]
        };

        string serialised = PhpSerialization.Serialize(response);

        const string expected = @"a:2:{s:7:""success"";b:1;s:4:""data"";a:1:{i:0;a:7:{s:7:""subject"";s:7:""Subject"";s:5:""image"";s:9:""image.tga"";s:4:""read"";b:0;s:10:""expiration"";i:0;s:4:""sent"";i:1700000000;s:3:""crc"";s:6:""abc123"";s:9:""deletable"";b:1;}}}";

        await Assert.That(serialised).IsEqualTo(expected);
    }
}
