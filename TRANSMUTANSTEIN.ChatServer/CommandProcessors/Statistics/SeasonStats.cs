namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Statistics;

[ChatCommand(ChatProtocol.NET_CHAT_CL_TMM_CAMPAIGN_STATS)]
public class SeasonStats : ICommandProcessor
{
    public void Process(TCPSession session, ChatBuffer buffer)
    {
        /*
           m_fCampaignNormalTMR = phpResponse.GetFloat(_U8("normal_mmr"), 1250.0f);
           m_fCampaignCasualTMR = phpResponse.GetFloat(_U8("casual_mmr"), 1250.0f);
           m_uiCampaignNormalMedal = phpResponse.GetInteger(_U8("normal_medal"), 0);
           m_uiCampaignCasualMedal = phpResponse.GetInteger(_U8("casual_medal"), 0);

           m_iCampaignNormalKills = phpResponse.GetInteger(_U8("normal_kills"), 0);
           m_iCampaignCasualKills = phpResponse.GetInteger(_U8("casual_kills"), 0);
           m_iCampaignNormalDeaths = phpResponse.GetInteger(_U8("normal_deaths"), 0);
           m_iCampaignCasualDeaths = phpResponse.GetInteger(_U8("casual_deaths"), 0);

           uint normalWins = phpResponse.GetInteger(_U8("normal_wins"), 0);
           uint normalLoss = phpResponse.GetInteger(_U8("normal_loss"), 0);
           uint normalWinStreak = phpResponse.GetInteger(_U8("normal_win_streak"), 0);
           uint casualWins = phpResponse.GetInteger(_U8("casual_wins"), 0);
           uint casualLoss = phpResponse.GetInteger(_U8("casual_loss"), 0);
           uint casualWinStreak = phpResponse.GetInteger(_U8("casual_win_streak"), 0);

           m_iCampaignNormalMatches = phpResponse.GetInteger(_U8("normal_rank"), -1);
           m_iCampaignCasualMatches = phpResponse.GetInteger(_U8("casual_rank"), -1);

           uint normalPlacementNum = phpResponse.GetInteger(_U8("normal_placement_num"), 0);
           uint casualPlacementNum = phpResponse.GetInteger(_U8("casual_placement_num"), 0);

           wstring normalPlacementWins = phpResponse.GetWString(_U8("normal_placement_wins"));
           wstring casualPlacementWins = phpResponse.GetWString(_U8("casual_placement_wins"));

           m_bCampaignEligible = phpResponse.GetBool(_U8("can_enter"), false);
           m_bExclusiveChannelEligible = phpResponse.GetBool(_U8("can_enter_exclusive_channel"), false);
           bool validSeason = phpResponse.GetBool(_U8("valid_season"), false);
           
           m_bufferSend.Clear();

           m_bufferSend << NET_CHAT_CL_TMM_CAMPAIGN_STATS << 
           	m_fCampaignNormalTMR << m_uiCampaignNormalMedal << normalWins << normalLoss << normalWinStreak << m_iCampaignNormalMatches << normalPlacementNum << WStringToUTF8(normalPlacementWins) << byte('\0') <<
           	m_fCampaignCasualTMR << m_uiCampaignCasualMedal << casualWins << casualLoss << casualWinStreak << m_iCampaignCasualMatches << casualPlacementNum << WStringToUTF8(casualPlacementWins) << byte('\0') <<
           	(byte)m_bCampaignEligible << (byte)validSeason;
           	
           Send(m_bufferSend);
         */
    }
}
