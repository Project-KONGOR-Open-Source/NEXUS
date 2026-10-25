using System.Text.RegularExpressions;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Services;

/// <summary>
///     Tests for the wording of the match lock's notices: each calls the restriction a match lock, never a ban, and comes from the sender "Match Lock".
/// </summary>
public sealed class MatchLockMessageTests
{
    [Test]
    public async Task Notices_Call_The_Restriction_A_Match_Lock_And_Never_A_Ban()
    {
        string[] messages =
        [
            MatchLockService.SlotAccountMessage,
            MatchLockService.SlotAccountLoginMessage,
            MatchLockService.OtherAccountMessage("[TAG]Slot"),
            MatchLockService.RefusalLiftedMessage,
            MatchLockService.GroupMemberLockedMessage("[TAG]Member"),
            MatchLockService.InviteeLockedMessage("[TAG]Invitee")
        ];

        using (Assert.Multiple())
        {
            await Assert.That(MatchLockService.MessageSenderName).IsEqualTo("Match Lock");

            foreach (string message in messages)
            {
                await Assert.That(message.Contains("match lock", StringComparison.OrdinalIgnoreCase)).IsTrue();
                await Assert.That(Regex.IsMatch(message, @"\bban(ned|s|ning)?\b", RegexOptions.IgnoreCase)).IsFalse();
            }
        }
    }
}
