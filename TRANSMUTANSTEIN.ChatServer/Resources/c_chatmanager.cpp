// (C)2007 S2 Games
// c_chatmanager.cpp
//
//=============================================================================

//=============================================================================
// Headers
//=============================================================================
#include "k2_common.h"

#include "c_chatmanager.h"

#include "c_xmldoc.h"
#include "c_uicmd.h"
#include "c_uitrigger.h"
#include "c_uimanager.h"
#include "c_soundmanager.h"
#include "c_stringtable.h"
#include "c_hostclient.h"
#include "c_date.h"
#include "c_uicmdregistry.h"
#include "c_phpdata.h"
#include "c_httpmanager.h"
#include "c_httprequest.h"
#include "c_resourcemanager.h"
#include "c_webbrowsermanager.h"
#include "c_system.h"
#include "c_crashreporter.h"
#include "c_luascriptmanager.h"
#include "c_luatable.h"
#include "c_logcollector.h"
#include "md5.h"
#include <time.h>
//=============================================================================

//=============================================================================
// Definitions
//=============================================================================
CChatManager*	pChatManager(CChatManager::GetInstance());
SINGLETON_INIT(CChatManager)

UI_TRIGGER(ChatStatus);
UI_TRIGGER(ChatUpdateName);

UI_TRIGGER(ChatChanNumUsers);
UI_TRIGGER(ChatChanTopic);
UI_TRIGGER(ChatUserNames);
UI_TRIGGER(ChatUserEvent);

UI_TRIGGER(ChatBuddyOnline);
UI_TRIGGER(ChatBuddyOffline);
UI_TRIGGER(ChatBuddyGame);
UI_TRIGGER(ChatBuddyEvent);
UI_TRIGGER(ChatBuddyStatusChanged);

UI_TRIGGER(ChatClanOnline);
UI_TRIGGER(ChatClanOffline);
UI_TRIGGER(ChatClanGame);
UI_TRIGGER(ChatClanEvent);
//UI_TRIGGER(ChatClanMemberStatusChanged);

UI_TRIGGER(ChatCompanion);
UI_TRIGGER(ChatCompanionEvent);

UI_TRIGGER(ChatNewChannel);
UI_TRIGGER(ChatChannelList);
UI_TRIGGER(ChatLeftChannel);

UI_TRIGGER(ChatNewGame);
UI_TRIGGER(ChatLeftGame);

UI_TRIGGER(ChatNotificationBuddy);
UI_TRIGGER(ChatNotificationClan);
UI_TRIGGER(ChatNotificationMessage);
UI_TRIGGER(ChatNotificationInvite);
UI_TRIGGER(ChatNotificationGroupInvite);
UI_TRIGGER(ChatNotificationHistoryPerformCMD);

UI_TRIGGER(ChatWhisperUpdate);
UI_TRIGGER(ChatClanWhisperUpdate);

UI_TRIGGER(ChatCloseIM);

UI_TRIGGER(ChatLookingForClanEvent);

UI_TRIGGER(ChatHoverName);
UI_TRIGGER(ChatHoverClan);
UI_TRIGGER(ChatHoverServer);
UI_TRIGGER(ChatHoverGameTime);

UI_TRIGGER(ChatRecentlyPlayedPlayer);
UI_TRIGGER(ChatRecentlyPlayedHeader);
UI_TRIGGER(ChatRecentlyPlayedEvent);

UI_TRIGGER(ChatAutoCompleteClear);
UI_TRIGGER(ChatAutoCompleteAdd);

UI_TRIGGER(ChatCloseNotifications);

UI_TRIGGER(ChatClanInvite);

UI_TRIGGER(ChatClanCreateFail);
UI_TRIGGER(ChatClanCreateSuccess);
UI_TRIGGER(ChatClanCreateAccept);
UI_TRIGGER(ChatClanCreateTime);

UI_TRIGGER(ChatClanCreateTip);

UI_TRIGGER(ChatShowPostGameStats);

UI_TRIGGER(ChatTotalFriends);
UI_TRIGGER(ChatOnlineFriends);

UI_TRIGGER(ChatTotalClanMembers);

UI_TRIGGER(ChatUsersOnline);

UI_TRIGGER(ChatNumUnreadChannels);
UI_TRIGGER(ChatUnreadChannel);

UI_TRIGGER(ChatSetFocusChannel);
UI_TRIGGER(ChatFocusedIM);
UI_TRIGGER(ChatUnreadIM);

UI_TRIGGER(ChatReceivedIMCount);
UI_TRIGGER(ChatReadIMCount);
UI_TRIGGER(ChatUnreadIMCount);
UI_TRIGGER(ChatSentIMCount);
UI_TRIGGER(ChatOpenIMCount);
UI_TRIGGER(ChatNotificationCount);

UI_TRIGGER(ChatUserStatus);

UI_TRIGGER(ChatRecievedChannelMessage);

UI_TRIGGER(TMMDebugInfo);
UI_TRIGGER(TMMTime);
UI_TRIGGER(TMMBanTime);
UI_TRIGGER(TMMDisplay);
UI_TRIGGER(TMMDisplayPopularity);

UI_TRIGGER(TMMReset);
UI_TRIGGER(TMMFoundMatch);
UI_TRIGGER(TMMFoundServer);
UI_TRIGGER(TMMOptionsAvailable);
UI_TRIGGER(TMMAvailable);
UI_TRIGGER(TMMReadyStatus);
UI_TRIGGER(TMMJoinGroup);
UI_TRIGGER(TMMLeaveGroup);
UI_TRIGGER(TMMJoinQueue);
UI_TRIGGER(TMMLeaveQueue);
UI_TRIGGER(TMMJoinMatch);
UI_TRIGGER(TMMNoMatchesFound);
UI_TRIGGER(TMMNoServersFound);

UI_TRIGGER(TMMNewPlayerStatus);
UI_TRIGGER(TMMPlayerStatus0);
UI_TRIGGER(TMMPlayerStatus1);
UI_TRIGGER(TMMPlayerStatus2);
UI_TRIGGER(TMMPlayerStatus3);
UI_TRIGGER(TMMPlayerStatus4);

UI_TRIGGER(TMMTeamBotChange);
UI_TRIGGER(TMMEnemyBotChange);
UI_TRIGGER(TMMStartLocalBotMatch);
UI_TRIGGER(TMMNoBotsSelected);

UI_TRIGGER(TMMRequestReadyUp);
UI_TRIGGER(TMMMatchPendingAccept);
UI_TRIGGER(TMMMatchFailedToAccept);
UI_TRIGGER(ChatDynamicProductListUpdate);

UI_TRIGGER(AllChatMessages);
UI_TRIGGER(RestrictedRegions);

UI_TRIGGER(DynamicCommandsExecuted);

CUITrigger* TMMPlayerStatus[] =
{
	&TMMPlayerStatus0,
	&TMMPlayerStatus1,
	&TMMPlayerStatus2,
	&TMMPlayerStatus3,
	&TMMPlayerStatus4
};

UI_TRIGGER(ChatRequestGameInfo);
UI_TRIGGER(EventListing);
UI_TRIGGER(ScheduledMatchListing);
UI_TRIGGER(ScheduledMatchInfo);
UI_TRIGGER(ScheduledMatchSpectatorInfo);
UI_TRIGGER(ScheduledMatchTime);
UI_TRIGGER(ScheduledMatchLeave);
UI_TRIGGER(ScheduledMatchServerInfo);

UI_TRIGGER(GameMatchID);

UI_TRIGGER(UploadReplayStatus);
UI_TRIGGER(MessagesUpdated);
UI_TRIGGER(MessageDeleted);
UI_TRIGGER(MessagesEnabled);

UI_TRIGGER(SpecialMessagesUpdated);

UI_TRIGGER(RankInfoUpdated);

UI_TRIGGER(LeaveStrikeWarning);

UI_TRIGGER(ChatServerMapSettingPhaseEndTime);
UI_TRIGGER(ChatServerMapSettingChanged);

#if defined(K2_CHINESE)
CVAR_BOOL(		cc_enableChatConnection,				false);
#else
CVAR_BOOL(		cc_enableChatConnection,				true);
#endif

CVAR_BOOLF(		cc_showBuddyConnectionNotification,		true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showClanConnectionNotification,		true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showBuddyDisconnectionNotification,	true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showClanDisconnectionNotification,	true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showBuddyJoinGameNotification,		true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showClanJoinGameNotification,		true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showBuddyLeaveGameNotification,		true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showClanLeaveGameNotification,		true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showBuddyRequestNotification,	true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showBuddyAddNotification,		true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showBuddyRemovedNotification,	true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showClanRankNotification,		true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showClanAddNotification,			true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showClanRemoveNotification,		true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showClanMessageNotification,		true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showIMNotification,				true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showGameInvites,					true,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_showNewPatchNotification,		true,			CVAR_SAVECONFIG);

CVAR_UINTF(		cc_notificationDuration,			10,				CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_DisableNotifications,			false,			CVAR_SAVECONFIG);
CVAR_BOOLF(		cc_DisableNotificationsInGame,		false,			CVAR_SAVECONFIG);

CVAR_UINTF(		chat_gameLobbyChatToggle,			274,			CVAR_SAVECONFIG);	// default to the value of the alt key when pressed (274)

CVAR_BOOLF(		cg_censorChat,						true,			CVAR_SAVECONFIG | OPTION_CLOUDCONFIG);

CVAR_UINTF(		chat_connectTimeout,				3,				CVAR_SAVECONFIG);

CVAR_STRINGF(	cc_curGameChannel,					"",				CVAR_SAVECONFIG);
CVAR_UINT(		cc_curGameChannelID,				~0);

CVAR_BOOL(		cc_forceTMMInterfaceUpdate,			false);
CVAR_UINTF(		cc_TMMMatchFidelity,				0,				CVAR_SAVECONFIG);
CVAR_BOOL(		cc_printTMMUpdates,					false);
CVAR_BOOL(		cc_printPendingMatch,				false);

ARRAY_CVAR_UINT(cc_mmrConfig, _T("1250,1275,1300,1330,1360,1400,1440,1480,1520,1560,1600,1650,1700,1750,1800,1850,1950"));
ARRAY_CVAR_UINT(cc_mmrConfigAfterS6, _T("1250,1275,1300,1330,1360,1400,1435,1470,1505,1540,1575,1610,1645,1685,1725,1765,1805,1850,1900,1950")); // effective from Season 7

#if 0
//CVAR_STRING(	chat_serverAddrOverride,			"64.20.203.130");
CVAR_STRING(	chat_serverAddrOverride,			"192.168.1.105");
CVAR_UINT(		chat_serverPortOverride,			11031);
#else
CVAR_STRING(	chat_serverAddrOverride,			"");
CVAR_UINT(		chat_serverPortOverride,			0);
#endif

#if defined(K2_GARENA)
	#ifdef K2_RELEASE_CANDIDATE
const uint CHAT_SERVERPORT(11034);
	#else
const uint CHAT_SERVERPORT(11033);
	#endif
#elif defined(K2_CHINESE)
	#ifdef K2_RELEASE_CANDIDATE
const uint CHAT_SERVERPORT(11037);
	#else
const uint CHAT_SERVERPORT(11035);
	#endif
#elif defined(K2_KOREAN)
	#ifdef K2_RELEASE_CANDIDATE
const uint CHAT_SERVERPORT(11034);
	#else
const uint CHAT_SERVERPORT(11033);
	#endif
#elif defined(K2_LATIN_AMERICA)
	#ifdef K2_RELEASE_CANDIDATE
const uint CHAT_SERVERPORT(11034);
	#else
const uint CHAT_SERVERPORT(11033);
	#endif
#else
	#ifdef K2_TEST
const uint CHAT_SERVERPORT(11032);
	#elif defined(K2_STABLE)
const uint CHAT_SERVERPORT(11031);
	#elif defined(K2_RELEASE_CANDIDATE)
const uint CHAT_SERVERPORT(11038);
	#else
const uint CHAT_SERVERPORT(11037);
	#endif
#endif

CVAR_BOOL(		chat_profile,						false);
CVAR_BOOL(		chat_debugInterface,				false);
CVAR_BOOL(		chat_debugInterface2,				false);

CVAR_UINTF(		chat_maxReconnectAttempts,			5,				CVAR_SAVECONFIG);

CVAR_BOOLF(		chat_showChatTimestamps,			false,			CVAR_SAVECONFIG | OPTION_CLOUDCONFIG);

CVAR_BOOL(		_testNoData,						false);
CVAR_BOOL(		_testHonTour,						false);

CVAR_BOOL(		cc_playChatSounds,					false);

EXTERN_CVAR_STRING(host_language);
EXTERN_CVAR_BOOL(man_debugOnDemandUploads);
EXTERN_CVAR_UINT(cl_packetResendTime);

#if !defined(K2_STABLE)
CVAR_BOOLF	(debug_forceCrash,							false,			CONEL_DEV);
#endif


static const tstring LOCALHOST(_T("127.0.0.1"));
//=============================================================================

/*====================
  CChatManager::CChatManager
  ====================*/
CChatManager::CChatManager() :
m_pHTTPManager(NULL),

m_cDate(CDate(true)),
m_uiConnectTimeout(INVALID_TIME),
m_uiNextReconnectTime(INVALID_TIME),
m_uiIgnoreChat(CHAT_IGNORE_NONE),
m_bFriendlyChat(false),
m_bBuddyUpdateRequired(false),
m_bClanUpdateRequired(false),
m_bCafeUpdateRequired(false),
m_bOnDemandS3Available(false),
m_bOnDemandFTPAvailable(false),
m_uiConnectRetries(0),
m_uiAccountID(INVALID_ACCOUNT),
m_hStringTable(INVALID_RESOURCE),
m_bPrivateGame(false),
m_bHost(false),
m_bWhisperMode(false),
m_uiTabNumber(0),
m_bRetrievingStats(false),
m_pRecentlyPlayed(NULL),
m_uiMatchID(-1),
m_yArrangedType(0),
m_uiHistoryPos(0),
m_uiCreateTimeSent(INVALID_TIME),
m_bFollow(false),
m_bMatchStarted(false),
m_bWaitingToShowStats(false),
m_uiShowStatsMatchID(INVALID_INDEX),
m_uiFocusedChannel(-1),
m_uiFocusCount(0),
m_uiLastIMNotificationTime(0),
m_uiReceivedIMCount(0),
m_uiReadIMCount(0),
m_uiSentIMCount(0),
m_uiNotificationIndex(0),
m_eStatus(CHAT_CLIENT_STATUS_DISCONNECTED),
m_bCanBeReferred(false),

m_uiTMMBanTime(INVALID_TIME),
m_uiTMMStartTime(INVALID_TIME),
m_uiTMMAverageQueueTime(INVALID_TIME),
m_bInGroup(false),
m_bTMMEnabled(false),

m_uiTMMGroupLeaderID(INVALID_INDEX),
m_bTMMOtherPlayersReady(false),
m_bTMMAllPlayersReady(false),
m_yGameType(0),
m_yArrangedMatchType(AM_MATCHMAKING),
m_uiTMMSelfGroupIndex(0),

m_uiLastPendingMatchUpdate(INVALID_TIME),
m_uiPendingMatchTimeEnd(0),
m_uiPendingMatchLength(0),
m_uiPendingMatchNumPlayers(0),
m_uiPendingMatchNumPlayersAccepted(0),
m_bAcceptedPendingMatch(false),
m_bHadPendingMatchLastUpdate(false),

m_bFinishedList(false),
m_yListStartSequence(0),
m_yProcessingListSequence(0xff),
m_yFinishedListSequence(0xff),
m_bInGameLobby(false),

m_uiNextTimerUpdateTime(INVALID_TIME),
m_yAntiAddictionBenefit(EEGPB_NORMAL),
m_bSMMapLoaded(false),
m_bSpectatingScheduledMatch(false),
m_uiScheduledMatchID(0),
m_uiChatMuteExpiration(0),
m_bMessagesEnabled(true),
m_bSpecialMessagesPopup(true),
m_uiMapSettingPhaseEndTime(0),
m_bMapSettingChangeTriggered(false)
{
}


/*====================
  CChatManager::~CChatManager
  ====================*/
CChatManager::~CChatManager()
{
	if (m_uiAccountID == INVALID_ACCOUNT || m_eStatus > CHAT_CLIENT_STATUS_CONNECTED)
		LeftGame();

	Disconnect();

	K2_DELETE(m_pRecentlyPlayed);
}


/*====================
  CChatManager::AddGameChatMessage
  ====================*/
void	CChatManager::AddGameChatMessage(EChatMessageType eType, const tstring& sMessage, const tstring& sHeroEntityName, bool bSelfMessage, const tstring& sSound)
{
	PROFILE("AddGameChatMessage");

	if (bSelfMessage && IsChatMuted() && !(eType == CHAT_MESSAGE_MENTOR))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_muted")));
		return;
	}

	tstring sMsg(sMessage);
	tstring sFormattedMessage;
	
	if (cg_censorChat)
		CensorChat(sMsg, false);
		
	if (chat_showChatTimestamps)
		m_cDate = CDate(true);
	
	switch (eType)
	{
	case CHAT_MESSAGE_CLEAR:
		break;
	
	/* case CHAT_MESSAGE_ROLL:
		if (chat_showChatTimestamps)
			sFormattedMessage = _T("[") + m_cDate.GetTimeString(TIME_NO_SECONDS) + _T("] ^190") + sMsg + _T("^*");
		else
			sFormattedMessage = _T("^190") + sMsg + _T("^*");
		break; */

	case CHAT_MESSAGE_EMOTE:
		if (chat_showChatTimestamps)
			sFormattedMessage = _T("[") + m_cDate.GetTimeString(TIME_NO_SECONDS) + _T("] ^839") + sMsg + _T("^*");
		else
			sFormattedMessage = _T("^839") + sMsg + _T("^*");
		break;
		
	case CHAT_MESSAGE_IRC:
	case CHAT_MESSAGE_GAME:
	case CHAT_MESSAGE_GAME_IMPORTANT:
	case CHAT_MESSAGE_TEAM:
	case CHAT_MESSAGE_MENTOR:
	case CHAT_MESSAGE_ALL:	
	case CHAT_MESSAGE_SERVER:
	case CHAT_MESSAGE_LOCAL:
	default:
		if (chat_showChatTimestamps)
			sFormattedMessage = _T("[") + m_cDate.GetTimeString(TIME_NO_SECONDS) + _T("] ") + sMsg;
		else
			sFormattedMessage = sMsg;
		break;
	}

	//the definition of unformatted being what Ben told me... (removing all, team, mentor, etc)
	size_t zPos(sMessage.find_first_of(_CTS("^"), 1));

	tstring sUnformatedString;

	if (zPos != tstring::npos)
		sUnformatedString = sMessage.substr(zPos, sMessage.length());
	else
		sUnformatedString = sMessage;

	static tsvector vTriggerParams(6);
	vTriggerParams[0] = XtoA(eType);		// Type
	vTriggerParams[1] = _CTS("");			// Channel
	vTriggerParams[2] = sFormattedMessage;	// Message
	vTriggerParams[3] = sHeroEntityName;	// Hero_Entity
	vTriggerParams[4] = sUnformatedString;			// unformated original message
	vTriggerParams[5] = XtoA(bSelfMessage);         // is the message the one the user just typed
	AllChatMessages.Trigger(vTriggerParams);
	
	for (ChatWidgetReference_it it(m_vWidgets.begin()); it != m_vWidgets.end(); it++)
	{
		if (!it->bGameChat || !it->refWidget.IsValid())
			continue;
		
		switch (eType)
		{
			case CHAT_MESSAGE_CLEAR:
				static_cast<CTextBuffer*>(it->refWidget.GetTarget())->ClearText();
				break;
				
			case CHAT_MESSAGE_IRC:
			case CHAT_MESSAGE_GAME:
			case CHAT_MESSAGE_GAME_IMPORTANT:
			case CHAT_MESSAGE_TEAM:
			case CHAT_MESSAGE_MENTOR:
			case CHAT_MESSAGE_ALL:
			// case CHAT_MESSAGE_ROLL:
			case CHAT_MESSAGE_EMOTE:
			case CHAT_MESSAGE_SERVER:
			case CHAT_MESSAGE_LOCAL:
			default:
				static_cast<CTextBuffer*>(it->refWidget.GetTarget())->AddText(sFormattedMessage, Host.GetTime());
				break;
		}
	}

	if (!sSound.empty() && cc_playChatSounds)
		PlaySound(sSound);
}


/*====================
  CChatManager::AddIRCChatMessage
  ====================*/
void	CChatManager::AddIRCChatMessage(EChatMessageType eType, const tstring& sMessage, const tstring& sChannel, bool bTimeStamp, bool bSelfMessage)
{
	tstring sMsg(sMessage);
	tstring sFormattedMessage;
	
	if (cg_censorChat)
		CensorChat(sMsg, false);
		
	m_cDate = CDate(true);

	switch (eType)
	{
	case CHAT_MESSAGE_CLEAR:
		break;
		
	/* case CHAT_MESSAGE_ROLL:
		sFormattedMessage = (bTimeStamp ? _T("^190[") + m_cDate.GetTimeString(TIME_NO_SECONDS) + _T("] ") + sMsg + _T("^*") : sMsg);
		break; */

	case CHAT_MESSAGE_EMOTE:
		sFormattedMessage = (bTimeStamp ? _T("^839[") + m_cDate.GetTimeString(TIME_NO_SECONDS) + _T("] ") + sMsg + _T("^*") : sMsg);
		break;

	case CHAT_MESSAGE_IRC:
	case CHAT_MESSAGE_WHISPER:
	case CHAT_MESSAGE_WHISPER_BUDDIES:
	case CHAT_MESSAGE_GLOBAL:
	case CHAT_MESSAGE_CLAN:
	case CHAT_MESSAGE_SYSTEM:
	default:
		sFormattedMessage = (bTimeStamp ? _T("^770[") + m_cDate.GetTimeString(TIME_NO_SECONDS) + _T("] ") + sMsg : sMsg);
		break;
	}

	size_t zPos(sMessage.find_first_of(_CTS("^"), 1));

	tstring sUnformatedString;

	if (zPos != tstring::npos)
		sUnformatedString = sMessage.substr(zPos, sMessage.length());
	else
		sUnformatedString = sMessage;

	static tsvector vTriggerParams(6);
	vTriggerParams[0] = XtoA(eType);		// Type
	vTriggerParams[1] = sChannel;			// Channel
	vTriggerParams[2] = sFormattedMessage;	// Message
	vTriggerParams[3] = TSNULL;				// Hero_Entity
	vTriggerParams[4] = sUnformatedString;	// unformated original message
	vTriggerParams[5] = XtoA(bSelfMessage);       // is the message the one the user just typed
	AllChatMessages.Trigger(vTriggerParams);

	for (ChatWidgetReference_it it(m_vWidgets.begin()); it != m_vWidgets.end(); it++)
	{
		if ((!sChannel.empty() && CompareNoCase(sChannel, it->sChannel) != 0) || (!sChannel.empty() && it->bGameChat) || !it->refWidget.IsValid())
			continue;
		
		switch (eType)
		{
			case CHAT_MESSAGE_CLEAR:
				static_cast<CTextBuffer*>(it->refWidget.GetTarget())->ClearText();
				break;

			case CHAT_MESSAGE_GROUP:
				if (!it->sChannel.empty() && it->sChannel.substr(0, 6) == _T("Group "))
				{
					static_cast<CTextBuffer*>(it->refWidget.GetTarget())->AddText(sFormattedMessage, Host.GetTime());
					return;
				}
				break;

			case CHAT_MESSAGE_IRC:
			// case CHAT_MESSAGE_ROLL:
			case CHAT_MESSAGE_EMOTE:
			case CHAT_MESSAGE_WHISPER:
			case CHAT_MESSAGE_WHISPER_BUDDIES:
			case CHAT_MESSAGE_GLOBAL:
			case CHAT_MESSAGE_CLAN:
			case CHAT_MESSAGE_SYSTEM:
			default:
				static_cast<CTextBuffer*>(it->refWidget.GetTarget())->AddText(sFormattedMessage, Host.GetTime());
				break;
		}
	}
}

/*====================
  CChatManager::SetCurrentChatMessage
  ====================*/
tstring		CChatManager::SetCurrentChatMessage(const tstring& sMessage)
{
	// always reply directly to the sender, not to the buddy group or clan
	if (m_lLastWhispers.size() > 0 && (CompareNoCase(sMessage, Translate(_T("chat_command_reply")) + _T(" ")) == 0 || CompareNoCase(sMessage, Translate(_T("chat_command_reply_short")) + _T(" ")) == 0))
		m_sCurrentMessage = Translate(_T("chat_command_whisper_short")) + _T(" ") + m_lLastWhispers.front() + _T(" ");
	else
		m_sCurrentMessage = sMessage;

	tstring sTempString = LowerString(m_sCurrentMessage);
	
	//Set WhisperMode
	if (m_lLastWhispers.size() > 0 && sTempString.find(Translate(_T("chat_command_whisper_short"))) == 0)
	{
		m_bWhisperMode = true;
	}
	else
	{
		m_bWhisperMode = false;
		m_uiTabNumber = 0;
	}

	return m_sCurrentMessage;
}


/*====================
  CChatManager::Connect
  ====================*/
void	CChatManager::Connect(bool bInvisible)
{
	if (!cc_enableChatConnection)
		return;

	if (m_uiAccountID == INVALID_ACCOUNT || m_eStatus != CHAT_CLIENT_STATUS_DISCONNECTED)
	{
		Console << _T("Invalid account or not disconnected: ") << m_uiAccountID << SPACE << m_eStatus << newl;
		return;
	}

	if (m_sCookie.empty())
	{
		Console << _T("Cookie empty: ") << m_sCookie << newl;
		return;
	}

	if (!m_sockChat.IsInitialized())
		m_sockChat.Init();

	if (m_sockChat.IsConnected())
		HandleDisconnect();

	if (m_uiConnectRetries == 0 || m_uiConnectRetries == -1)
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_connecting"));
	else
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_reconnecting", L"attempt", XtoW(m_uiConnectRetries), L"maxattempts", XtoW(chat_maxReconnectAttempts)));
		
	// set the clients chat mode type to be 'available' upon connecting, or set it to be 'invisible'
	if (bInvisible)
		SetChatModeType(CHAT_MODE_INVISIBLE, WSNULL, true);
	else
		SetChatModeType(CHAT_MODE_AVAILABLE, WSNULL, true);
	
	LeaveTMMGroup(true);

	// Always reset the TMM screen so it isn't stuck looking like we were in a group
	TMMReset.Trigger(TSNULL, cc_forceTMMInterfaceUpdate);

	m_mapChannels.clear();

	m_mapIMs.clear();
	m_mapIMUnreadCount.clear();
	
	m_uiReceivedIMCount= 0;
	m_uiReadIMCount = 0;
	m_uiSentIMCount = 0;		

	if (!m_sockChat.SetSendAddress(u8string(chat_serverAddrOverride.empty() ? m_sChatAddress : chat_serverAddrOverride), chat_serverPortOverride != 0 ? chat_serverPortOverride : CHAT_SERVERPORT))
	{
		Console << _T("SetSendAddr failed") << newl;
		return;
	}

	m_eStatus = CHAT_CLIENT_STATUS_CONNECTING;
	m_uiConnectTimeout = Host.GetTimeSeconds() + chat_connectTimeout;
	m_uiNextReconnectTime = INVALID_TIME;

	if (m_uiConnectRetries == -1)
		m_uiConnectRetries = 0;

	tsvector vMiniParams(2);

	vMiniParams[0] = _T("irc_status_chan");
	vMiniParams[1] = K2System.GetGameName() + _T(" - v") + K2_Version(K2System.GetVersionString());
	ChatChanTopic.Trigger(vMiniParams);

	SetFocusedChannel(uint(-9001));
	SetFocusedChannel(uint(-1));
}


/*====================
  CChatManager::SetInfo
  ====================*/
void	CChatManager::SetInfo(int iAccountID, const tstring& sCookie, const tstring& sNickname, const tstring& sClan, const tstring& sClanTag, int iClanID, EClanRank eClanRank, byte yFlags, uint uiChatSymbol, uint uiChatNameColor, uint uiAccountIcon, int iAccountIconSlot, const wstring& sRemoteIP, const wstring& sAuthHash)
{
	if (m_uiAccountID != INVALID_ACCOUNT)
	{
		m_setRecentlyPlayed.clear();
		m_pRecentlyPlayed->EndNode();

		m_mapUserList.clear();
		m_mapNameToAccountID.clear();
		m_setBuddyList.clear();
		m_setChannelsIn.clear();
		m_mapBanList.clear();
		m_mapIgnoreList.clear();
		m_mapChannels.clear();
		m_setClanList.clear();
		m_setAutoJoinChannels.clear();
	}

	bool bTraversed = m_pRecentlyPlayed->TraverseChildren();
	bool bFound = false;

	if (bTraversed)
	{
		bFound = (m_pRecentlyPlayed->GetProperty(_T("id")) == XtoA(iAccountID));

		while (!bFound && m_pRecentlyPlayed->TraverseNextChild())
			bFound = (m_pRecentlyPlayed->GetProperty(_T("id")) == XtoA(iAccountID));
	}

	if (!bFound)
	{
		if (bTraversed)
			m_pRecentlyPlayed->EndNode();

		m_pRecentlyPlayed->NewNode(_T("user"));
		m_pRecentlyPlayed->AddProperty(_T("id"), XtoA(iAccountID));
		m_pRecentlyPlayed->AddProperty(_T("name"), sNickname);
	}

	m_uiAccountID = iAccountID;
	m_sCookie = sCookie;
	m_sRemoteIP = sRemoteIP;
	m_sAuthHash = sAuthHash;

	ChatClientMap_it findit(m_mapUserList.find(m_uiAccountID));

	if (findit == m_mapUserList.end())
	{
		SChatClient structClient;
		structClient.uiAccountID = iAccountID;
		findit = m_mapUserList.insert(ChatClientPair(iAccountID, structClient)).first;
		m_mapNameToAccountID.insert(pair<wstring, uint>(LowerString(StripClanTag(sNickname)), iAccountID));
	}

	SChatClient& ChatClient(findit->second);

	ChatClient.sName = sNickname;
	ChatClient.iClanID = iClanID;
	ChatClient.sClan = sClan;
	ChatClient.sClanTag = sClanTag;
	ChatClient.yFlags |= yFlags;
	ChatClient.uiChatSymbol = uiChatSymbol;
	ChatClient.uiChatNameColor = uiChatNameColor;
	ChatClient.uiAccountIcon = uiAccountIcon;
	ChatClient.iAccountIconSlot = iAccountIconSlot;

	uint uiChatNameColor2(uiChatNameColor);

	if (yFlags & CHAT_CLIENT_IS_STAFF && uiChatNameColor2 == INVALID_INDEX)
	{
		uint uiDevChatNameColor(Host.LookupChatNameColor(_CTS("s2logo")));
		if (uiDevChatNameColor != INVALID_INDEX)
			uiChatNameColor2 = uiDevChatNameColor;
	}
	if (yFlags & CHAT_CLIENT_IS_PREMIUM && uiChatNameColor2 == INVALID_INDEX)
	{
		uint uiGoldChatNameColor(Host.LookupChatNameColor(_CTS("goldshield")));
		if (uiGoldChatNameColor != INVALID_INDEX)
			uiChatNameColor2 = uiGoldChatNameColor;
	}

	if (uiChatNameColor2 != INVALID_INDEX)
		ChatClient.uiSortIndex = Host.GetChatNameColorSortIndex(uiChatNameColor2);
	else
		ChatClient.uiSortIndex = DEFAULT_CHAT_NAME_COLOR_SORT_INDEX;

	UpdateRecentlyPlayed();
}


/*====================
  CChatManager::Init
  ====================*/
void	CChatManager::Init(CHTTPManager* pHTTPManager)
{
	m_pHTTPManager = pHTTPManager;
	m_sRequester = "/client_requester.php";

	m_hStringTable = g_ResourceManager.Register(_T("/stringtables/chat_sounds.str"), RES_STRINGTABLE);

	m_pRecentlyPlayed = K2_NEW(ctx_Net, CXMLDoc)();

	CFile* pFile(FileManager.GetFile(_T("~/recentlyplayed.xml"), FILE_READ | FILE_TEXT | FILE_ALLOW_CUSTOM));
	if (pFile != NULL)
	{
		uint uiFileSize(0);
		const char* pBuffer(pFile->GetBuffer(uiFileSize));

		if (pBuffer != NULL && uiFileSize > 0)
			m_pRecentlyPlayed->ReadBuffer(pBuffer, uiFileSize);
		else
			m_pRecentlyPlayed->NewNode(_T("recentlyplayed"));

		pFile->Close();
		SAFE_DELETE(pFile);
	}

	pFile = FileManager.GetFile(_T("~/notes.txt"), FILE_READ | FILE_TEXT | FILE_ALLOW_CUSTOM | FILE_TEST);
	if (pFile != NULL)
	{
		while (!pFile->IsEOF())
		{
			tstring sLine(pFile->ReadLine());
			tsvector vsTokens(TokenizeString(sLine, _T('|')));

			if (vsTokens.size() >= 3)
			{
				m_vNoteTimes.push_back(vsTokens[1]);
				m_vNotes.push_back(vsTokens[2]);
			}
		}

		pFile->Close();
		SAFE_DELETE(pFile);
	}
}


/*====================
  CChatManager::Frame
  ====================*/
void	CChatManager::Frame()
{
#if !defined(K2_STABLE)
    if(debug_forceCrash)
        *(int*)(0x0) = 0;
#endif

	RequestFrame();

	ChatStatus.Trigger(XtoW(IsConnected()));

	if (m_uiMapSettingPhaseEndTime > 0)
	{	
		time_t tNow;
		time(&tNow);
		uint uiNow = (uint)tNow;
		
		if (m_uiMapSettingPhaseEndTime > uiNow)
			m_bMapSettingChangeTriggered = false;
		else if (m_uiMapSettingPhaseEndTime <= uiNow && !m_bMapSettingChangeTriggered)
		{
			ChatServerMapSettingChanged.Trigger(TSNULL);
			m_bMapSettingChangeTriggered = true;
		}
	}
	
	if (m_uiAccountID != INVALID_ACCOUNT)
	{
		switch (m_eStatus)
		{
		case CHAT_CLIENT_STATUS_CONNECTING:
			ConnectingFrame();
			break;

		case CHAT_CLIENT_STATUS_WAITING_FOR_AUTH:
			AuthFrame();
			break;

		case CHAT_CLIENT_STATUS_CONNECTED:
		case CHAT_CLIENT_STATUS_JOINING_GAME:
		case CHAT_CLIENT_STATUS_IN_GAME:
			ConnectedFrame();
			break;

		default:
			break;
		}
	}

	if (m_uiNextReconnectTime != INVALID_TIME && m_uiNextReconnectTime < K2System.Milliseconds() && !IsConnected())
	{
		m_uiNextReconnectTime = INVALID_TIME;
		Connect();
	}

#if defined(NEWUICODE)
	if (m_bBuddyUpdateRequired || m_bCafeUpdateRequired)
#else
	if (m_bBuddyUpdateRequired || m_bClanUpdateRequired || m_bCafeUpdateRequired)
#endif
	{
		UpdateCompanionList();

		m_bCafeUpdateRequired = false;
#if defined(NEWUICODE)
		m_bBuddyUpdateRequired = false;
#endif
	}
#if !defined(NEWUICODE)
	if (m_bBuddyUpdateRequired)
	{
		UpdateBuddyList();
		m_bBuddyUpdateRequired = false;
	}
#endif

	if (m_bClanUpdateRequired)
	{
		UpdateClanList();
		m_bClanUpdateRequired = false;
	}

	m_sockChat.Frame();

	UpdateTimers();

	// Expire player spectate requests
	if (Host.GetPendingPlayerSpectateExpiration() != INVALID_TIME && Host.GetTime() >= Host.GetPendingPlayerSpectateExpiration())
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_player_spectate_denied"), _T("name"), Host.GetPendingPlayerSpectateName()));
		Host.SetPendingPlayerSpectate(TSNULL, false);
		
	}

	PendingMatchFrame();
}


/*====================
  CChatManager::UpdateTimers
  ====================*/
void	CChatManager::UpdateTimers()
{
	// Only update timers every half second
	if (m_uiNextTimerUpdateTime == INVALID_INDEX || Host.GetSystemTime() >= m_uiNextTimerUpdateTime)
	{
		m_uiNextTimerUpdateTime = Host.GetSystemTime() + 500;

		// TMM UI updates
		if (m_uiTMMStartTime != INVALID_TIME)
		{
			static tsvector vMiniParams(3);

			if (m_uiTMMAverageQueueTime == INVALID_TIME)
			{
				vMiniParams[0] = XtoA(0);
				vMiniParams[1] = XtoA(0);
			}
			else
			{
				uint uiDifference(Host.GetTime() - m_uiTMMStartTime);
				uint uiMinutes(MsToMin(uiDifference));
				uint uiSeconds(uint(MsToSec(uiDifference)) % SEC_PER_MIN);

				vMiniParams[0] = XtoA(uiMinutes) + L":" + XtoA(uiSeconds, FMT_PADZERO, 2);
				
				if (m_uiTMMAverageQueueTime == 0)
				{
					vMiniParams[1] = L"??:??";
				}
				else
				{			
					uiMinutes = uint(SecToMin(m_uiTMMAverageQueueTime));
					uiSeconds = m_uiTMMAverageQueueTime % SEC_PER_MIN;

					vMiniParams[1] = XtoA(uiMinutes) + L":" + XtoA(uiSeconds, FMT_PADZERO, 2);
				}
			}

			vMiniParams[2] = XtoA(cc_TMMMatchFidelity);

			TMMTime.Trigger(vMiniParams, cc_forceTMMInterfaceUpdate);
		}
		
		// If a player has been temporarily banned from matchmaking, the timer is updated here
		if (m_uiTMMBanTime != INVALID_TIME)
		{
			tstring sBanTime;

			int iDifference(m_uiTMMBanTime - Host.GetTimeSeconds());

			if (iDifference <= 0)
			{
				sBanTime = L"0:00";
			}
			else
			{
				uint uiMinutes(SecToMin(uint(iDifference)));
				uint uiSeconds(iDifference % SEC_PER_MIN);

				sBanTime = XtoA(uiMinutes) + L":" + XtoA(uiSeconds, FMT_PADZERO, 2);
			}
			
			TMMBanTime.Trigger(sBanTime, cc_forceTMMInterfaceUpdate);
		}

		// For updating the match timer for a scheduled match
		map<uint, SScheduledMatchInfo>::iterator itScheduledMatch(m_mapScheduledMatchInfo.find(m_uiScheduledMatchID));

		if (itScheduledMatch != m_mapScheduledMatchInfo.end())
		{
			int iSecondsLeft(0);
			uint uiSecondsTillStart(itScheduledMatch->second.uiSecondsTillStart);
			uint uiSecondsTillExpiration(itScheduledMatch->second.uiSecondsTillExpiration);

			// Figure out whether to display time time left till the match starts, or the time left till it expires
			if (K2System.GetUnixTimestamp() < uiSecondsTillStart)
			{
				itScheduledMatch->second.yShouldStart = 0;
				iSecondsLeft = uiSecondsTillStart - K2System.GetUnixTimestamp();
			}
			else if (K2System.GetUnixTimestamp() < uiSecondsTillExpiration)
			{
				itScheduledMatch->second.yShouldStart = 1;
				iSecondsLeft = uiSecondsTillExpiration - K2System.GetUnixTimestamp();
			}
			else
			{
				// Since it appears both of these have expired we need to shut down this scheduled match on the client
				itScheduledMatch->second.yShouldStart = 2;

				// If the match should expire give it 60 seconds of additional time before removing ourselves from the scheduled match
				if (K2System.GetUnixTimestamp() - uiSecondsTillExpiration > MinToSec(1u))
					LeaveScheduledMatch(true, _T("matchexpired"));
			}

			if (uiSecondsTillStart != INVALID_TIME || uiSecondsTillExpiration != INVALID_TIME)
			{
				static tsvector vMiniParams(2);

				vMiniParams[0] = XtoA(itScheduledMatch->second.yShouldStart);

				if (iSecondsLeft <= 0)
				{
					vMiniParams[1] = L"0:00";
				}
				else
				{
					uint uiMinutes(SecToMin(uint(iSecondsLeft)));
					uint uiSeconds(iSecondsLeft % SEC_PER_MIN);

					vMiniParams[1] = XtoA(uiMinutes) + L":" + XtoA(uiSeconds, FMT_PADZERO, 2);
				}

				ScheduledMatchTime.Trigger(vMiniParams, cc_forceTMMInterfaceUpdate);
			}
		}
	}
}


/*====================
  CChatManager::ProcessFailedRequest
  ====================*/
void	CChatManager::ProcessFailedRequest(SChatRequest* pRequest)
{
	tsvector vParams(2);

	switch (pRequest->eType)
	{
	case REQUEST_DELETE_BUDDY:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_buddy_remove"), _T("target"), pRequest->sTarget));
		break;

	case REQUEST_CLAN_PROMOTE:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_clan_promote"), _T("target"), pRequest->sTarget));
		break;

	case REQUEST_CLAN_DEMOTE:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_clan_demote"), _T("target"), pRequest->sTarget));
		break;
		
	case REQUEST_CLAN_REMOVE:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_clan_remove"), _T("target"), pRequest->sTarget));
		break;

	case REQUEST_ADD_BANNED_NICK2ID:
	case REQUEST_ADD_BANNED:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_banlist_add"), _T("target"), pRequest->sTarget));
		break;

	case REQUEST_REMOVE_BANNED:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_banlist_remove"), _T("target"), pRequest->sTarget));
		break;

	case REQUEST_ADD_IGNORED_NICK2ID:
	case REQUEST_ADD_IGNORED:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_ignore_add"), _T("target"), pRequest->sTarget));
		break;

	case REQUEST_REMOVE_IGNORED:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_ignore_remove"), _T("target"), pRequest->sTarget));
		break;

	case REQUEST_CHECK_CLAN_NAME:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_clan_name")));
		ChatClanCreateTip.Trigger(Translate(_T("chat_failed_clan_name_tip")));
		break;

	case REQUEST_GET_MESSAGES:
		MessagesUpdated.Trigger(_T("false"));
		break;

	case REQUEST_GET_MESSAGE:
		ProcessGetMessageResponse(pRequest, true);
		break;

	case REQUEST_DELETE_MESSAGE:
		vParams[0] = pRequest->sTarget;
		vParams[1] = _T("false");
		MessageDeleted.Trigger(vParams);
		break;
	}
}


/*====================
  CChatManager::ProcessRemoveBuddySuccess
  ====================*/
void	CChatManager::ProcessRemoveBuddySuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	if (CompareNoCase(phpResponse.GetTString(_U8("remove_buddy")), _T("OK")) == 0 && phpResponse.GetVar(_U8("error")) == NULL)
	{
		// Update our buddy list...
		RemoveBuddy(pRequest->uiTarget);
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_success_buddy_remove"), _T("target"), RemoveClanTag(pRequest->sTarget)));

		const CPHPData* pNotify(phpResponse.GetVar(_U8("notification")));
		if (pNotify != NULL && IsConnected())
		{
			const uint uiNotify1(pNotify->GetInteger(_U8("1")));
			const uint uiNotify2(pNotify->GetInteger(_U8("2")));

			CPacket pktSend;
			pktSend << CHAT_CMD_NOTIFY_BUDDY_REMOVE << pRequest->uiTarget << uiNotify1 << uiNotify2;
			m_sockChat.SendPacket(pktSend);
		}
	}
	else
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_buddy_remove"), _T("target"), RemoveClanTag(pRequest->sTarget)));
	}
}


/*====================
  CChatManager::ProcessClanPromoteSuccess
  ====================*/
void	CChatManager::ProcessClanPromoteSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	if (CompareNoCase(phpResponse.GetTString(_U8("set_rank")), _T("Member updated.")) == 0 && phpResponse.GetVar(_U8("error")) == NULL)
	{
		// Update our clan list...
		ChatClientMap_it itFind(m_mapUserList.find(pRequest->uiTarget));
		if (itFind != m_mapUserList.end())
		{
			itFind->second.yFlags |= CHAT_CLIENT_IS_OFFICER;
			RefreshClanList();
		}

		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_success_clan_promote"), _T("target"), RemoveClanTag(pRequest->sTarget)));

		CPacket pktSend;
		pktSend << CHAT_CMD_CLAN_PROMOTE_NOTIFY << pRequest->uiTarget;
		m_sockChat.SendPacket(pktSend);

	}
	else
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_clan_promote_generic")));
	}
}


/*====================
  CChatManager::ProcessClanDemoteSuccess
  ====================*/
void	CChatManager::ProcessClanDemoteSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	if (CompareNoCase(phpResponse.GetTString(_U8("set_rank")), _T("Member updated.")) == 0 && phpResponse.GetVar(_U8("error")) == NULL)
	{
		// Update our clan list...
		ChatClientMap_it itFind(m_mapUserList.find(pRequest->uiTarget));
		if (itFind != m_mapUserList.end())
		{
			itFind->second.yFlags &= ~CHAT_CLIENT_IS_OFFICER;
			RefreshClanList();
		}

		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_success_clan_demote"), _T("target"), RemoveClanTag(pRequest->sTarget)));

		CPacket pktSend;
		pktSend << CHAT_CMD_CLAN_DEMOTE_NOTIFY << pRequest->uiTarget;
		m_sockChat.SendPacket(pktSend);
	}
	else
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_clan_demote_generic")));
	}
}


/*====================
  CChatManager::ProcessClanRemoveSuccess
  ====================*/
void	CChatManager::ProcessClanRemoveSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	if (CompareNoCase(phpResponse.GetTString(_U8("set_rank")), _T("Member updated.")) == 0 && phpResponse.GetVar(_U8("error")) == NULL)
	{
		CPacket pktSend;
		pktSend << CHAT_CMD_CLAN_REMOVE_NOTIFY << pRequest->uiTarget;
		m_sockChat.SendPacket(pktSend);

		if (pRequest->uiTarget != m_uiAccountID)
		{
			// Update our clan list...
			RemoveClanMember(pRequest->uiTarget);
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_success_clan_remove"), _T("target"), RemoveClanTag(pRequest->sTarget)));
		}
	}
	else
	{
		if (pRequest->uiTarget != m_uiAccountID)
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_clan_remove"), _T("target"),RemoveClanTag(pRequest->sTarget)));
		else
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_clan_remove_self")));
	}
}


/*====================
  CChatManager::ProcessClanUpdateSuccess
  ====================*/
void	CChatManager::ProcessClanUpdateSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	if (phpResponse.GetString(_U8("error")).empty() && phpResponse.GetVar(_U8("error")) == NULL)
	{
		ClearClanList();

		const CPHPData* pClan(phpResponse.GetVar(_U8("clan_roster")));
		if (pClan != NULL && pClan->GetVar(_U8("error")) == NULL)
		{
			uint uiNum(0);
			const CPHPData* pClanItem(pClan->GetVar(uiNum++));

			while (pClanItem != NULL)
			{
				tstring sRank(pClanItem->GetTString(_U8("rank")));
				tstring sName(pClanItem->GetTString(_U8("nickname")));
				int iAccountID(pClanItem->GetInteger(_U8("account_id")));

				if (!pRequest->sText.empty())
					sName = _T("[") + pRequest->sText + _T("]") + sName;
				
				byte yFlags(0);

				if (CompareNoCase((sRank), _T("Leader")) == 0)
					yFlags |= CHAT_CLIENT_IS_CLAN_LEADER;
				else if (CompareNoCase((sRank), _T("Officer")) == 0)
					yFlags |= CHAT_CLIENT_IS_OFFICER;

				AddClanMember(iAccountID, sName, yFlags);
				pClanItem = pClan->GetVar(uiNum++);
			}
		}
	}
}


/*====================
  CChatManager::ProcessClanNameCheckSuccess
  ====================*/
void	CChatManager::ProcessClanNameCheckSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	tsmapts mapParams;
	mapParams[_T("name")] = pRequest->sTarget;
	mapParams[_T("tag")] = pRequest->sText;

	if (CompareNoCase(phpResponse.GetTString(_U8("clan_check")), _T("OK")) == 0)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_success_clan_name_tip"), mapParams));
		ChatClanCreateTip.Trigger(Translate(_T("chat_success_clan_name_tip"), mapParams));
	}
	else if (CompareNoCase(phpResponse.GetTString(_U8("clan_name")), _T("Clan name already taken.")) == 0)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_invalid_clan_name_used"), mapParams));
		ChatClanCreateTip.Trigger(Translate(_T("chat_invalid_clan_name_used_tip"), mapParams));
	}
	else if (CompareNoCase(phpResponse.GetTString(_U8("clan_tag")), _T("Clan tag already taken.")) == 0)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_invalid_clan_tag_used"), mapParams));
		ChatClanCreateTip.Trigger(Translate(_T("chat_invalid_clan_tag_used_tip"), mapParams));
	}
	else if (CompareNoCase(phpResponse.GetTString(_U8("clan_name")), _T("Invalid clan name format. 1-32 alphanum or spaces chars.")) == 0)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_invalid_clan_name_format"), mapParams));
		ChatClanCreateTip.Trigger(Translate(_T("chat_invalid_clan_name_format_tip"), mapParams));
	}
	else if (CompareNoCase(phpResponse.GetTString(_U8("clan_tag")), _T("Invalid clan tag format. 1-4 alphanum chars.")) == 0)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_invalid_clan_tag_format"), mapParams));
		ChatClanCreateTip.Trigger(Translate(_T("chat_invalid_clan_tag_format_tip"), mapParams));
	}
	else
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_invalid_clan_name"), mapParams));
		ChatClanCreateTip.Trigger(Translate(_T("chat_invalid_clan_name_tip"), mapParams));
	}
}


/*====================
  CChatManager::ProcessBanLookupIDSuccess
  ====================*/
void	CChatManager::ProcessBanLookupIDSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	const CPHPData* pAccountID(phpResponse.GetVar(0));
	if (phpResponse.GetTString(_U8("error")).empty() && phpResponse.GetVar(_U8("error")) == NULL && pAccountID != NULL)
	{
		CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
		if (pHTTPRequest == NULL)
			return;

		pHTTPRequest->SetHost(Host.GetMasterServerAddress());
		pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=new_banned");
		pHTTPRequest->AddVariable(L"account_id", m_uiAccountID);
		pHTTPRequest->AddVariable(L"banned_id", pAccountID->GetInteger());
		pHTTPRequest->AddVariable(L"reason", pRequest->sText);
		pHTTPRequest->AddVariable(L"cookie", m_sCookie);
		pHTTPRequest->SendPostRequest();

		SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_ADD_BANNED, pAccountID->GetInteger(), pRequest->sText));
		pNewRequest->sTarget = pRequest->sTarget;

		m_lHTTPRequests.push_back(pNewRequest);
	}
	else
	{
		if (!phpResponse.GetTString(_U8("error")).empty())
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_banlist_add_reason"), _T("target"), pRequest->sTarget, _T("reason"), phpResponse.GetTString(_U8("error"))));
		else if (phpResponse.GetVar(_U8("error")) != NULL)
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_banlist_add_reason"), _T("target"), pRequest->sTarget, _T("reason"), phpResponse.GetVar(_U8("error"))->GetTString()));
		else
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_banlist_add"), _T("target"), pRequest->sTarget));
	}
}


/*====================
  CChatManager::ProcessAddBanSuccess
  ====================*/
void	CChatManager::ProcessAddBanSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	if (phpResponse.GetTString(_U8("error")).empty() && phpResponse.GetVar(_U8("error")) == NULL)
	{
		AddBan(pRequest->uiTarget, pRequest->sTarget, pRequest->sText);
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_success_banlist_add"), _T("target"), pRequest->sTarget));
	}
	else
	{
		if (!phpResponse.GetTString(_U8("error")).empty())
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_banlist_add_reason"), _T("target"), pRequest->sTarget, _T("reason"), phpResponse.GetTString(_U8("error"))));
		else if (phpResponse.GetVar(_U8("error")) != NULL)
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_banlist_add_reason"), _T("target"), pRequest->sTarget, _T("reason"), phpResponse.GetVar(_U8("error"))->GetTString()));
	}
}


/*====================
  CChatManager::ProcessRemoveBanSuccess
  ====================*/
void	CChatManager::ProcessRemoveBanSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	if (phpResponse.GetTString(_U8("error")).empty() && phpResponse.GetVar(_U8("error")) == NULL)
	{
		//Update our banlist...
		RemoveBan(pRequest->uiTarget);
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_success_banlist_remove"), _T("target"), pRequest->sTarget));
	}
	else
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_banlist_remove"), _T("target"), pRequest->sTarget));
	}
}


/*====================
  CChatManager::ProcessIgnoreLookupIDSuccess
  ====================*/
void	CChatManager::ProcessIgnoreLookupIDSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	const CPHPData* pAccountID(phpResponse.GetVar(0));
	if (phpResponse.GetTString(_U8("error")).empty() && phpResponse.GetVar(_U8("error")) == NULL && pAccountID != NULL)
	{
		CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
		if (pHTTPRequest == NULL)
			return;

		pHTTPRequest->SetHost(Host.GetMasterServerAddress());
		pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=new_ignored");
		pHTTPRequest->AddVariable(L"account_id", m_uiAccountID);
		pHTTPRequest->AddVariable(L"ignored_id", pAccountID->GetInteger());
		pHTTPRequest->AddVariable(L"cookie", m_sCookie);
		pHTTPRequest->SendPostRequest();

		SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_ADD_IGNORED, pAccountID->GetInteger()));
		pNewRequest->sTarget = pRequest->sTarget;

		m_lHTTPRequests.push_back(pNewRequest);
	}
	else
	{
		if (!phpResponse.GetTString(_U8("error")).empty())
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_ignore_add_reason"), _T("target"), pRequest->sTarget, _T("reason"), phpResponse.GetTString(_U8("error"))));
		else if (phpResponse.GetVar(_U8("error")) != NULL)
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_ignore_add_reason"), _T("target"), pRequest->sTarget, _T("reason"), phpResponse.GetVar(_U8("error"))->GetTString()));
		else
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_ignore_add"), _T("target"), pRequest->sTarget));
	}

}


/*====================
  CChatManager::ProcessIgnoreAddSuccess
  ====================*/
void	CChatManager::ProcessIgnoreAddSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	if (phpResponse.GetTString(_U8("error")).empty() && phpResponse.GetVar(_U8("error")) == NULL)
	{
		AddIgnore(pRequest->uiTarget, pRequest->sTarget);
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_success_ignore_add"), _T("target"), pRequest->sTarget));
	}
	else
	{
		if (!phpResponse.GetTString(_U8("error")).empty())
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_ignore_add_reason"), _T("target"), pRequest->sTarget, _T("reason"), phpResponse.GetTString(_U8("error"))));
		else if (phpResponse.GetVar(_U8("error")) != NULL)
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_ignore_add_reason"), _T("target"), pRequest->sTarget, _T("reason"), phpResponse.GetVar(_U8("error"))->GetTString()));
	}
}


/*====================
  CChatManager::ProcessIgnoreRemoveSuccess
  ====================*/
void	CChatManager::ProcessIgnoreRemoveSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	if (phpResponse.GetTString(_U8("error")).empty() && phpResponse.GetVar(_U8("error")) == NULL)
	{
		// Update our ignore list...
		RemoveIgnore(pRequest->uiTarget);
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_success_ignore_remove"), _T("target"), pRequest->sTarget));
	}
	else
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_ignore_remove"), _T("target"), pRequest->sTarget));
	}
}


/*====================
  CChatManager::ProcessCompleteNickSuccess
  ====================*/
void	CChatManager::ProcessCompleteNickSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	const CPHPData* pArray(phpResponse.GetVar(_U8("nicks")));
	if (pArray == NULL)
		return;

	uint uiPos(0);
	const CPHPData* pName(pArray->GetVar(uiPos));

	while (pName != NULL)
	{
		ChatAutoCompleteAdd.Trigger(pName->GetTString());
		
		++uiPos;
		pName = pArray->GetVar(uiPos);
	}
}


/*====================
  CChatManager::ProcessSaveChannelSuccess
  ====================*/
void	CChatManager::ProcessSaveChannelSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	if (CompareNoCase(phpResponse.GetTString(_U8("add_room")), _T("OK")) != 0)
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_saving_channel"), _T("channel"), pRequest->sTarget));
	else
		SaveChannelLocal(pRequest->sTarget);
}


/*====================
  CChatManager::ProcessRemoveChannelSuccess
  ====================*/
void	CChatManager::ProcessRemoveChannelSuccess(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	if (CompareNoCase(phpResponse.GetTString(_U8("remove_room")), _T("OK")) == 0)
		RemoveChannelLocal(pRequest->sTarget);
	else if (CompareNoCase(phpResponse.GetTString(_U8("clear_rooms")), _T("OK")) == 0)
		RemoveChannelsLocal();
	else
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_removing_channel"), _T("channel"), pRequest->sTarget));
}


/*====================
  CChatManager::ProcessSaveNotificationResponse
  ====================*/
void	CChatManager::ProcessSaveNotificationResponse(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());
	
	if (CompareNoCase(phpResponse.GetTString(_U8("status")), _T("OK")) != 0)
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_saving_notification")));
	else
	{		
		const tstring sNotification(phpResponse.GetTString(_U8("notification")));
		const uint uiExternalNotificationID(phpResponse.GetInteger(_U8("notify_id")));
		ParseNotification(sNotification, uiExternalNotificationID);			
	}
}


/*====================
  CChatManager::ProcessRemoveNotificationResponse
  ====================*/
void	CChatManager::ProcessRemoveNotificationResponse(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());
	
	const uint uiExternalNotificationID(phpResponse.GetInteger(_U8("notify_id")));	

	if (CompareNoCase(phpResponse.GetTString(_U8("status")), _T("OK")) != 0)
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_removing_notification"), _T("id"), XtoA(uiExternalNotificationID)));
	else
	{
		const uint uiInternalNotificationID(phpResponse.GetInteger(_U8("internal_id")));				
		RemoveExternalNotification(uiInternalNotificationID);
	}		
}


/*====================
  CChatManager::ProcessRemoveAllNotificationsResponse
  ====================*/
void	CChatManager::ProcessRemoveAllNotificationsResponse(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());
	
	if (CompareNoCase(phpResponse.GetTString(_U8("status")), _T("OK")) != 0)
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_removing_notifications")));
	else
		ClearNotifications();
}


/*====================
  CChatManager::RequestFrame
  ====================*/
void	CChatManager::RequestFrame()
{
	ChatRequestList_it itRequest(m_lHTTPRequests.begin());
	ChatRequestList_it itEnd(m_lHTTPRequests.end());
	while (itRequest != itEnd)
	{
		SChatRequest* pRequest(*itRequest);

		switch (pRequest->pRequest->GetStatus())
		{
		case HTTP_REQUEST_SENDING:
			++itRequest;
			break;

		default:
		case HTTP_REQUEST_IDLE:
		case HTTP_REQUEST_ERROR:
			ProcessFailedRequest(pRequest);
			
			m_pHTTPManager->ReleaseRequest(pRequest->pRequest);
			K2_DELETE(pRequest);
			itRequest = m_lHTTPRequests.erase(itRequest);
			itEnd = m_lHTTPRequests.end();
			break;

		case HTTP_REQUEST_SUCCESS:
			switch (pRequest->eType)
			{
			case REQUEST_DELETE_BUDDY:				ProcessRemoveBuddySuccess(pRequest); break;
			case REQUEST_CLAN_PROMOTE:				ProcessClanPromoteSuccess(pRequest); break;
			case REQUEST_CLAN_DEMOTE:				ProcessClanDemoteSuccess(pRequest); break;
			case REQUEST_CLAN_REMOVE:				ProcessClanRemoveSuccess(pRequest); break;
			case REQUEST_ADD_BANNED_NICK2ID:		ProcessBanLookupIDSuccess(pRequest); break;
			case REQUEST_ADD_BANNED:				ProcessAddBanSuccess(pRequest); break;
			case REQUEST_REMOVE_BANNED:				ProcessRemoveBanSuccess(pRequest); break;
			case REQUEST_GET_BANNED:				break;
			case REQUEST_ADD_IGNORED_NICK2ID:		ProcessIgnoreLookupIDSuccess(pRequest); break;
			case REQUEST_ADD_IGNORED:				ProcessIgnoreAddSuccess(pRequest); break;
			case REQUEST_REMOVE_IGNORED:			ProcessIgnoreRemoveSuccess(pRequest); break;
			case REQUEST_UPDATE_CLAN:				ProcessClanUpdateSuccess(pRequest); break;
			case REQUEST_CHECK_CLAN_NAME:			ProcessClanNameCheckSuccess(pRequest); break;
			case REQUEST_COMPLETE_NICK:				ProcessCompleteNickSuccess(pRequest); break;
			case REQUEST_SAVE_CHANNEL:				ProcessSaveChannelSuccess(pRequest); break;
			case REQUEST_REMOVE_CHANNEL:			ProcessRemoveChannelSuccess(pRequest); break;
			case REQUEST_SAVE_NOTIFICATION:			ProcessSaveNotificationResponse(pRequest); break;
			case REQUEST_REMOVE_NOTIFICATION:		ProcessRemoveNotificationResponse(pRequest); break;
			case REQUEST_REMOVE_ALL_NOTIFICATIONS:	ProcessRemoveAllNotificationsResponse(pRequest); break;
			case REQUEST_CHANGE_BUDDY_GROUP:		ProcessChangeBuddyGroupResponse(pRequest); break;
			case REQUEST_GET_MESSAGES:				ProcessGetMessagesResponse(pRequest); break;
			case REQUEST_GET_MESSAGE:				ProcessGetMessageResponse(pRequest); break;
			case REQUEST_DELETE_MESSAGE:			ProcessDeleteMessageResponse(pRequest); break;
			case REQUEST_GET_SPECIAL_MESSAGES:		ProcessGetSpecialMessagesResponse(pRequest); break;
			}

			m_pHTTPManager->ReleaseRequest(pRequest->pRequest);
			K2_DELETE(pRequest);
			itRequest = m_lHTTPRequests.erase(itRequest);
			itEnd = m_lHTTPRequests.end();
			break;
		}
	}
}


/*====================
  CChatManager::Disconnect
  ====================*/
void	CChatManager::Disconnect()
{
	m_uiConnectRetries = -1;
	m_uiNextReconnectTime = INVALID_TIME;
	m_uiChatMuteExpiration = 0;

	m_setChannelsIn.clear();
	ChatLeftChannel.Trigger(_T("-1"));
	
	// These need to be done on logout and on login, to make sure the UI stays in sync with the various routes they can 
	// disconnect or logout by (manual disconnect, net drop, chat connect retries exceeded, etc)
	ClearNotifications();
#if defined(K2_AWESOMIUM)
	WebBrowserManager.ClearActiveStreams();
#endif // defined(K2_AWESOMIUM)

	HandleDisconnect();
}


/*====================
  CChatManager::ConnectingFrame
  ====================*/
void	CChatManager::ConnectingFrame()
{
	if (!m_sockChat.IsConnected())
	{
		if (Host.GetTimeSeconds() > m_uiConnectTimeout)
			HandleDisconnect();

		return;
	}

	if (_testNoData)
	{
		AuthFrame();

		return;
	}

	SOSInfo OSInfo(K2System.GetOSInfo());
	tsvector vsVersion(TokenizeString(K2System.GetVersionString(), L'.'));
	uint uiVersion((AtoI(vsVersion[0]) << 0) + (AtoI(vsVersion[1]) << 8) + (AtoI(vsVersion[2]) << 16) + (AtoI(vsVersion[3]) << 24));
	int iLastClientState(CrashReporter.GetLastClientState().eState);

//	Console.Warn << ">>>>>>" << newl;
//	Console.Warn << ">>>>>> iLastClientState" << iLastClientState << newl;
//	Console.Warn << ">>>>>>" << newl;

	CPacket pktSend;
	pktSend 
		<< NET_CHAT_CL_CONNECT 
		<< m_uiAccountID 
		<< m_sCookie 
		<< m_sRemoteIP 
		<< m_sAuthHash 
		<< CHAT_PROTOCOL_VERSION 
		<< OSInfo.yOSType
		<< OSInfo.yMajorVersion 
		<< OSInfo.yMinorVersion 
		<< OSInfo.yMicroVersion 
		<< K2System.GetBuildOSCodeString()
		<< K2System.GetBuildArchString()
		<< uiVersion
		<< byte(iLastClientState)
		<< m_yChatModeType
		<< Host.GetRegion()
		<< host_language;

	m_sockChat.SendPacket(pktSend);

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_waiting_verification")));

	m_eStatus = CHAT_CLIENT_STATUS_WAITING_FOR_AUTH;
}


/*====================
  CChatManager::AuthFrame
  ====================*/
void	CChatManager::AuthFrame()
{
	for (CMessageSocket::CProcessor it(&m_sockChat) ; !it.Done() ; ++it)
	{
		if (it.GetStatus() < 0)
		{
			// Socket closed or socket error
			HandleDisconnect();
			return;
		}

		ProcessData(it.GetPacket());
	}

	if (!m_sockChat.IsConnected())
	{
		// Connection dropped
		HandleDisconnect();
		return;
	}
}


/*====================
  CChatManager::ConnectedFrame
  ====================*/
void	CChatManager::ConnectedFrame()
{
	if (!m_sockChat.IsConnected())
	{
		// Connection dropped
		HandleDisconnect();
		return;
	}

	for (CMessageSocket::CProcessor it(&m_sockChat) ; !it.Done() ; ++it)
	{
		if (it.GetStatus() < 0)
		{
			// Socket closed or socket error
			HandleDisconnect();
			return;
		}

		ProcessData(it.GetPacket());
	}

	// Update clan creation timer
	if (m_uiCreateTimeSent != INVALID_TIME)
	{
		if (m_uiCreateTimeSent + 120000 < K2System.Milliseconds())
		{
			m_uiCreateTimeSent = INVALID_TIME;
			ChatClanCreateFail.Trigger(Translate(_T("chat_clan_create_result_fail_time")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_fail_time")));
		}
		else
		{
			ChatClanCreateTime.Trigger(XtoA((m_uiCreateTimeSent + 120000) - K2System.Milliseconds()));
		}
	}
}


/*====================
  CChatManager::ProcessData
  ====================*/
bool	CChatManager::ProcessData(CPacket& pkt)
{
	if (pkt.IsEmpty())
	{
		Console << _T("Received an empty packet from the chat server") << newl;
		return true;
	}

	ushort unPrevCmd(NET_CHAT_INVALID);

	while (!pkt.DoneReading())
	{
		ushort unCmd(pkt.ReadShort(NET_CHAT_INVALID));

		//Console << L"Chat server: " << SHORT_HEX_STR(unCmd) << newl;
		
		switch (unCmd)
		{
		case NET_CHAT_CL_ACCEPT:	HandleAuthAccepted(); break;
		case NET_CHAT_CL_REJECT:	HandleRejected(pkt); break;

		case NET_CHAT_PING:
			HandlePing();
			break;
			
		case NET_CHAT_CL_CHANNEL_INFO:
			HandleChannelInfo(pkt);
			break;

		case NET_CHAT_CL_CHANNEL_LIST_SYN:
			{
				CPacket pktSend;
				pktSend << NET_CHAT_CL_CHANNEL_LIST_ACK;
				m_sockChat.SendPacket(pktSend);
				ChatChannelList.Execute(L"SortByCol(0);");
			}
			break;

		case NET_CHAT_CL_CHANNEL_INFO_SUB:
			HandleChannelInfoSub(pkt);
			break;

		case NET_CHAT_CL_CHANNEL_SUBLIST_SYN:
			{
				byte ySequence(pkt.ReadByte());

				CPacket pktSend;
				pktSend << NET_CHAT_CL_CHANNEL_SUBLIST_ACK << ySequence;
				m_sockChat.SendPacket(pktSend);
			}
			break;

		case NET_CHAT_CL_CHANNEL_SUBLIST_START:
			{
				byte ySequence(pkt.ReadByte());
				tstring sHead(WStringToTString(pkt.ReadWString()));
				
				//Console << _T("Start Channel sublist: ") << ySequence << newl;

				if (ySequence == m_yListStartSequence)
				{
					AutoCompleteClear();
					m_mapChannelList.clear();
					m_bFinishedList = false;

					m_sProcessingListHead = sHead;
					m_yProcessingListSequence = ySequence;
				}
			}
			break;

		case NET_CHAT_CL_CHANNEL_SUBLIST_END:
			{
				byte ySequence(pkt.ReadByte());

				//Console << _T("End Channel sublist: ") << ySequence << newl;

				if (ySequence == m_yProcessingListSequence)
				{
					m_yFinishedListSequence = m_yProcessingListSequence;
					m_sFinishedListHead = m_sProcessingListHead;
					m_bFinishedList = true;

					m_yProcessingListSequence = 0xff;
				}
			}
			break;

		case NET_CHAT_CL_USER_STATUS:				HandleUserStatus(pkt);	break;
		case CHAT_CMD_CHANGED_CHANNEL:				HandleChannelChange(pkt);	break;
		case CHAT_CMD_JOINED_CHANNEL:				HandleChannelJoin(pkt);	break;
		case CHAT_CMD_CHANNEL_MSG:					HandleChannelMessage(pkt);	break;
		case CHAT_CMD_LEFT_CHANNEL:					HandleChannelLeave(pkt);	break;
		case CHAT_CMD_WHISPER:						HandleWhisper(pkt);	break;
		case CHAT_CMD_WHISPER_BUDDIES:				HandleWhisperBuddies(pkt);	break;
		case CHAT_CMD_WHISPER_FAILED:				HandleWhisperFailed(pkt);	break;
		case CHAT_CMD_DISCONNECTED:					HandleDisconnect();	break;
		case CHAT_CMD_INITIAL_STATUS:				HandleInitialStatusUpdate(pkt);	break;
		case CHAT_CMD_UPDATE_STATUS:				HandleStatusUpdate(pkt);	break;
		case CHAT_CMD_CLAN_WHISPER:					HandleClanWhisper(pkt);	break;
		case CHAT_CMD_CLAN_WHISPER_FAILED:			HandleClanWhisperFailed();	break;
		case CHAT_CMD_FLOODING:						HandleFlooding();	break;
		case CHAT_CMD_IM:							HandleIM(pkt);	break;
		case CHAT_CMD_IM_FAILED:					HandleIMFailed(pkt);	break;
		case CHAT_CMD_MAX_CHANNELS:					HandleMaxChannels();	break;
		case CHAT_CMD_INVITED_TO_SERVER:			HandleServerInvite(pkt);	break;
		case CHAT_CMD_INVITE_FAILED_USER:			HandleInviteFailedUserNotFound();	break;
		case CHAT_CMD_INVITE_FAILED_GAME:			HandleInviteFailedNotInGame();	break;
		case CHAT_CMD_INVITE_REJECTED:				HandleInviteRejected(pkt);	break;
		case CHAT_CMD_USER_INFO_NO_EXIST:			HandleUserInfoNoExist(pkt);	break;
		case CHAT_CMD_USER_INFO_OFFLINE:			HandleUserInfoOffline(pkt);	break;
		case CHAT_CMD_USER_INFO_ONLINE:				HandleUserInfoOnline(pkt);	break;
		case CHAT_CMD_USER_INFO_IN_GAME:			HandleUserInfoInGame(pkt);	break;
		case CHAT_CMD_CHANNEL_UPDATE:				HandleChannelUpdate(pkt);	break;
		case CHAT_CMD_CHANNEL_TOPIC:				HandleChannelTopic(pkt);	break;
		case CHAT_CMD_CHANNEL_KICK:					HandleChannelKick(pkt);	break;
		case CHAT_CMD_CHANNEL_BAN:					HandleChannelBan(pkt);	break;
		case CHAT_CMD_CHANNEL_UNBAN:				HandleChannelUnban(pkt);	break;
		case CHAT_CMD_CHANNEL_IS_BANNED:			HandleBannedFromChannel(pkt);	break;
		case CHAT_CMD_CHANNEL_SILENCED:				HandleChannelSilenced(pkt);	break;
		case CHAT_CMD_CHANNEL_SILENCE_LIFTED:		HandleChannelSilenceLifted(pkt);	break;
		case CHAT_CMD_CHANNEL_SILENCE_PLACED:		HandleSilencePlaced(pkt);	break;
		case CHAT_CMD_CHANNEL_PROMOTE:				HandleChannelPromote(pkt);	break;
		case CHAT_CMD_CHANNEL_DEMOTE:				HandleChannelDemote(pkt);	break;
		case CHAT_CMD_MESSAGE_ALL:					HandleMessageAll(pkt);	break;
		case CHAT_CMD_CHANNEL_SET_AUTH:				HandleChannelAuthEnabled(pkt);	break;
		case CHAT_CMD_CHANNEL_REMOVE_AUTH:			HandleChannelAuthDisabled(pkt);	break;
		case CHAT_CMD_CHANNEL_ADD_AUTH_USER:		HandleChannelAddAuthUser(pkt);	break;
		case CHAT_CMD_CHANNEL_REMOVE_AUTH_USER:		HandleChannelRemoveAuthUser(pkt);	break;
		case CHAT_CMD_CHANNEL_ADD_AUTH_FAIL:		HandleChannelAddAuthUserFailed(pkt);	break;
		case CHAT_CMD_CHANNEL_REMOVE_AUTH_FAIL:		HandleChannelRemoveAuthUserFailed(pkt);	break;
		case CHAT_CMD_CHANNEL_LIST_AUTH:			HandleChannelListAuth(pkt);	break;
		case CHAT_CMD_CHANNEL_SET_PASSWORD:			HandleChannelSetPassword(pkt);	break;
		case CHAT_CMD_JOIN_CHANNEL_PASSWORD:		HandleChannelJoinPassword(pkt);	break;
		case CHAT_CMD_CLAN_ADD_MEMBER:				HandleClanInvite(pkt);	break;
		case CHAT_CMD_CLAN_ADD_REJECTED:			HandleClanInviteRejected(pkt);	break;
		case CHAT_CMD_CLAN_ADD_FAIL_ONLINE:			HandleClanInviteFailedOnline(pkt);	break;
		case CHAT_CMD_CLAN_ADD_FAIL_CLAN:			HandleClanInviteFailedClan(pkt);	break;
		case CHAT_CMD_CLAN_ADD_FAIL_INVITED:		HandleClanInviteFailedInvite(pkt);	break;
		case CHAT_CMD_CLAN_ADD_FAIL_PERMS:			HandleClanInviteFailedPermissions(pkt);	break;
		case CHAT_CMD_CLAN_ADD_FAIL_UNKNOWN:		HandleClanInviteFailedUnknown(pkt);	break;
		case CHAT_CMD_NEW_CLAN_MEMBER:				HandleNewClanMember(pkt);	break;
		case CHAT_CMD_CLAN_RANK_CHANGE:				HandleClanRankChanged(pkt);	break;
		case CHAT_CMD_CLAN_CREATE_ACCEPT:			HandleClanCreateAccept(pkt);	break;
		case CHAT_CMD_CLAN_CREATE_REJECT:			HandleClanCreateRejected(pkt);	break;
		case CHAT_CMD_CLAN_CREATE_COMPLETE:			HandleClanCreateComplete(pkt);	break;
		case CHAT_CMD_CLAN_CREATE_FAIL_CLAN:		HandleClanCreateFailedClan(pkt);	break;
		case CHAT_CMD_CLAN_CREATE_FAIL_INVITE:		HandleClanCreateFailedInvite(pkt);	break;
		case CHAT_CMD_CLAN_CREATE_FAIL_FIND:		HandleClanCreateFailedNotFound(pkt);	break;
		case CHAT_CMD_CLAN_CREATE_FAIL_DUPE:		HandleClanCreateFailedDuplicate(pkt);	break;
		case CHAT_CMD_CLAN_CREATE_FAIL_PARAM:		HandleClanCreateFailedParam(pkt);	break;
		case CHAT_CMD_NAME_CHANGE:					HandleNameChange(pkt);	break;
		case CHAT_CMD_CLAN_CREATE_FAIL_NAME:		HandleClanCreateFailedClanName(pkt);	break;
		case CHAT_CMD_CLAN_CREATE_FAIL_TAG:			HandleClanCreateFailedTag(pkt);	break;
		case CHAT_CMD_CLAN_CREATE_FAIL_UNKNOWN:		HandleClanCreateFailedUnknown(pkt);	break;
		case CHAT_CMD_AUTO_MATCH_CONNECT:			HandleAutoMatchConnect(pkt);	break;
		case CHAT_CMD_SERVER_NOT_IDLE:				HandleServerNotIdle(pkt);	break;
		// case CHAT_CMD_CHAT_ROLL:					HandleChatRoll(pkt);	break;
		// case CHAT_CMD_CHAT_EMOTE:					HandleChatEmote(pkt);	break;
		case CHAT_CMD_SET_CHAT_MODE_TYPE:			HandleSetChatModeType(pkt);	break;
		case CHAT_CMD_CHAT_MODE_AUTO_RESPONSE:		HandleChatModeAutoResponse(pkt);	break;
		case CHAT_CMD_PLAYER_COUNT:					HandleUserCount(pkt);	break;
		case CHAT_CMD_REQUEST_BUDDY_ADD_RESPONSE:		HandleRequestBuddyAddResponse(pkt);	break;			
		case CHAT_CMD_REQUEST_BUDDY_APPROVE_RESPONSE:	HandleRequestBuddyApproveResponse(pkt);break;
		case CHAT_CMD_STAFF_JOIN_MATCH_RESPONSE:	HandleStaffJoinMatchResponse(pkt);	break;
		case NET_CHAT_CL_TMM_GROUP_INVITE:			HandleTMMInviteToGroup(pkt);	break;
		case NET_CHAT_CL_TMM_GROUP_INVITE_BROADCAST:	HandleTMMInviteToGroupBroadcast(pkt);	break;
		case NET_CHAT_CL_TMM_GROUP_REJECT_INVITE:	HandleTMMRejectInvite(pkt);	break;
		case NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE:		HandleTMMJoinQueue(pkt);	break;
		case NET_CHAT_CL_TMM_GROUP_REJOIN_QUEUE:	HandleTMMReJoinQueue(pkt);	break;
		case NET_CHAT_CL_TMM_GENERIC_RESPONSE:		HandleTMMGenericResponse(pkt);	break;
		case NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE:		HandleTMMLeaveQueue(pkt);	break;
		case NET_CHAT_CL_TMM_GROUP_UPDATE:			HandleTMMPlayerUpdates(pkt);	break;
		case NET_CHAT_CL_TMM_POPULARITY_UPDATE:		HandleTMMPopularityUpdates(pkt);	break;
		case NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE:	HandleTMMQueueUpdates(pkt);	break;
		case NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE:	HandleTMMMatchFound(pkt);	break;
		case NET_CHAT_CL_TMM_FAILED_TO_JOIN:		HandleTMMJoinFailed(pkt);	break;
		case CHAT_CMD_REQUEST_GAME_INFO:			HandleRequestGameInfo(pkt);	break;
		case NET_CHAT_CL_TMM_REGION_UNAVAILABLE:	HandleTMMRegionUnavailable(pkt);	break;
		case CHAT_CMD_LIST_DATA:					HandleListData(pkt);	break;
		case CHAT_CMD_ACTIVE_STREAMS:				HandleActiveStreams(pkt);	break;
		case CHAT_CMD_PLAYER_SPECTATE_REQUEST:		HandlePlayerSpectateRequest(pkt);	break;
		case NET_CHAT_CL_TMM_EVENTS_INFO:			HandleEventInfo(pkt);	break;
		case NET_CHAT_CL_TMM_SCHEDULED_MATCH_INFO:	HandleScheduledMatchInfo(pkt);	break;
		case NET_CHAT_CL_TMM_SCHEDULED_MATCH_UPDATE:	HandleScheduledMatchUpdates(pkt);	break;
		case NET_CHAT_CL_TMM_SCHEDULED_MATCH_SERVER_INFO:	HandleScheduledMatchFound(pkt);	break;
		case NET_CHAT_CL_TMM_BOT_GROUP_UPDATE:		HandleTMMPlayerBotUpdates(pkt);	break;
		case NET_CHAT_CL_TMM_BOT_GROUP_BOTS:		HandleTMMPlayerBots(pkt);	break;
		case NET_CHAT_CL_TMM_BOT_SPAWN_LOCAL_MATCH:	HandleTMMStartLocalBotMatch();	break;
		case NET_CHAT_CL_TMM_BOT_NO_BOTS_SELECTED:	HandleTMMTeamNoBotsSelected();	break;
		case NET_CHAT_CL_TMM_SCHEDULED_MATCH_LOBBY_INFO:	HandleScheduledMatchLobbyInfo(pkt);	break;
		case NET_CHAT_CL_TMM_LEAVER_INFO:			HandleLeaverInfo(pkt);	break;
		case NET_CHAT_CL_TMM_REQUEST_READY_UP:		HandleRequestReadyUp(pkt);	break;
		case NET_CHAT_CL_TMM_START_LOADING:			HandleTMMStartLoading(pkt);	break;	
		case CHAT_CMD_EXCESSIVE_GAMEPLAY_MESSAGE:	HandleExcessiveGameplayMessage(pkt);	break;
		case CHAT_CMD_MAINTENANCE_MESSAGE:			HandleMaintenanceMessage(pkt);	break;
		case NET_CHAT_CL_TMM_PENDING_MATCH:			HandleTMMPendingMatch(pkt);	break;
		case NET_CHAT_CL_TMM_FAILED_TO_ACCEPT_PENDING_MATCH:	HandleTMMFailedToAcceptPendingMatch(pkt);	break;
		case CHAT_CMD_UPLOAD_STATUS:				HandleUploadStatus(pkt);	break;
		case CHAT_CMD_OPTIONS:						HandleOptions(pkt);	break;
		case CHAT_CMD_LOGOUT:						HandleLogout(pkt);	break;
		case CHAT_CMD_NEW_MESSAGES:					HandleNewMessages(pkt); break;

		case CHAT_CMD_DYANMIC_PRODUCT_LIST:
			Host.ReadDynamicProductListPacket(pkt);
			ChatDynamicProductListUpdate.Trigger(XtoA(true));
			break;

		case NET_CHAT_CL_TMM_CAMPAIGN_STATS:		HandleRankedPlayInfo(pkt); break;
		case NET_CHAT_CL_TMM_LEAVER_STRIKE_WARN:	HandleLeaveStrikeWarning(pkt); break;

		default:
		case NET_CHAT_INVALID:
			Console << L"Invalid command from chat server: " << SHORT_HEX_STR(unCmd) << newl;

			if (unPrevCmd != NET_CHAT_INVALID)
				Console << L"Last valid cmd: " << SHORT_HEX_STR(unPrevCmd) << newl;
			
			HandleDisconnect();
			return false;
		}

		unPrevCmd = unCmd;

		if (pkt.HasFaults())
		{
			Console << L"Bad packet from chat server" << newl;
			HandleDisconnect();
			return false;
		}
	}

	return true;
}


/*====================
  CChatManager::ClearNotifications
  ====================*/
void	CChatManager::ClearNotifications()
{ 
	m_uiNotificationIndex = 0;
	ChatNotificationHistoryPerformCMD.Trigger(_T("ClearItems();")); 
	m_mapNotifications.clear(); 
	ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
}


/*====================
  CChatManager::HandleTMMStartLocalBotMatch
  ====================*/
void CChatManager::HandleTMMStartLocalBotMatch()
{
	TMMStartLocalBotMatch.Trigger(XtoA(true), cc_forceTMMInterfaceUpdate);
}


/*====================
  CChatManager::HandleTMMTeamNoBotsSelected
  ====================*/
void CChatManager::HandleTMMTeamNoBotsSelected()
{
	m_bTMMOtherPlayersReady = false;
	m_bTMMAllPlayersReady = false;
	m_uiTMMStartTime = INVALID_TIME;

	TMMNoBotsSelected.Trigger(XtoA(true), cc_forceTMMInterfaceUpdate);
}


/*====================
  CChatManager::HandleTMMPlayerBots
  ====================*/
void	CChatManager::HandleTMMPlayerBots(CPacket& pkt)
{
	m_bTMMOtherPlayersReady = false;
	m_bTMMAllPlayersReady = false;

	tsvector vTeamBots(5);
	tsvector vEnemyBots(5);
	
	for (uint uiBots(0); uiBots < 5; ++uiBots)
	{
		vTeamBots[uiBots] = pkt.ReadTString();
	}

	for (uint uiBots(0); uiBots < 5; ++uiBots)
	{
		vEnemyBots[uiBots] = pkt.ReadTString();
	}

	if (pkt.HasFaults())
		return;

	for (uint uiBots(0); uiBots < 5; ++uiBots)
	{
		tsvector vParams(2);
		vParams[0] = XtoA(uiBots);

		vParams[1] = vTeamBots[uiBots];
		TMMTeamBotChange.Trigger(vParams, cc_forceTMMInterfaceUpdate);

		vParams[1] = vEnemyBots[uiBots];
		TMMEnemyBotChange.Trigger(vParams, cc_forceTMMInterfaceUpdate);
	}
}


/*====================
  CChatManager::HandleTMMPlayerBotUpdates
  ====================*/
void	CChatManager::HandleTMMPlayerBotUpdates(CPacket& pkt)
{
	m_bTMMOtherPlayersReady = false;
	m_bTMMAllPlayersReady = false;

	byte yTeam(pkt.ReadByte());
	byte ySlot(pkt.ReadByte());
	tstring sBot(pkt.ReadTString());

	if (pkt.HasFaults())
		return;

	tsvector vParams(2);
	vParams[0] = XtoA(ySlot);
	vParams[1] = sBot;

	if (yTeam == 1)
		TMMTeamBotChange.Trigger(vParams, cc_forceTMMInterfaceUpdate);
	else if (yTeam == 2)
		TMMEnemyBotChange.Trigger(vParams, cc_forceTMMInterfaceUpdate);
}


/*====================
  CChatManager::PushNotification
  ====================*/
void	CChatManager::PushNotification(byte yType, const tstring& sParam1, const tstring& sParam2, const tstring& sParam3, const tsvector& vParam4, uint uiExternalNotificationID, bool bSilent, const tstring& sNotificationTime)
{
	if (yType >= NUM_NOTIFICATIONS)
		return;

	// 10 initial params + 19 for the game invite params + 1 for 0 based, + 1 for pushing params to interface silently
	tsvector vParams(33);
	m_cDate = CDate(true);

	vParams[0] = sParam1;								// sParam1, sParam2, sParam3 are used different ways for each notification
	vParams[1] = sParam2;					
	vParams[2] = XtoA(yType);							// type (ENotifyType)
	vParams[3] = g_NotificationInfo[yType].sText;		// stringtable text entry (notify_unknown)
	vParams[4] = g_NotificationInfo[yType].sType;		// generic notify type (notfication_generic_info)
	vParams[5] = g_NotificationInfo[yType].sAction;		// specific action template within the notifytype
	
	// the time needs to be overriden because this notification is stored in the DB and we want that time
	if (sNotificationTime.empty())
		vParams[6] = XtoA(m_cDate.GetMonth(), FMT_PADZERO, 2) + _T("/") + XtoA(m_cDate.GetDay(), FMT_PADZERO, 2) + _T("  ") + m_cDate.GetTimeString(TIME_NO_SECONDS | TIME_TWELVE_HOUR);
	else
		vParams[6] = sNotificationTime;
		
	vParams[7] = XtoA(uiExternalNotificationID);		// external notification ID of notification in DB
	vParams[8] = XtoA(IncrementNotificationIndex());	// internal notification ID starts at 0 on signon and increases for each additional notification
	vParams[9] = sParam3;
	vParams[30] = XtoA(bSilent);						// show this notification when logging in or keep it silent so it populates the notification history

	switch (yType)
	{
		case NOTIFY_TYPE_UNKNOWN:
			break;	
		case NOTIFY_TYPE_BUDDY_ADDER:
		{
			ChatNotificationBuddy.Trigger(vParams);
			AddNotification(GetNotificationIndex(), vParams);
			ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
			break;
		}
		case NOTIFY_TYPE_BUDDY_ADDED:
		{
			ChatNotificationBuddy.Trigger(vParams);
			AddNotification(GetNotificationIndex(), vParams);
			ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
			break;
		}
		case NOTIFY_TYPE_BUDDY_REMOVER:
		{
			break;
		}
		case NOTIFY_TYPE_BUDDY_REMOVED:
		{
			break;
		}
		case NOTIFY_TYPE_BUDDY_ONLINE:
		{
			ChatNotificationBuddy.Trigger(vParams);
			break;
		}
		case NOTIFY_TYPE_BUDDY_LEFT_GAME:
		{
			ChatNotificationBuddy.Trigger(vParams);
			break;
		}
		case NOTIFY_TYPE_BUDDY_OFFLINE:
		{
			ChatNotificationBuddy.Trigger(vParams);
			break;
		}
		case NOTIFY_TYPE_CLAN_RANK:
		{
			ChatNotificationClan.Trigger(vParams);
			AddNotification(GetNotificationIndex(), vParams);
			ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
			break;
		}
		case NOTIFY_TYPE_CLAN_ADD:
		{
			ChatNotificationClan.Trigger(vParams);
			AddNotification(GetNotificationIndex(), vParams);
			ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
			break;
		}
		case NOTIFY_TYPE_CLAN_REMOVE:
		{
			ChatNotificationClan.Trigger(vParams);
			AddNotification(GetNotificationIndex(), vParams);
			ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
			break;
		}
		case NOTIFY_TYPE_CLAN_ONLINE:
		{
			ChatNotificationClan.Trigger(vParams);
			break;
		}
		case NOTIFY_TYPE_CLAN_LEFT_GAME:
		{
			ChatNotificationClan.Trigger(vParams);
			break;
		}
		case NOTIFY_TYPE_CLAN_OFFLINE:
		{
			ChatNotificationClan.Trigger(vParams);
			break;
		}
		case NOTIFY_TYPE_UPDATE:
		{
			AddNotification(GetNotificationIndex(), vParams);
			ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
			ChatNotificationMessage.Trigger(vParams);
			break;
		}
		case NOTIFY_TYPE_ACTIVE_STREAM:
		{
			AddNotification(GetNotificationIndex(), vParams);
			ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
			ChatNotificationMessage.Trigger(vParams);
			break;
		}
		case NOTIFY_TYPE_REPLAY_AVAILABLE:
		{
			AddNotification(GetNotificationIndex(), vParams);
			ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
			ChatNotificationMessage.Trigger(vParams);
			break;
		}
		case NOTIFY_TYPE_GENERIC:
			break;
		case NOTIFY_TYPE_IM:
		{
			ChatReceivedIMCount.Trigger(XtoA(AddReceivedIM()));
			AddUnreadIM(sParam1);			
			ChatUnreadIMCount.Trigger(XtoA(GetUnreadIMCount()));
			ChatOpenIMCount.Trigger(XtoA(GetOpenIMCount()));
			
			// throttle IM notifications from popping up on every IM, to every 2 minutes
			if (m_uiLastIMNotificationTime < K2System.Milliseconds())
			{
				m_uiLastIMNotificationTime = K2System.Milliseconds() + 120000;
				ChatNotificationBuddy.Trigger(vParams);
			}
			break;
		}
		case NOTIFY_TYPE_BUDDY_JOIN_GAME:
		case NOTIFY_TYPE_CLAN_JOIN_GAME:
		case NOTIFY_TYPE_GAME_INVITE:		
		case NOTIFY_TYPE_SELF_JOIN_GAME:
		{
			if (!vParam4.empty())
			{		
				vParams[10] = vParam4[0];		// Address:Port
				vParams[11] = StringReplace(vParam4[1], _T("'"), _T("`")); // Game Name - Replace ' with ` to avoid UI errors
				vParams[12] = vParam4[2];		// Buddy Name/Clan Member Name/Inviter Name/Self
				vParams[13] = vParam4[3];		// Server Region
				vParams[14] = vParam4[4];		// Game Mode
				vParams[15] = vParam4[5];		// Team Size			
				vParams[16] = vParam4[6];		// Map Name
				vParams[17] = vParam4[7];		// Tier - Noobs Only (0), Noobs Allowed (1), Pro (2) (Deprecated)
				vParams[18] = vParam4[8];		// 0 - Unofficial, 1 - Official w/ stats, 2 - Official w/o stats
				vParams[19] = vParam4[9];		// No Leavers (1), Leavers (0)
				vParams[20] = vParam4[10];		// Private (1), Not Private (0)									
				vParams[21] = _T("0");			// All Heroes (1), Not All Heroes (0) -- (NOTE: Deprecated)
				vParams[22] = vParam4[12];		// Casual Mode (1), Not Casual Mode (0)
				vParams[23] = vParam4[13];		// Force Random (1), Not Force Random (0) -- (NOTE: Deprecated)
				vParams[24] = vParam4[14];		// Auto Balanced (1), Non Auto Balanced (0)
				vParams[25] = vParam4[15];		// Advanced Options	(1), No Advanced Options (0)
				vParams[26] = vParam4[16];		// Min PSR
				vParams[27] = vParam4[17];		// Max PSR
				vParams[28] = vParam4[18];		// Dev Heroes (1), Non Dev Heroes (0)
				vParams[29] = vParam4[19];		// Hardcore (1), Non Hardcore (0)
				vParams[31] = vParam4[20];		// Verified Only (1), Everyone (0)
				vParams[32] = vParam4[21];		// Gated (1), Not Gated (0)

				AddNotification(GetNotificationIndex(), vParams);
				ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
				
				switch (yType)
				{
					case NOTIFY_TYPE_BUDDY_JOIN_GAME:
						ChatNotificationBuddy.Trigger(vParams);
						break;
					case NOTIFY_TYPE_CLAN_JOIN_GAME:
						ChatNotificationClan.Trigger(vParams);
						break;
					case NOTIFY_TYPE_GAME_INVITE:
						ChatNotificationInvite.Trigger(vParams);
						break;
					case NOTIFY_TYPE_SELF_JOIN_GAME:				
						ChatNotificationInvite.Trigger(vParams);
						break;
					default:
						break;
				}
			}
			break;
		}
		case NOTIFY_TYPE_BUDDY_REQUESTED_ADDER:
		{
			AddNotification(GetNotificationIndex(), vParams);
			ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
			ChatNotificationBuddy.Trigger(vParams);
			break;
		}
		case NOTIFY_TYPE_BUDDY_REQUESTED_ADDED:
		{
			AddNotification(GetNotificationIndex(), vParams);
			ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
			ChatNotificationBuddy.Trigger(vParams);
			break;
		}
		case NOTIFY_TYPE_TMM_GROUP_INVITE:
		{
			if (!vParam4.empty())
			{		
				vParams[10] = vParam4[0];		// Map Names - caldavar, grimmscrossing, darkwoodvale, etc, can be multiple and pipe (|) delimited
				vParams[11] = vParam4[1];		// Game Type - 0 = Normal, 1 = Casual
				vParams[12] = vParam4[2];		// Game Modes - ap, sd, bd, bp, ar, pipe (|) delimited
				vParams[13] = vParam4[3];		// Regions - USE, USW, EU, pipe (|) delimited
				
				AddNotification(GetNotificationIndex(), vParams);
				ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
				ChatNotificationGroupInvite.Trigger(vParams);
			}
			break;		
		}
		case NOTIFY_TYPE_CLOUDSAVE_UPLOAD_SUCCESS:
		case NOTIFY_TYPE_CLOUDSAVE_UPLOAD_FAIL:
		case NOTIFY_TYPE_CLOUDSAVE_DOWNLOAD_SUCCESS:
		case NOTIFY_TYPE_CLOUDSAVE_DOWNLOAD_FAIL:
		{
			AddNotification(GetNotificationIndex(), vParams);
			ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
			ChatNotificationMessage.Trigger(vParams);
			break;
		}
		default:
		{
			ChatNotificationMessage.Trigger(vParams);
			break;
		}
	}
}


/*====================
  CChatManager::RemoveNotification
  ====================*/
void	CChatManager::RemoveNotification(uint uiIndex)
{
	NotificationMap_it it(m_mapNotifications.find(uiIndex));
	
	if (it != m_mapNotifications.end())
	{
		// this vParam size should match the vParam in ChatPushNotification
		tsvector vParams(31);
		
		vParams = it->second;
				
		// if the external ID isn't set...
		if (AtoI(vParams[7]) == 0)
		{
			// then remove the notification
			if (it != m_mapNotifications.end()) 
				STL_ERASE(m_mapNotifications, it);
				
			// update the notification count and tell the interface to remove this listitem
			const tstring sEraseNotification(_T("EraseListItemByValue('") + XtoA(uiIndex) + _T("');"));
			
			ChatNotificationHistoryPerformCMD.Trigger(sEraseNotification);
			ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
		}
		else
			RequestRemoveNotification(AtoI(vParams[8]), AtoI(vParams[7]));
	}
}


/*====================
  CChatManager::RemoveExternalNotification
  ====================*/
void	CChatManager::RemoveExternalNotification(uint uiIndex)
{
	NotificationMap_it it(m_mapNotifications.find(uiIndex));
	if (it != m_mapNotifications.end()) 
		STL_ERASE(m_mapNotifications, it);
		
	// we tried to remove a notification, found it was an external notification stored in the DB,
	// got back the response, and now we are removing this and updating the interface
	ChatNotificationCount.Trigger(XtoA(GetNotificationCount()));
	
	const tstring sEraseNotification(_T("EraseListItemByValue('") + XtoA(uiIndex) + _T("');"));
	ChatNotificationHistoryPerformCMD.Trigger(sEraseNotification);
}


/*====================
  CChatManager::RequestSaveNotification (Deprecated, not being used but could be tweaked to save notifications inefficiently)
  ====================*/
void CChatManager::RequestSaveNotification(byte yType, const tstring& sParam1, const tstring& sParam2, const tstring& sParam3, const tsvector vParam4)
{
	if (yType >= NUM_NOTIFICATIONS)
		return;

	// 10 initial params + 19 for the game invite params + 1 for 0 based, this should match the vParam in ChatPushNotification - 1
	tsvector vParams(30);
	m_cDate = CDate(true);

	vParams[0] = sParam1;								// sParam1, sParam2, sParam3 are used different ways for each notification
	vParams[1] = sParam2;					
	vParams[2] = XtoA(yType);							// type (ENotifyType)
	vParams[3] = g_NotificationInfo[yType].sText;					// stringtable text entry (notify_unknown)
	vParams[4] = g_NotificationInfo[yType].sType;					// generic notify type (notfication_generic_info)
	vParams[5] = g_NotificationInfo[yType].sAction;				// specific action template within the notifytype
	vParams[6] = XtoA(m_cDate.GetMonth(), FMT_PADZERO, 2) + _T("/") + XtoA(m_cDate.GetDay(), FMT_PADZERO, 2) + _T("  ") + m_cDate.GetTimeString(TIME_NO_SECONDS | TIME_TWELVE_HOUR);
	vParams[7] = _T("");								// external notification ID of notification in DB
	vParams[8] = XtoA(IncrementNotificationIndex());	// internal notification ID starts at 0 on signon and increases for each additional notification
	vParams[9] = sParam3;
						
	switch (yType)
	{
		case NOTIFY_TYPE_BUDDY_REQUESTED_ADDER:
		case NOTIFY_TYPE_BUDDY_REQUESTED_ADDED:
		case NOTIFY_TYPE_BUDDY_ADDER:
		case NOTIFY_TYPE_BUDDY_ADDED:
		case NOTIFY_TYPE_BUDDY_REMOVER:
		case NOTIFY_TYPE_BUDDY_REMOVED:
		case NOTIFY_TYPE_CLAN_RANK:
		case NOTIFY_TYPE_CLAN_ADD:
		case NOTIFY_TYPE_CLAN_REMOVE:
			break;
		case NOTIFY_TYPE_BUDDY_JOIN_GAME:
		case NOTIFY_TYPE_CLAN_JOIN_GAME:
		case NOTIFY_TYPE_GAME_INVITE:		
		case NOTIFY_TYPE_SELF_JOIN_GAME:
		{
			if (!vParam4.empty())
			{
				vParams[10] = vParam4[0];	// Address:Port
				vParams[11] = vParam4[1];	// Game Name
				vParams[12] = vParam4[2];	// Buddy Name/Clan Member Name/Inviter/Self
				vParams[13] = vParam4[3];	// Server Region
				vParams[14] = vParam4[4];	// Game Mode
				vParams[15] = vParam4[5];	// Team Size			
				vParams[16] = vParam4[6];	// Map Name
				vParams[17] = vParam4[7];	// Tier - Noobs Only (0), Noobs Allowed (1), Pro (2) (Deprecated)
				vParams[18] = vParam4[8];	// 0 - Unofficial, 1 - Official w/ stats, 2 - Official w/o stats
				vParams[19] = vParam4[9];	// No Leavers (1), Leavers (0)
				vParams[20] = vParam4[10];	// Private (1), Not Private (0)									
				vParams[21] = vParam4[11];	// All Heroes (1), Not All Heroes (0)
				vParams[22] = vParam4[12];	// Casual Mode (1), Not Casual Mode (0)
				vParams[23] = vParam4[13];	// Force Random (1), Not Force Random (0) -- (NOTE: Deprecated)
				vParams[24] = vParam4[14];	// Auto Balanced (1), Non Auto Balanced (0)
				vParams[25] = vParam4[15];	// Advanced Options	(1), No Advanced Options (0)
				vParams[26] = vParam4[16];	// Min PSR
				vParams[27] = vParam4[17];	// Max PSR
				vParams[28] = vParam4[18];	// Dev Heroes (1), Non Dev Heroes (0)
				vParams[29] = vParam4[19];	// Hardcore (1), Non Hardcore (0)				
			}	
			break;		
		}
		default:
		{
			break;
		}
	}
	
	// escape the pipes with the escape sequence
	for (int i = 0; i < 30; i++)
		vParams[i] = StringReplace(vParams[i], SEARCH_PIPES, REPLACE_PIPES);
	
	tstring sNotification(ConcatinateArgs(vParams, _T("|")));
	
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;		

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=test_create_notification");
	pHTTPRequest->AddVariable(L"account_id[]", m_uiAccountID);
	pHTTPRequest->AddVariable(L"cookie", m_sCookie);
	pHTTPRequest->AddVariable(L"type", yType);
	pHTTPRequest->AddVariable(L"params[notification]", sNotification);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_SAVE_NOTIFICATION, TSNULL, TSNULL));
	m_lHTTPRequests.push_back(pNewRequest);	
}


/*====================
  CChatManager::RequestSaveNotification
  ====================*/
void CChatManager::RequestRemoveNotification(uint uiInternalNotificationID, uint uiExternalNotificationID)
{
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;		

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=delete_notification");
	pHTTPRequest->AddVariable(L"account_id", m_uiAccountID);
	pHTTPRequest->AddVariable(L"cookie", m_sCookie);
	pHTTPRequest->AddVariable(L"internal_id", uiInternalNotificationID);
	pHTTPRequest->AddVariable(L"notify_id", uiExternalNotificationID);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_REMOVE_NOTIFICATION, TSNULL, TSNULL));
	m_lHTTPRequests.push_back(pNewRequest);	
}


/*====================
  CChatManager::RequestRemoveAllNotifications
  ====================*/
void CChatManager::RequestRemoveAllNotifications()
{
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;		

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=remove_all_notifications");
	pHTTPRequest->AddVariable(L"account_id", m_uiAccountID);
	pHTTPRequest->AddVariable(L"cookie", m_sCookie);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_REMOVE_ALL_NOTIFICATIONS, TSNULL, TSNULL));
	m_lHTTPRequests.push_back(pNewRequest);	
}


/*====================
  CChatManager::ParseNotification
  ====================*/
void CChatManager::ParseNotification(const tstring& sNotification, uint uiExternalNotificationID, bool bSilent)
{
	// this is used to parse out notifications retrieved from the db that are delimited by pipes "|"
	// some values for vParams are generated on the fly, while others need to be read directly from the db response
	tstring sParam;
	tstring sParam1;
	tstring sParam2;
	tstring sParam3;
	tstring sNotificationTime;
	byte yType(0);
	static tsvector vParams(20);
	uint uiIndex(0);
	
	const tsvector vNotificationInfo(TokenizeString(sNotification, _T('|')));

	for (tsvector_cit it(vNotificationInfo.begin()), itEnd(vNotificationInfo.end()); it != itEnd; ++it)
	{
		// unescape the pipes in case there were any in the game name or some other field
		sParam = StringReplace(*it, REPLACE_PIPES, SEARCH_PIPES);
		
		switch(uiIndex)
		{
			case 0:
				sParam1 = sParam;
				break;
			case 1:
				sParam2 = sParam;
				break;
			case 2:
				yType = byte(AtoI(sParam));
				break;
			case 6:
				sNotificationTime = sParam;
				break;
			case 9:
				sParam3 = sParam;
				break;
			case 10:
				vParams[0] = sParam;
				break;
			case 11:
				vParams[1] = sParam;
				break;
			case 12:
				vParams[2] = sParam;
				break;
			case 13:
				vParams[3] = sParam;
				break;
			case 14:
				vParams[4] = sParam;
				break;
			case 15:
				vParams[5] = sParam;
				break;
			case 16:
				vParams[6] = sParam;
				break;
			case 17:
				vParams[7] = sParam;
				break;
			case 18:
				vParams[8] = sParam;
				break;
			case 19:
				vParams[9] = sParam;
				break;
			case 20:
				vParams[10] = sParam;
				break;
			case 21:
				vParams[11] = sParam;
				break;
			case 22:
				vParams[12] = sParam;
				break;
			case 23:
				vParams[13] = sParam;
				break;
			case 24:
				vParams[14] = sParam;
				break;
			case 25:
				vParams[15] = sParam;
				break;
			case 26:
				vParams[16] = sParam;
				break;
			case 27:
				vParams[17] = sParam;
				break;
			case 28:
				vParams[18] = sParam;
				break;
			case 29:
				vParams[19] = sParam;
				break;
			default:		
				break;
		}			
					
		uiIndex++;				
	}
	
	if (yType == NOTIFY_TYPE_SELF_JOIN_GAME)
		bSilent = true;
				
	PushNotification(yType, sParam1, sParam2, sParam3, vParams, uiExternalNotificationID, bSilent, sNotificationTime);	
}


/*====================
  CChatManager::HandlePing
  ====================*/
void	CChatManager::HandlePing()
{
	if (!IsConnected())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_PONG;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::HandleDisconnect
  ====================*/
void	CChatManager::HandleDisconnect()
{
	if (m_uiAccountID == INVALID_ACCOUNT || m_eStatus == CHAT_CLIENT_STATUS_DISCONNECTED)
		return;

	m_sockChat.Close();
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_disconnected")));

	m_eStatus = CHAT_CLIENT_STATUS_DISCONNECTED;
	LeaveTMMGroup(true, _T("disconnected"));

	LeaveScheduledMatch(true, _T("disconnected"));

	tsvector vParams(2);	
	vParams[0] = vParams[1] = TSNULL;
	ChatChanNumUsers.Trigger(vParams);
	
	for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
	{
		it->second.yStatus = CHAT_CLIENT_STATUS_DISCONNECTED;
	}

	for (ChatChannelMap_it it(m_mapChannels.begin()); it != m_mapChannels.end(); it++)
	{
		if (it->second.uiFlags & CHAT_CHANNEL_FLAG_UNJOINABLE)
		{
			RemoveUnreadChannel(it->first);
			m_setChannelsIn.erase(it->first);			
			ChatLeftChannel.Trigger(XtoA(it->first));
		}
	}
				
	vParams[0] = _T("0");
	vParams[1] = TSNULL;
	ChatUsersOnline.Trigger(vParams);

	vParams[0] = TSNULL;
	vParams[1] = _T("ClearItems();");
	ChatUserEvent.Trigger(vParams);

	ClearCafeList();

	SetFriendlyChat(false);

	if (m_uiConnectRetries < chat_maxReconnectAttempts)
	{
		++m_uiConnectRetries;
		m_uiNextReconnectTime = K2System.Milliseconds() + M_Randnum(15000, 45000);
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_reconnecting_delay"), _T("attempt"), XtoA(m_uiConnectRetries), _T("maxattempts"), XtoA(chat_maxReconnectAttempts)));
	}
}


/*====================
  CChatManager::HandleChannelChange
  ====================*/
void	CChatManager::HandleChannelChange(CPacket& pkt)
{
	wstring sChannel(pkt.ReadWString());
	uint uiChannelID(pkt.ReadInt());
	byte yChannelFlags(pkt.ReadByte());
	wstring sTopic(pkt.ReadWString());
	uint uiNumAdmins(pkt.ReadInt());

	m_mapChannelNameToFlag[sChannel] = yChannelFlags;

	if (chat_debugInterface)
		Console.UI << _T("HandleChannelChange ") << uiChannelID << _T(" ") << QuoteStr(sChannel) << newl;

	m_mapChannels[uiChannelID].sChannelName = sChannel;
	m_mapChannels[uiChannelID].sTopic = sTopic;
	m_mapChannels[uiChannelID].uiFlags = yChannelFlags;
	m_mapChannels[uiChannelID].bUnread = false;
	
	static tsvector vParams(18);
	static tsvector vMiniParams(2);
	
	if (!(yChannelFlags & CHAT_CHANNEL_FLAG_HIDDEN))
	{
		vMiniParams[0] = XtoA(uiChannelID);
		vMiniParams[1] = sChannel;

		ChatNewChannel.Trigger(vMiniParams);
	}

	if (yChannelFlags & CHAT_CHANNEL_FLAG_SERVER)
	{
		cc_curGameChannel = sChannel;
		cc_curGameChannelID = uiChannelID;
	}

	m_setChannelsIn.insert(uiChannelID);
	m_mapChannels[uiChannelID].mapAdmins.clear();

	// Read admin list
	for (uint uiLoop(0); uiLoop < uiNumAdmins; uiLoop++)
	{
		if (pkt.HasFaults())
			break;

		uint uiID(pkt.ReadInt());
		byte yLevel(pkt.ReadByte());

		m_mapChannels[uiChannelID].mapAdmins.insert(ChatAdminPair(uiID, yLevel));
	}

	uint uiNumUsers(pkt.ReadInt());

	m_mapChannels[uiChannelID].uiUserCount = uiNumUsers + 1;

	// These stay the same throughout the rest of the function
	vParams[0] = vMiniParams[0] = sChannel;	
	
	vMiniParams[1] = _T("ClearItems();");
	ChatUserEvent.Trigger(vMiniParams);		

	if (IsCafeChannel(sChannel))
		ClearCafeList();	

	for (uint uiLoop(0); uiLoop < uiNumUsers; uiLoop++)
	{
		if (pkt.HasFaults())
			break;

		wstring sName(pkt.ReadWString());
		uint uiAccountID(pkt.ReadInt());
		byte yStatus(pkt.ReadByte());
		byte yUserFlags(pkt.ReadByte());
		uint uiChatSymbol(Host.LookupChatSymbol(pkt.ReadTString()));
		uint uiChatNameColor(Host.LookupChatNameColor(pkt.ReadTString()));
		
		uint uiAccountIcon;
		int iAccountIconSlot;

		GetAccountIconInfo(pkt.ReadTString(), uiAccountIcon, iAccountIconSlot);

		uint uiAscensionLevel(pkt.ReadInt());

		ChatClientMap_it findit(m_mapUserList.find(uiAccountID));

		if (findit == m_mapUserList.end())
		{
			SChatClient cNewClient;
			
			cNewClient.sName = sName;
			cNewClient.yStatus = yStatus;
			cNewClient.uiAccountID = uiAccountID;
			cNewClient.yFlags = yUserFlags;
			cNewClient.uiChatSymbol = uiChatSymbol;
			cNewClient.uiChatNameColor = uiChatNameColor;
			cNewClient.uiAccountIcon = uiAccountIcon;
			cNewClient.iAccountIconSlot = iAccountIconSlot;
			cNewClient.uiAscensionLevel = uiAscensionLevel;

			uint uiChatNameColor2(uiChatNameColor);

			if (cNewClient.yFlags & CHAT_CLIENT_IS_STAFF && uiChatNameColor2 == INVALID_INDEX)
			{
				uint uiDevChatNameColor(Host.LookupChatNameColor(_CTS("s2logo")));
				if (uiDevChatNameColor != INVALID_INDEX)
					uiChatNameColor2 = uiDevChatNameColor;
			}
			if (cNewClient.yFlags & CHAT_CLIENT_IS_PREMIUM && uiChatNameColor2 == INVALID_INDEX)
			{
				uint uiGoldChatNameColor(Host.LookupChatNameColor(_CTS("goldshield")));
				if (uiGoldChatNameColor != INVALID_INDEX)
					uiChatNameColor2 = uiGoldChatNameColor;
			}

			if (uiChatNameColor2 != INVALID_INDEX)
				cNewClient.uiSortIndex = Host.GetChatNameColorSortIndex(uiChatNameColor2);
			else
				cNewClient.uiSortIndex = DEFAULT_CHAT_NAME_COLOR_SORT_INDEX;

			findit = m_mapUserList.insert(ChatClientPair(uiAccountID, cNewClient)).first;
			m_mapNameToAccountID.insert(pair<wstring, uint>(LowerString(StripClanTag(sName)), uiAccountID));
		}
		else
		{
			UpdateClientChannelStatus(TSNULL, sName, uiAccountID, yStatus, yUserFlags, uiChatSymbol, uiChatNameColor, uiAccountIcon, iAccountIconSlot, uiAscensionLevel);
		}
		if (IsCafeChannel(sChannel))
			m_setCafeList.insert(uiAccountID);

		if (findit->second.yStatus >= CHAT_CLIENT_STATUS_CONNECTED)
		{
			findit->second.setChannels.insert(uiChannelID);

			if (!(yChannelFlags & CHAT_CHANNEL_FLAG_HIDDEN))
			{				
				vParams[1] = sName;
				vParams[2] = XtoA(GetAdminLevel(uiChannelID, findit->second.uiAccountID));
				vParams[3] = XtoA(findit->second.yStatus > CHAT_CLIENT_STATUS_CONNECTED, true);
				vParams[4] = XtoA((findit->second.yFlags & CHAT_CLIENT_IS_PREMIUM) != 0, true);
				vParams[5] = XtoA(findit->second.uiAccountID);
				vParams[6] = Host.GetChatSymbolTexturePath(findit->second.uiChatSymbol);
				vParams[7] = Host.GetChatNameColorTexturePath(findit->second.uiChatNameColor);
				vParams[8] = Host.GetChatNameColorString(findit->second.uiChatNameColor);
				vParams[9] = Host.GetChatNameColorIngameString(findit->second.uiChatNameColor);
				vParams[10] = Host.GetAccountIconTexturePath(findit->second.uiAccountIcon, findit->second.iAccountIconSlot, findit->second.uiAccountID);
				vParams[11] = XtoA(findit->second.uiSortIndex);
				vParams[12] = XtoA(Host.GetChatNameGlow(findit->second.uiChatNameColor));
				vParams[13] = Host.GetChatNameGlowColorString(findit->second.uiChatNameColor);
				vParams[14] = Host.GetChatNameGlowColorIngameString(findit->second.uiChatNameColor);
				vParams[15] = XtoA(uiAscensionLevel);
				vParams[16] = Host.GetChatNameColorFont(findit->second.uiChatNameColor);
				vParams[17] = XtoA(Host.GetChatNameBackgroundGlow(findit->second.uiChatNameColor));
				ChatUserNames.Trigger(vParams);
			}

			if (m_eStatus > CHAT_CLIENT_STATUS_CONNECTED && (yChannelFlags & CHAT_CHANNEL_FLAG_SERVER))
				AddToRecentlyPlayed(sName);
		}
	}

	// Add us to the channel list
	ChatClientMap_it findit(m_mapUserList.find(m_uiAccountID));

	if (findit == m_mapUserList.end())
	{
		SChatClient cNewClient;
		cNewClient.uiAccountID = m_uiAccountID;
		findit = m_mapUserList.insert(ChatClientPair(m_uiAccountID, cNewClient)).first;
		m_mapNameToAccountID.insert(pair<wstring, uint>(LowerString(StripClanTag(m_mapUserList[m_uiAccountID].sName)), m_uiAccountID));
	}

	if (IsCafeChannel(sChannel))
	{
		m_setCafeList.insert(m_uiAccountID);
		RefreshCafeList();
	}


	findit->second.yStatus = m_eStatus;
	findit->second.setChannels.insert(uiChannelID);

	if (!(yChannelFlags & CHAT_CHANNEL_FLAG_HIDDEN))
	{
		vParams[1] = m_mapUserList[m_uiAccountID].sName;
		vParams[2] = XtoA(GetAdminLevel(uiChannelID, findit->first));
		vParams[3] = XtoA(findit->second.yStatus > CHAT_CLIENT_STATUS_CONNECTED, true);
		vParams[4] = XtoA((findit->second.yFlags & CHAT_CLIENT_IS_PREMIUM) != 0, true);
		vParams[5] = XtoA(findit->second.uiAccountID);
		vParams[6] = Host.GetChatSymbolTexturePath(findit->second.uiChatSymbol);
		vParams[7] = Host.GetChatNameColorTexturePath(findit->second.uiChatNameColor);
		vParams[8] = Host.GetChatNameColorString(findit->second.uiChatNameColor);
		vParams[9] = Host.GetChatNameColorIngameString(findit->second.uiChatNameColor);
		vParams[10] = Host.GetAccountIconTexturePath(findit->second.uiAccountIcon, findit->second.iAccountIconSlot, findit->second.uiAccountID);
		vParams[11] = XtoA(findit->second.uiSortIndex);
		vParams[12] = XtoA(Host.GetChatNameGlow(findit->second.uiChatNameColor));
		vParams[13] = Host.GetChatNameGlowColorString(findit->second.uiChatNameColor);
		vParams[14] = Host.GetChatNameGlowColorIngameString(findit->second.uiChatNameColor);
		vParams[15] = XtoA(findit->second.uiAscensionLevel);
		vParams[16] = Host.GetChatNameColorFont(findit->second.uiChatNameColor);
		vParams[17] = XtoA(Host.GetChatNameBackgroundGlow(findit->second.uiChatNameColor));
		ChatUserNames.Trigger(vParams);

		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_new_channel"), _T("channel"), sChannel), sChannel);
		
		vMiniParams[1] = _T("SortListboxSortIndex();");				
		ChatUserEvent.Trigger(vMiniParams);

		vMiniParams[1] = XtoA(uiNumUsers + 1);
		ChatChanNumUsers.Trigger(vMiniParams);

		vMiniParams[1] = sTopic;
		ChatChanTopic.Trigger(vMiniParams);

		if (!sTopic.empty())
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_topic"), _T("topic"), sTopic), sChannel);

		bool bMatchChannel(false);
		for (uiset_it it(m_setChannelsIn.begin()); it != m_setChannelsIn.end(); ++it)
		{
			ChatChannelMap_it itFind(m_mapChannels.find(*it));
			if (itFind == m_mapChannels.end())
				continue;

			if (itFind->second.uiFlags & CHAT_CHANNEL_FLAG_SERVER)
			{
				bMatchChannel = true;
				break;
			}
		}

		// Don't change focus if this is a new match channel or if we already 
		// have a match channel and this is a general use channel
		if (!(yChannelFlags & CHAT_CHANNEL_FLAG_SERVER) && !(yChannelFlags & CHAT_CHANNEL_FLAG_GENERAL_USE && bMatchChannel))
			SetFocusedChannel(uiChannelID);
		else if (sChannel.substr(0, 6) == _T("Group "))
		{
			// If this channel was a group channel then set the focus just this time
			SetFocusedChannel(sChannel, true);
		}
		else if (sChannel.substr(0, 16) == _T("Scheduled Match "))
		{
			// If this channel was a scheduled match channel then set the focus just this time
			SetFocusedChannel(sChannel, true);
		}
	}

	UpdateChannel(uiChannelID);
}


/*====================
  CChatManager::HandleChannelJoin
  ====================*/
void	CChatManager::HandleChannelJoin(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	wstring sName(pkt.ReadWString());
	uint uiAccountID(pkt.ReadInt());
	uint uiAccountIcon(0);
	int iAccountIconSlot(0);
	byte yStatus(pkt.ReadByte());
	byte yFlags(pkt.ReadByte());
	uint uiChatSymbol(Host.LookupChatSymbol(pkt.ReadTString()));
	uint uiChatNameColor(Host.LookupChatNameColor(pkt.ReadTString()));
	GetAccountIconInfo(pkt.ReadTString(), uiAccountIcon, iAccountIconSlot);
	uint uiAscensionLevel(pkt.ReadInt());

	tstring sChannel(GetChannelName(uiChannelID));

	if (sChannel.empty())
		return;
		
	static tsvector vMiniParams(2);
	
	// These stay the same throughout the rest of the function
	vMiniParams[0] = sChannel;

	ChatClientMap_it findit(m_mapUserList.find(uiAccountID));

	if (findit == m_mapUserList.end())
	{
		SChatClient cNewClient;
		cNewClient.sName = sName;
		cNewClient.uiAccountID = uiAccountID;
		findit = m_mapUserList.insert(ChatClientPair(uiAccountID, cNewClient)).first;
		m_mapNameToAccountID.insert(pair<wstring, uint>(LowerString(StripClanTag(sName)), uiAccountID));
	}

	if (yStatus >= CHAT_CLIENT_STATUS_CONNECTED && findit->second.setChannels.find(uiChannelID) == findit->second.setChannels.end())
	{
		findit->second.setChannels.insert(uiChannelID);

		if (m_eStatus > CHAT_CLIENT_STATUS_CONNECTED && m_mapChannels.find(uiChannelID) != m_mapChannels.end() && (m_mapChannels[uiChannelID].uiFlags & CHAT_CHANNEL_FLAG_SERVER))
			AddToRecentlyPlayed(sName);

		m_mapChannels[uiChannelID].uiUserCount++;

		vMiniParams[1] = XtoA(m_mapChannels[uiChannelID].uiUserCount);		
		ChatChanNumUsers.Trigger(vMiniParams);
	}

	UpdateClientChannelStatus(sChannel, sName, uiAccountID, yStatus, yFlags, uiChatSymbol, uiChatNameColor, uiAccountIcon, iAccountIconSlot, uiAscensionLevel);
}


/*====================
  CChatManager::HandleChannelMessage
  ====================*/
void	CChatManager::HandleChannelMessage(CPacket& pkt)
{
	uint uiAccountID(pkt.ReadInt());
	uint uiChannelID(pkt.ReadInt());
	wstring sMessage(pkt.ReadWString());

	ChatClientMap_it it(m_mapUserList.find(uiAccountID));

	if (it == m_mapUserList.end() || sMessage.empty())
		return;

	if (IsIgnored(it->first))
		return;

	// only play channel sounds for the active channel
	if (m_uiFocusedChannel != uiChannelID)
		AddUnreadChannel(uiChannelID);
	else
		ChatRecievedChannelMessage.Trigger(GetChannelName(uiChannelID));

	AddIRCChatMessage(CHAT_MESSAGE_IRC, Translate(_T("chat_channel_message"), _T("sender"), it->second.sName, _T("message"), sMessage), GetChannelName(uiChannelID), true);
}


/*====================
  CChatManager::HandleChannelLeave
  ====================*/
void	CChatManager::HandleChannelLeave(CPacket& pkt)
{
	uint uiAccountID(pkt.ReadInt());
	uint uiChannelID(pkt.ReadInt());

	tstring sChannel(GetChannelName(uiChannelID));

	if (sChannel.empty())
		return;

	if (uiAccountID == m_uiAccountID)
	{
		// We left or were removed from the channel
		RemoveUnreadChannel(uiChannelID);
		m_setChannelsIn.erase(uiChannelID);
		ChatLeftChannel.Trigger(XtoA(uiChannelID));
		if (IsCafeChannel(sChannel))
		{
			ClearCafeList();
			RefreshCafeList();
		}
		return;
	}

	ChatClientMap_it it(m_mapUserList.find(uiAccountID));

	if (it == m_mapUserList.end())
		return;

	if (it->second.setChannels.find(uiChannelID) != it->second.setChannels.end())
	{
		static tsvector vParams(2);
		vParams[0] = sChannel;
		vParams[1] = _T("EraseListItemByValue('") + it->second.sName + _T("');");
		ChatUserEvent.Trigger(vParams);

		if (IsCafeChannel(sChannel))
		{
			m_setCafeList.erase(uiAccountID);
			RefreshCafeList();
		}

		it->second.setChannels.erase(uiChannelID);
		m_mapChannels[uiChannelID].uiUserCount--;

		vParams[1] = XtoA(m_mapChannels[uiChannelID].uiUserCount);
		ChatChanNumUsers.Trigger(vParams);
	}
}


/*====================
  CChatManager::HandleWhisper
  ====================*/
void	CChatManager::HandleWhisper(CPacket& pkt)
{
	const wstring sSenderName(pkt.ReadWString());
	const wstring sMessage(pkt.ReadWString());

	if (IsIgnored(sSenderName) || (GetFriendlyChat() && !IsCompanion(sSenderName)))
		return;

	m_lLastWhispers.remove(sSenderName);
	m_lLastWhispers.push_front(sSenderName);

	if (cc_playChatSounds)
		PlaySound(_T("RecievedWhisper"));

	AddIRCChatMessage(CHAT_MESSAGE_WHISPER, Translate(_T("chat_whisper"), _T("sender"), sSenderName, _T("message"), sMessage), TSNULL, true);
}


/*====================
  CChatManager::HandleWhisperBuddies
  ====================*/
void	CChatManager::HandleWhisperBuddies(CPacket& pkt)
{
	const wstring sSenderName(pkt.ReadWString());
	const wstring sMessage(pkt.ReadWString());

	if (IsIgnored(sSenderName))
		return;

	if (cc_playChatSounds)
		PlaySound(_T("RecievedWhisper"));
	AddIRCChatMessage(CHAT_MESSAGE_WHISPER_BUDDIES, Translate(_T("chat_whisper_to_buddies"), _T("sender"), sSenderName, _T("message"), sMessage), TSNULL, true);
}


/*====================
  CChatManager::HandleWhisperFailed
  ====================*/
void	CChatManager::HandleWhisperFailed(CPacket& pkt)
{
	wstring sSenderName(pkt.ReadWString());
	wstring sMessage(pkt.ReadWString());

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_user_offline")));
}


/*====================
  CChatManager::HandleIM
  ====================*/
void	CChatManager::HandleIM(CPacket& pkt)
{
	byte ySendClientInfo(pkt.ReadByte());
	wstring sSenderName;

	// If the chat server sends 1 or 2 here, read the new information from the packet properly
	if (ySendClientInfo >= 1)
	{
		sSenderName = RemoveClanTag(pkt.ReadWString());
		uint uiSenderAccountID(pkt.ReadInt());
		byte yStatus(pkt.ReadByte());
		byte yFlags(pkt.ReadByte());

		uint uiChatNameColor(Host.LookupChatNameColor(pkt.ReadTString()));
		uint uiAccountIcon(0);
		int iAccountIconSlot(0);		

		GetAccountIconInfo(pkt.ReadTString(), uiAccountIcon, iAccountIconSlot);

		uint uiAscensionLevel(pkt.ReadInt());

		// The first time we send an IM both the sender and the recipient get the chat client info sent to them
		ChatClientMap_it it(m_mapUserList.find(uiSenderAccountID));
		if (it == m_mapUserList.end())
		{
			it = m_mapUserList.insert(ChatClientPair(uiSenderAccountID, SChatClient())).first;
			m_mapNameToAccountID.insert(pair<wstring, uint>(LowerString(StripClanTag(sSenderName)), uiSenderAccountID));
		}

		it->second.sName = sSenderName;
		it->second.uiAccountID = uiSenderAccountID;
		it->second.yStatus = yStatus;
		it->second.yFlags = yFlags;
		it->second.uiChatNameColor = uiChatNameColor;
		it->second.uiAccountIcon = uiAccountIcon;
		it->second.iAccountIconSlot = iAccountIconSlot;
		it->second.uiAscensionLevel = uiAscensionLevel;
	}
	else
	{
		sSenderName = RemoveClanTag(pkt.ReadWString());
	}

	wstring sMessage(pkt.ReadWString());

	if (IsIgnored(sSenderName))
		return;

	if (cg_censorChat)
		CensorChat(sMessage);

	m_cDate = CDate(true);
	static tsvector vParams(3);

	// Handle the IM the same way we always have
	if (ySendClientInfo == 0 || ySendClientInfo == 1)
	{
		tstring sFinal(_T("^770[") + m_cDate.GetTimeString(TIME_NO_SECONDS) + _T("] ") + Translate(_T("chat_im"), _T("sender"), sSenderName, _T("message"), sMessage));

		m_mapIMs[LowerString(sSenderName)].push_back(sFinal);

		if (cc_showIMNotification)
			PushNotification(NOTIFY_TYPE_IM, sSenderName);

		vParams[0] = sSenderName;
		vParams[1] = sFinal;
		vParams[2] = _T("1");
		ChatWhisperUpdate.Trigger(vParams);
	}
	else if (ySendClientInfo == 2)
	{
		// We actualy sent this IM, and the chat server has now responded back to both the sender and the recipient so populate the IM accordingly
		tstring sFinal(_T("^770[") + m_cDate.GetTimeString(TIME_NO_SECONDS) + _T("] ") + Translate(_T("chat_im_sent"), _T("name"), RemoveClanTag(m_mapUserList[m_uiAccountID].sName), _T("message"), sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH)));

		// Note sSenderName is actually the name of the recipient in this case
		m_mapIMs[LowerString(sSenderName)].push_back(sFinal);

		vParams[0] = sSenderName;
		vParams[1] = sFinal;
		vParams[2] = _T("0");
		ChatWhisperUpdate.Trigger(vParams);
		
		PlaySound(_T("SentIM"));
		
		ChatSentIMCount.Trigger(XtoA(AddSentIM()));
		ChatOpenIMCount.Trigger(XtoA(GetOpenIMCount()));
	}
}


/*====================
  CChatManager::HandleIMFailed
  ====================*/
void	CChatManager::HandleIMFailed(CPacket& pkt)
{
	wstring sTarget(RemoveClanTag(pkt.ReadWString()));
	wstring sFinal(Translate(L"chat_im_failed", L"target", sTarget));

	m_mapIMs[LowerString(sTarget)].push_back(sFinal);

	static tsvector vParams(3);
	vParams[0] = sTarget;
	vParams[1] = sFinal;
	vParams[2] = _T("0");
	ChatWhisperUpdate.Trigger(vParams);
}


/*====================
  CChatManager::HandleClanWhisperFailed
  ====================*/
void	CChatManager::HandleClanWhisperFailed()
{
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_whisper_failed")));
}


/*====================
  CChatManager::HandleInitialStatusUpdate
  ====================*/
void	CChatManager::HandleInitialStatusUpdate(CPacket& pkt)
{
	uint uiNumUpdated(pkt.ReadInt());
	ChatClientMap_it it;

	for (uint i(0); i < uiNumUpdated; ++i)
	{
		if (pkt.HasFaults())
			break;

		uint uiAccountID(pkt.ReadInt());
		byte yStatus(pkt.ReadByte());
		byte yFlags(pkt.ReadByte());

		uint uiChatNameColor(Host.LookupChatNameColor(pkt.ReadTString()));
		uint uiAccountIcon(0);
		int iAccountIconSlot(0);
		GetAccountIconInfo(pkt.ReadTString(), uiAccountIcon, iAccountIconSlot);

		wstring sServerAddressPort;
		wstring sGameName;
		uint uiMatchID(-1);

		if (yStatus > CHAT_CLIENT_STATUS_CONNECTED)
		{
			sServerAddressPort = pkt.ReadWString();

			if (yStatus == CHAT_CLIENT_STATUS_IN_GAME)
			{
				sGameName = pkt.ReadWString();
				uiMatchID = pkt.ReadInt();
			}
		}

		uint uiAscensionLevel(pkt.ReadInt());

		it = m_mapUserList.find(uiAccountID);
		if (it != m_mapUserList.end())
		{
			it->second.yStatus = yStatus;
			it->second.yFlags = yFlags;

			it->second.uiChatNameColor = uiChatNameColor;
			it->second.uiAccountIcon = uiAccountIcon;
			it->second.iAccountIconSlot = iAccountIconSlot;
			it->second.uiMatchID = uiMatchID;

			it->second.sServerAddressPort = sServerAddressPort;
			it->second.sGameName = sGameName;
			it->second.uiAscensionLevel = uiAscensionLevel;
		}

		if (IsBuddy(uiAccountID))
			RefreshBuddyList();

		if (IsClanMember(uiAccountID))
			RefreshClanList();
	}
}


/*====================
  CChatManager::HandleStatusUpdate
  ====================*/
void	CChatManager::HandleStatusUpdate(CPacket& pkt)
{
	uint uiAccountID(pkt.ReadInt());
	byte yStatus(pkt.ReadByte());
	byte yFlags(pkt.ReadByte());
	int iClanID(pkt.ReadInt());
	wstring sClan(pkt.ReadWString());
	uint uiChatSymbol(Host.LookupChatSymbol(pkt.ReadTString()));
	uint uiChatNameColor(Host.LookupChatNameColor(pkt.ReadTString()));
	uint uiAccountIcon(0);
	int iAccountIconSlot(0);

	GetAccountIconInfo(pkt.ReadTString(), uiAccountIcon, iAccountIconSlot);
		
	wstring sServerAddressPort;
	wstring sGameName;
	uint uiMatchID(-1);
			
	byte	yArrangedType(0);
	wstring sPlayerName;
	wstring sRegion;
	wstring sGameModeName;
	byte	yTeamSize(0);
	wstring	sMapName;
	byte	yTier(0);
	byte	yNoStats(0);
	byte	yNoLeavers(0);
	byte	yPrivate(0);
	byte	yAllHeroes(0);
	byte	yCasualMode(0);
	byte	yForceRandom(0);
	byte	yAutoBalanced(0);
	byte	yAdvancedOptions(0);	
	ushort	unMinPSR(0);
	ushort	unMaxPSR(0);
	byte	yDevHeroes(0);
	byte	yHardcore(0);
	byte	yVerifiedOnly(0);
	byte	yGated(0);
		
	if (yStatus > CHAT_CLIENT_STATUS_CONNECTED)
	{
		sServerAddressPort = pkt.ReadWString();
	}
	
	if (yStatus == CHAT_CLIENT_STATUS_IN_GAME)
	{
		sGameName = pkt.ReadWString();
		uiMatchID = pkt.ReadInt();

		byte yExtendedInfo(pkt.ReadByte());

		if (yExtendedInfo != 0)
		{
			// new stuff the chat server will send related to game info
			yArrangedType = pkt.ReadByte();
			sPlayerName = pkt.ReadWString();
			sRegion = pkt.ReadWString();
			sGameModeName = pkt.ReadWString();
			yTeamSize = pkt.ReadByte();
			sMapName = pkt.ReadWString();
			yTier = pkt.ReadByte();
			yNoStats = pkt.ReadByte();
			yNoLeavers = pkt.ReadByte();
			yPrivate = pkt.ReadByte();
			yAllHeroes = pkt.ReadByte();
			yCasualMode = pkt.ReadByte();
			yForceRandom = pkt.ReadByte();
			yAutoBalanced = pkt.ReadByte();
			yAdvancedOptions = pkt.ReadByte();
			unMinPSR = pkt.ReadShort();
			unMaxPSR = pkt.ReadShort();
			yDevHeroes = pkt.ReadByte();
			yHardcore = pkt.ReadByte();
			yVerifiedOnly = pkt.ReadByte();
			yGated = pkt.ReadByte();
		}
	}

	uint uiAscensionLevel(pkt.ReadInt());

	if (pkt.HasFaults())
		return;

	ChatClientMap_it it(m_mapUserList.find(uiAccountID));
	if (it == m_mapUserList.end())
		return;

	bool bAddedNotification(false);
	static tsvector vParams(22);

	if (m_bFollow && m_sFollowName == RemoveClanTag(it->second.sName) && yStatus != CHAT_CLIENT_STATUS_IN_GAME &&
		CompareNoCase(sServerAddressPort.substr(0, 9), LOCALHOST) != 0 &&
		yArrangedType != 1)
		UpdateFollow(sServerAddressPort);

	it->second.uiMatchID = uiMatchID;

	if (GetAccountID() != uiAccountID && IsBuddy(uiAccountID))
	{
#if !defined(NEWUICODE)
		RefreshBuddyList();
#else
		static tsvector vParameters(3);
		vParameters[0] = it->second.sName;//name
		vParameters[1] = XtoA(yStatus);//new status
		vParameters[2] = XtoA(it->second.yStatus);//old status
		ChatBuddyStatusChanged.Trigger(vParameters);
#endif

		if (yStatus == CHAT_CLIENT_STATUS_CONNECTED && it->second.yStatus == CHAT_CLIENT_STATUS_DISCONNECTED && cc_showBuddyConnectionNotification)
		{
			PushNotification(NOTIFY_TYPE_BUDDY_ONLINE, it->second.sName);
		}
		else if (yStatus < CHAT_CLIENT_STATUS_IN_GAME && it->second.yStatus == CHAT_CLIENT_STATUS_IN_GAME && cc_showBuddyLeaveGameNotification)
		{
			PushNotification(NOTIFY_TYPE_BUDDY_LEFT_GAME, it->second.sName, it->second.sGameName);
		}
		else if (yStatus == CHAT_CLIENT_STATUS_DISCONNECTED && it->second.yStatus > CHAT_CLIENT_STATUS_DISCONNECTED && cc_showBuddyDisconnectionNotification)
		{
			PushNotification(NOTIFY_TYPE_BUDDY_OFFLINE, it->second.sName);
		}
		else if (yStatus == CHAT_CLIENT_STATUS_IN_GAME && it->second.yStatus < CHAT_CLIENT_STATUS_IN_GAME && cc_showBuddyJoinGameNotification)
		{
			if (CompareNoCase(sServerAddressPort.substr(0, 9), LOCALHOST) != 0)
			{
				vParams[0] = sServerAddressPort;		// Address
				vParams[1] = sGameName;					// Game Name
				vParams[2] = it->second.sName;			// Buddy Name
				vParams[3] = sRegion;					// Server Region
				vParams[4] = sGameModeName;				// Game Mode Name (banningdraft)
				vParams[5] = XtoA(yTeamSize);			// Team Size			
				vParams[6] = sMapName;					// Map Name (caldavar)
				vParams[7] = XtoA(yTier);				// Tier - Noobs Only (0), Noobs Allowed (1), Pro (2) (Deprecated)
				vParams[8] = XtoA(yNoStats);			// 0 - Unofficial, 1 - Official w/ stats, 2 - Official w/o stats
				vParams[9] = XtoA(yNoLeavers);			// No Leavers (1), Leavers (0)
				vParams[10] = XtoA(yPrivate);			// Private (1), Not Private (0)									
				vParams[11] = XtoA(yAllHeroes);			// All Heroes (1), Not All Heroes (0)
				vParams[12] = XtoA(yCasualMode);		// Casual Mode (1), Not Casual Mode (0)
				vParams[13] = XtoA(yForceRandom);		// Force Random (1), Not Force Random (0) -- (NOTE: Deprecated)
				vParams[14] = XtoA(yAutoBalanced);		// Auto Balanced (1), Non Auto Balanced (0)
				vParams[15] = XtoA(yAdvancedOptions);	// Advanced Options	(1), No Advanced Options (0)
				vParams[16] = XtoA(unMinPSR);			// Min PSR
				vParams[17] = XtoA(unMaxPSR);			// Max PSR
				vParams[18] = XtoA(yDevHeroes);			// Dev Heroes (1), Non Dev Heroes (0)
				vParams[19] = XtoA(yHardcore);			// Hardcore (1), Non Hardcore (0)
				vParams[20] = XtoA(yVerifiedOnly);		// VerifiedOnly (1), Everyone (0)
				vParams[21] = XtoA(yGated);				// Gated (1), Not Gated (0)
			
				PushNotification(NOTIFY_TYPE_BUDDY_JOIN_GAME, XtoA(yArrangedType), TSNULL, TSNULL, vParams);
			}
		}

		bAddedNotification = true;
	}
	
	if (GetAccountID() != uiAccountID && IsClanMember(uiAccountID))
	{
//#if !defined(NEWUICODE)
		RefreshClanList();
// #else
// 		static tsvector vParams(3);
// 		vParams[0] = it->second.sName;//name
// 		vParams[1] = XtoA(yStatus);//new status
// 		vParams[2] = XtoA(it->second.yStatus);//old status
// 		ChatClanMemberStatusChanged.Trigger(vParams);
// #endif

		if (!bAddedNotification)
		{
			if (yStatus == CHAT_CLIENT_STATUS_CONNECTED && it->second.yStatus == CHAT_CLIENT_STATUS_DISCONNECTED && cc_showClanConnectionNotification)
			{
				PushNotification(NOTIFY_TYPE_CLAN_ONLINE, it->second.sName);
			}
			else if (yStatus < CHAT_CLIENT_STATUS_IN_GAME && it->second.yStatus == CHAT_CLIENT_STATUS_IN_GAME && cc_showClanLeaveGameNotification)
			{
				PushNotification(NOTIFY_TYPE_CLAN_LEFT_GAME, it->second.sName, it->second.sGameName);
			}
			else if (yStatus == CHAT_CLIENT_STATUS_DISCONNECTED && it->second.yStatus > CHAT_CLIENT_STATUS_DISCONNECTED && cc_showClanDisconnectionNotification)
			{
				PushNotification(NOTIFY_TYPE_CLAN_OFFLINE, it->second.sName);
			}
			else if (yStatus == CHAT_CLIENT_STATUS_IN_GAME && it->second.yStatus < CHAT_CLIENT_STATUS_IN_GAME && cc_showClanJoinGameNotification)
			{
				if (CompareNoCase(sServerAddressPort.substr(0, 9), LOCALHOST) != 0)
				{
					vParams[0] = sServerAddressPort;				// Address
					vParams[1] = sGameName;							// Game Name
					vParams[2] = RemoveClanTag(it->second.sName);	// Clan Member Name
					vParams[3] = sRegion;							// Server Region
					vParams[4] = sGameModeName;						// Game Mode Name (banningdraft)
					vParams[5] = XtoA(yTeamSize);					// Team Size			
					vParams[6] = sMapName;							// Map Name (caldavar)
					vParams[7] = XtoA(yTier);						// Tier - Noobs Only (0), Noobs Allowed (1), Pro (2) (Deprecated)
					vParams[8] = XtoA(yNoStats);					// 0 - Unofficial, 1 - Official w/ stats, 2 - Official w/o stats
					vParams[9] = XtoA(yNoLeavers);					// No Leavers (1), Leavers (0)
					vParams[10] = XtoA(yPrivate);					// Private (1), Not Private (0)									
					vParams[11] = XtoA(yAllHeroes);					// All Heroes (1), Not All Heroes (0)
					vParams[12] = XtoA(yCasualMode);				// Casual Mode (1), Not Casual Mode (0)
					vParams[13] = XtoA(yForceRandom);				// Force Random (1), Not Force Random (0) -- (NOTE: Deprecated)
					vParams[14] = XtoA(yAutoBalanced);				// Auto Balanced (1), Non Auto Balanced (0)
					vParams[15] = XtoA(yAdvancedOptions);			// Advanced Options	(1), No Advanced Options (0)
					vParams[16] = XtoA(unMinPSR);					// Min PSR
					vParams[17] = XtoA(unMaxPSR);					// Max PSR					
					vParams[18] = XtoA(yDevHeroes);					// Dev Heroes (1), Non Dev Heroes (0)
					vParams[19] = XtoA(yHardcore);					// Hardcore (1), Non Hardcore (0)
					vParams[20] = XtoA(yVerifiedOnly);				// VerifiedOnly (1), Everyone (0)
					vParams[21] = XtoA(yGated);						// Gated (1), Not Gated (0)
				
					PushNotification(NOTIFY_TYPE_CLAN_JOIN_GAME, XtoA(yArrangedType), TSNULL, TSNULL, vParams);
				}
			}
		}
	}
	
	// save a notification that this player joined a game, in case they disconnect and want to rejoin
	if (GetAccountID() == uiAccountID && yStatus == CHAT_CLIENT_STATUS_IN_GAME)
	{
		if (CompareNoCase(sServerAddressPort.substr(0, 9), LOCALHOST) != 0)
		{
			vParams[0] = sServerAddressPort;		// Address
			vParams[1] = sGameName;					// Game Name
			vParams[2] = it->second.sName;			// Self
			vParams[3] = sRegion;					// Server Region
			vParams[4] = sGameModeName;				// Game Mode Name (banningdraft)
			vParams[5] = XtoA(yTeamSize);			// Team Size			
			vParams[6] = sMapName;					// Map Name (caldavar)
			vParams[7] = XtoA(yTier);				// Tier - Noobs Only (0), Noobs Allowed (1), Pro (2) (Deprecated)
			vParams[8] = XtoA(yNoStats);			// 0 - Unofficial, 1 - Official w/ stats, 2 - Official w/o stats
			vParams[9] = XtoA(yNoLeavers);			// No Leavers (1), Leavers (0)
			vParams[10] = XtoA(yPrivate);			// Private (1), Not Private (0)									
			vParams[11] = XtoA(yAllHeroes);			// All Heroes (1), Not All Heroes (0)
			vParams[12] = XtoA(yCasualMode);		// Casual Mode (1), Not Casual Mode (0)
			vParams[13] = XtoA(yForceRandom);		// Force Random (1), Not Force Random (0) -- (NOTE: Deprecated)
			vParams[14] = XtoA(yAutoBalanced);		// Auto Balanced (1), Non Auto Balanced (0)
			vParams[15] = XtoA(yAdvancedOptions);	// Advanced Options	(1), No Advanced Options (0)
			vParams[16] = XtoA(unMinPSR);			// Min PSR
			vParams[17] = XtoA(unMaxPSR);			// Max PSR
			vParams[18] = XtoA(yDevHeroes);			// Dev Heroes (1), Non Dev Heroes (0)
			vParams[19] = XtoA(yHardcore);			// Hardcore (1), Non Hardcore (0)
			vParams[20] = XtoA(yVerifiedOnly);		// VerifiedOnly (1), Everyone (0)
			vParams[21] = XtoA(yGated);				// Gated (1), Not Gated (0)
		
			PushNotification(NOTIFY_TYPE_SELF_JOIN_GAME, TSNULL, TSNULL, TSNULL, vParams);
		}
	}

	if (GetAccountID() != uiAccountID && m_mapIMs.find(LowerString(RemoveClanTag(it->second.sName))) != m_mapIMs.end())
	{
		m_cDate = CDate(true);
		tstring sFinal;
		
		if (it->second.yStatus > CHAT_CLIENT_STATUS_DISCONNECTED && yStatus == CHAT_CLIENT_STATUS_DISCONNECTED)
			sFinal = _T("^770[") + m_cDate.GetTimeString(TIME_NO_SECONDS) + _T("] ") + Translate(_T("chat_im_user_offline"), _T("name"), RemoveClanTag(it->second.sName));
		else if (it->second.yStatus == CHAT_CLIENT_STATUS_DISCONNECTED && yStatus > CHAT_CLIENT_STATUS_DISCONNECTED)
			sFinal = _T("^770[") + m_cDate.GetTimeString(TIME_NO_SECONDS) + _T("] ") + Translate(_T("chat_im_user_online"), _T("name"), RemoveClanTag(it->second.sName));
			
		if (!sFinal.empty())
		{
			m_mapIMs[LowerString(RemoveClanTag(it->second.sName))].push_back(sFinal);

			static tsvector vMiniParams(3);
			vMiniParams[0] = RemoveClanTag(it->second.sName);
			vMiniParams[1] = sFinal;
			vMiniParams[2] = _T("0");
			ChatWhisperUpdate.Trigger(vMiniParams);
		}
	}

	it->second.sServerAddressPort = sServerAddressPort;
	it->second.sGameName = sGameName;
	it->second.iClanID = iClanID;
	it->second.sClan = sClan;

	UpdateClientChannelStatus(TSNULL, it->second.sName, uiAccountID, yStatus, yFlags, uiChatSymbol, uiChatNameColor, uiAccountIcon, iAccountIconSlot, uiAscensionLevel);
}


/*====================
  CChatManager::HandleClanWhisper
  ====================*/
void	CChatManager::HandleClanWhisper(CPacket& pkt)
{
	const uint uiAccountID(pkt.ReadInt());
	const wstring sMessage(pkt.ReadWString());

	ChatClientMap_it itFind(m_mapUserList.find(uiAccountID));

	if (itFind == m_mapUserList.end())
		return;

	if (IsIgnored(itFind->first))
		return;

	AddIRCChatMessage(CHAT_MESSAGE_CLAN, Translate(_T("chat_clan_whisper"), _T("name"), itFind->second.sName, _T("message"), sMessage));

	const tstring sIM(Translate(_T("chat_clan_im"), _T("name"), itFind->second.sName, _T("message"), sMessage));

	m_vClanWhispers.push_back(sIM);
	ChatClanWhisperUpdate.Trigger(sIM);
	PlaySound(_T("RecievedClanMessage"));
}


/*====================
  CChatManager::HandleFlooding
  ====================*/
void	CChatManager::HandleFlooding()
{
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_flooding")));
}


/*====================
  CChatManager::HandleMaxChannels
  ====================*/
void	CChatManager::HandleMaxChannels()
{
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_max_channels")));
}


/*====================
  CChatManager::HandleChannelInfo
  ====================*/
void	CChatManager::HandleChannelInfo(CPacket& pkt)
{
	uint uiID(pkt.ReadInt());
	wstring sName(pkt.ReadWString());
	ushort unUsers(pkt.ReadShort());
	if (pkt.HasFaults())
		return;
	
	SChatChannel& channel(m_mapChannels[uiID]);
	channel.sChannelName = sName;
	channel.uiUserCount = unUsers;

	ChatChannelList.Execute(L"Data('" + XtoW(uiID) + L"','" + sName + L"','" + XtoW(unUsers) + L"');");
	ChatChannelList.Execute(L"SortByCol(0);");
}


/*====================
  CChatManager::HandleChannelInfoSub
  ====================*/
void	CChatManager::HandleChannelInfoSub(CPacket& pkt)
{
	byte ySequence(pkt.ReadByte());
	uint uiID(pkt.ReadInt());
	wstring sName(pkt.ReadWString());
	ushort unUsers(pkt.ReadShort());
	if (pkt.HasFaults())
		return;

	if (ySequence != m_yProcessingListSequence)
		return;

	SChatChannelInfo& channel(m_mapChannelList[uiID]);

	channel.sName = sName;
	channel.sLowerName = LowerString(sName);
	channel.uiUserCount = unUsers;
	
	//SChatChannel& channel(m_mapChannels[uiID]);
	//channel.sChannelName = sName;
	//channel.uiUserCount = unUsers;

	//ChatChannelList.Execute(L"Data('" + XtoW(uiID) + L"','" + sName + L"','" + XtoW(unUsers) + L"');");
	//ChatChannelList.Execute(L"SortByCol(0);");

	ChatAutoCompleteAdd.Trigger(sName);

	//Console << sName << _T(" ") << uiID << _T(" ") << unUsers << newl;
}


/*====================
  CChatManager::HandleUserStatus
  ====================*/
void	CChatManager::HandleUserStatus(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());
	byte yStatus(pkt.ReadByte());
	if (pkt.HasFaults())
		return;

	tsvector vParams(2);
	vParams[0] = RemoveClanTag(sName);
	vParams[1] = XtoA(yStatus);

	ChatUserStatus.Trigger(vParams);
}


/*====================
  CChatManager::HandleServerInvite
  ====================*/
void	CChatManager::HandleServerInvite(CPacket& pkt)
{
	wstring sInviterName(pkt.ReadWString());
	uint uiInviterAccountID(pkt.ReadInt());
	wstring sAddressPort;

	byte yStatus(pkt.ReadByte());
	byte yFlags(pkt.ReadByte());

	uint uiChatNameColor(Host.LookupChatNameColor(pkt.ReadTString()));
	uint uiAccountIcon(0);
	int iAccountIconSlot(0);		

	GetAccountIconInfo(pkt.ReadTString(), uiAccountIcon, iAccountIconSlot);

	ChatClientMap_it it(m_mapUserList.find(uiInviterAccountID));
	if (it == m_mapUserList.end())
	{
		it = m_mapUserList.insert(ChatClientPair(uiInviterAccountID, SChatClient())).first;
		m_mapNameToAccountID.insert(pair<wstring, uint>(LowerString(StripClanTag(sInviterName)), uiInviterAccountID));
	}

	it->second.sName = sInviterName;
	it->second.uiAccountID = uiInviterAccountID;
	it->second.yStatus = yStatus;
	it->second.yFlags = yFlags;
	it->second.uiChatNameColor = uiChatNameColor;
	it->second.uiAccountIcon = uiAccountIcon;
	it->second.iAccountIconSlot = iAccountIconSlot;

	if (yStatus > CHAT_CLIENT_STATUS_CONNECTED)
	{
		it->second.sServerAddressPort = sAddressPort = pkt.ReadWString();

		if (yStatus == CHAT_CLIENT_STATUS_IN_GAME)
		{
			it->second.sGameName = pkt.ReadWString();
			it->second.uiMatchID = pkt.ReadInt();
		}
	}

	it->second.uiAscensionLevel = pkt.ReadInt();

	if (IsIgnored(uiInviterAccountID) || !cc_showGameInvites || (GetFriendlyChat() && !IsCompanion(uiInviterAccountID)))
	{
		RejectServerInvite(uiInviterAccountID);
		return;
	}

	CHostClient* pClient(Host.GetActiveClient());
	if (pClient != NULL)
	{
		if (pClient->ServerInvite(sInviterName, uiInviterAccountID, sAddressPort))
		{
			m_mapUserList[uiInviterAccountID].sServerAddressPort = sAddressPort;
		}
	}
}


/*====================
  CChatManager::HandleInviteFailedUserNotFound
  ====================*/
void	CChatManager::HandleInviteFailedUserNotFound()
{
}


/*====================
  CChatManager::HandleInviteFailedNotInGame
  ====================*/
void	CChatManager::HandleInviteFailedNotInGame()
{
}


/*====================
  CChatManager::HandleInviteRejected
  ====================*/
void	CChatManager::HandleInviteRejected(CPacket& pkt)
{
	pkt.ReadWString();	// wstring sName - Name of the rejecting client
	pkt.ReadInt();		// int iAccountID - ID of the rejecting client
	
	if (pkt.HasFaults())
		return;
}


/*====================
  CChatManager::HandleUserInfoNoExist
  ====================*/
void	CChatManager::HandleUserInfoNoExist(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());

	if (pkt.HasFaults())
		return;

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_info_not_found", L"name", sName));
}


/*====================
  CChatManager::HandleUserInfoOffline
  ====================*/
void	CChatManager::HandleUserInfoOffline(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());
	wstring sLastOnline(pkt.ReadWString());

	if (pkt.HasFaults())
		return;

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_info_offline", L"name", sName, L"seen", sLastOnline));
}


/*====================
  CChatManager::HandleUserInfoInGame
  ====================*/
void	CChatManager::HandleUserInfoInGame(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());
	wstring sGameName(pkt.ReadWString());
	wstring sCGT(pkt.ReadWString());
	
	if (pkt.HasFaults())
		return;	

	if (sGameName.empty())
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_info_joining_game", L"name", sName));
	else if (sCGT.empty())
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_info_in_game", L"name", sName, L"game", sGameName));
	else
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_info_in_game_time", L"name", sName, L"game", sGameName, L"cgt", sCGT));
}


/*====================
  CChatManager::HandleUserInfoOnline
  ====================*/
void	CChatManager::HandleUserInfoOnline(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());
	uint uiNumChannels(pkt.ReadInt());
	
	if (uiNumChannels == 0)
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_info_online_no_channels", L"name", sName));
	else if (uiNumChannels == 1)
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_info_online_one_channel", L"name", sName, L"channel", pkt.ReadWString()));
	else
	{
		wstring sChannels;

		for (uint i(0); i < uiNumChannels; ++i)
		{
			if (pkt.HasFaults())
				break;

			if (i == 0)
				sChannels = pkt.ReadWString();
			else
				sChannels += L", " + pkt.ReadWString();
		}

		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_info_online_multi_channels", L"name", sName, L"channels", sChannels));
	}
}


/*====================
  CChatManager::HandleChannelUpdate
  ====================*/
void	CChatManager::HandleChannelUpdate(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	wstring sName(pkt.ReadWString());
	byte yFlags(pkt.ReadByte());
	wstring sTopic(pkt.ReadWString());

	if (chat_debugInterface)
		Console.UI << _T("HandleChannelUpdate - ") << uiChannelID << _T(" ") << QuoteStr(sName) << newl;

	m_mapChannels[uiChannelID].sChannelName = sName;
	m_mapChannels[uiChannelID].uiFlags = yFlags;
	m_mapChannels[uiChannelID].sTopic = sTopic;

	uint uiNumAdmins(pkt.ReadInt());

	m_mapChannels[uiChannelID].mapAdmins.clear();

	for (uint ui(0); ui < uiNumAdmins; ui++)
	{
		if (pkt.HasFaults())
			break;

		uint uiID(pkt.ReadInt());
		byte yLevel(pkt.ReadByte());

		m_mapChannels[uiChannelID].mapAdmins.insert(ChatAdminPair(uiID, yLevel));
	}

	UpdateChannel(uiChannelID);
}


/*====================
  CChatManager::HandleChannelTopic
  ====================*/
void	CChatManager::HandleChannelTopic(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	wstring sTopic(pkt.ReadWString());

	m_mapChannels[uiChannelID].sTopic = sTopic;

	wsvector vsTopic(2);
	vsTopic[0] = m_mapChannels[uiChannelID].sChannelName;
	vsTopic[1] = m_mapChannels[uiChannelID].sTopic;

	ChatChanTopic.Trigger(vsTopic);

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_topic_change"), _T("topic"), sTopic), m_mapChannels[uiChannelID].sChannelName);
}


/*====================
  CChatManager::HandleChannelKick
  ====================*/
void	CChatManager::HandleChannelKick(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	uint uiKickerID(pkt.ReadInt());
	uint uiKickeeID(pkt.ReadInt());

	ChatClientMap_it itKicker(m_mapUserList.find(uiKickerID));
	if (itKicker == m_mapUserList.end())
		return;

	ChatClientMap_it itKickee(m_mapUserList.find(uiKickeeID));
	if (itKickee == m_mapUserList.end())
		return;

	if (uiKickeeID != m_uiAccountID)
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_user_kicked"), _T("kicker"), itKicker->second.sName, _T("kickee"), itKickee->second.sName), GetChannelName(uiChannelID));
	else
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_kicked"), _T("kicker"), itKicker->second.sName, _T("channel"), GetChannelName(uiChannelID)));
}


/*====================
  CChatManager::HandleChannelBan
  ====================*/
void	CChatManager::HandleChannelBan(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	uint uiBanningID(pkt.ReadInt());
	wstring sBannedName(pkt.ReadWString());

	ChatClientMap_it it(m_mapUserList.find(uiBanningID));

	wstring sBanner;
	if (it != m_mapUserList.end())
		sBanner = it->second.sName;

	if (!sBanner.empty() && IsIgnored(sBanner))
		return;

	if (!CompareNames(sBannedName, m_mapUserList[m_uiAccountID].sName))
	{
		if (sBanner.empty())
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_user_banned_no_name"), _T("banned"), sBannedName), GetChannelName(uiChannelID));
		else
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_user_banned"), _T("banner"), sBanner, _T("banned"), sBannedName), GetChannelName(uiChannelID));
	}
	else 
	{
		if (sBanner.empty())
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_channel_banned_no_name"), _T("channel"), GetChannelName(uiChannelID)));
		else
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_channel_banned"), _T("banner"), sBanner, _T("channel"), GetChannelName(uiChannelID)));
	}
}


/*====================
  CChatManager::HandleChannelUnban
  ====================*/
void	CChatManager::HandleChannelUnban(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	uint uiUnbanningID(pkt.ReadInt());
	wstring sBannedName(pkt.ReadWString());

	ChatClientMap_it it(m_mapUserList.find(uiUnbanningID));

	wstring sBanner;
	if (it != m_mapUserList.end())
		sBanner = it->second.sName;

	if (!sBanner.empty() && IsIgnored(sBanner))
		return;

	if (!CompareNames(sBannedName, m_mapUserList[m_uiAccountID].sName))
	{
		if (sBanner.empty())
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_user_unbanned_no_name"), _T("unbanned"), sBannedName), GetChannelName(uiChannelID));
		else
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_user_unbanned"), _T("unbanner"), sBanner, _T("unbanned"), sBannedName), GetChannelName(uiChannelID));
	}
	else
	{
		if (sBanner.empty())
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_unbanned_no_name", L"channel", GetChannelName(uiChannelID)));
		else
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_unbanned", L"unbanner", sBanner, L"channel", GetChannelName(uiChannelID)));
	}
}


/*====================
  CChatManager::HandleBannedFromChannel
  ====================*/
void	CChatManager::HandleBannedFromChannel(CPacket& pkt)
{
	wstring sChannelName(pkt.ReadWString());
	
	for (sset_it it(m_setAutoJoinChannels.begin()), itEnd(m_setAutoJoinChannels.end()); it != itEnd; ++it)
	{
		// Send a request to remove the channel from their autojoin channel list if they are banned from it.
		// If they are banned from a channel then they aren't able to remove the channel from their list because they aren't 
		// able to actually join the channel, and thus they never see the "Auto Connect" checkbox for the channel.
		if (CompareNoCase(sChannelName, *it) == 0)
		{
			RemoveChannel(sChannelName);
			break;	
		}
	}		
	
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_banned_from_channel", L"channel", sChannelName));
}


/*====================
  CChatManager::HandleChannelSilenced
  ====================*/
void	CChatManager::HandleChannelSilenced(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_silenced")), GetChannelName(uiChannelID));
}


/*====================
  CChatManager::HandleChannelSilenceLifted
  ====================*/
void	CChatManager::HandleChannelSilenceLifted(CPacket& pkt)
{
	wstring sChannelName(pkt.ReadWString());
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_silence_lifted", L"channel", sChannelName));
}


/*====================
  CChatManager::HandleChannelPromote
  ====================*/
void	CChatManager::HandleChannelPromote(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	uint uiAccountID(pkt.ReadInt());
	uint uiPromoterID(pkt.ReadInt());

	ChatChannelMap_it channelit(m_mapChannels.find(uiChannelID));

	if (channelit == m_mapChannels.end())
		return;

	ChatClientMap_it userit(m_mapUserList.find(uiAccountID));

	if (userit == m_mapUserList.end())
		return;

	ChatClientMap_it promoterit(m_mapUserList.find(uiPromoterID));

	if (promoterit == m_mapUserList.end())
		return;

	ChatAdminMap_it adminit(channelit->second.mapAdmins.find(uiAccountID));

	if (adminit == channelit->second.mapAdmins.end())
	{
		adminit = channelit->second.mapAdmins.insert(ChatAdminPair(userit->first, CHAT_CLIENT_ADMIN_NONE)).first;
	}

	if (adminit == channelit->second.mapAdmins.end())
		return;

	adminit->second++;

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_promote_success"), _T("name"), userit->second.sName, _T("rank"), Translate(g_sAdminNames[adminit->second]), _T("promoter"), promoterit->second.sName), channelit->second.sChannelName);

	UpdateChannel(uiChannelID);
}


/*====================
  CChatManager::HandleChannelDemote
  ====================*/
void	CChatManager::HandleChannelDemote(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	uint uiAccountID(pkt.ReadInt());
	uint uiDemoterID(pkt.ReadInt());

	ChatChannelMap_it channelit(m_mapChannels.find(uiChannelID));

	if (channelit == m_mapChannels.end())
		return;

	ChatClientMap_it userit(m_mapUserList.find(uiAccountID));

	if (userit == m_mapUserList.end())
		return;

	ChatClientMap_it demoterit(m_mapUserList.find(uiDemoterID));

	if (demoterit == m_mapUserList.end())
		return;

	ChatAdminMap_it adminit(channelit->second.mapAdmins.find(uiAccountID));

	if (adminit == channelit->second.mapAdmins.end())
		return;

	adminit->second--;

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_demote_success"), _T("name"), userit->second.sName, _T("rank"), Translate(g_sAdminNames[adminit->second]), _T("demoter"), demoterit->second.sName), channelit->second.sChannelName);

	UpdateChannel(uiChannelID);
}


/*====================
  CChatManager::HandleSilencePlaced
  ====================*/
void	CChatManager::HandleSilencePlaced(CPacket& pkt)
{
	wstring sChannel(pkt.ReadWString());
	wstring sName(pkt.ReadWString());
	wstring sSilenced(pkt.ReadWString());
	uint uiDuration(pkt.ReadInt());

	uiDuration = uint(MsToMin(uiDuration));
	
	if (IsIgnored(sName))
		return;		

	if (CompareNames(sSilenced, m_mapUserList[m_uiAccountID].sName))
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_silence_placed", L"channel", sChannel, L"name", sName, L"duration", XtoW(uiDuration)));
	else
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_user_silence_placed", L"name", sName, L"silenced", sSilenced, L"duration", XtoW(uiDuration)), sChannel);
}


/*====================
  CChatManager::HandleMessageAll
  ====================*/
void	CChatManager::HandleMessageAll(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());
	wstring sMessage(pkt.ReadWString());

	// Always show date/time when displaying a server message
	AddIRCChatMessage(CHAT_MESSAGE_GLOBAL, Translate(L"chat_message_all", L"name", sName, L"message", sMessage), WSNULL, true);
}


/*====================
  CChatManager::HandleExcessiveGameplayMessage
  ====================*/
void	CChatManager::HandleExcessiveGameplayMessage(CPacket& pkt)
{
	byte yType(pkt.ReadByte());
	m_yAntiAddictionBenefit = pkt.ReadByte();

	if (pkt.HasFaults())
		return;

	switch (yType)
	{
	case EEGPT_1_HOUR:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_excessive_gameplay_1_hour")));
		break;

	case EEGPT_2_HOURS:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_excessive_gameplay_2_hours")));
		break;

	case EEGPT_FATIGUE:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_excessive_gameplay_fatigue")));
		break;

	case EEGPT_UNHEALTHY:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_excessive_gameplay_unhealthy")));
		break;

	case EEGPT_GENERAL:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_excessive_gameplay_message")));
		break;

	case EEGPT_NONE:
	default:
		break;
	}
}


/*====================
  CChatManager::HandleMaintenanceMessage
  ====================*/
void	CChatManager::HandleMaintenanceMessage(CPacket& pkt)
{
	byte yType(pkt.ReadByte());

	if (pkt.HasFaults())
		return;

	switch (yType)
	{
	case 1:
		AddIRCChatMessage(CHAT_MESSAGE_GLOBAL, Translate(L"chat_maintenance"), WSNULL, true);
		break;

	case 2:
		AddIRCChatMessage(CHAT_MESSAGE_GLOBAL, Translate(L"chat_matchmaking_disabled"), WSNULL, true);
		break;

	default:
		break;
	}
}


/*====================
  CChatManager::HandleAuthAccepted
  ====================*/
void	CChatManager::HandleAuthAccepted()
{
	m_uiConnectRetries = 0;
	m_uiNextReconnectTime = INVALID_TIME;

	if (GetChatModeType() == CHAT_MODE_INVISIBLE)
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_connected_invisible")));
	else
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_connected")));

	m_eStatus = CHAT_CLIENT_STATUS_CONNECTED;

	// Update server on our status
	if (Host.IsConnected())
		JoiningGame(Host.GetConnectedAddress());

	if (Host.IsInGame())
	{
		const CHostClient* pHostClient(Host.GetActiveClient());
		if (pHostClient != NULL)
			FinishedJoiningGame(m_sGameName, m_uiMatchID, m_yArrangedType, (pHostClient == NULL || (!pHostClient->IsPlayerSpectator() && !pHostClient->IsHiddenClient())));
	}

	UpdateRecentlyPlayed();

	// always set these on logging in or else a player may see the previous account they logged in as
	// as being online, or in the clan panel
	RefreshBuddyList();
	RefreshClanList();
	RequestRankedPlayInfo();

	// Update server on channels we're in
	for (uiset::iterator it(m_setChannelsIn.begin()); it != m_setChannelsIn.end(); it++)
		JoinChannel(GetChannelName(*it));

	if (GetChatModeType() == CHAT_MODE_INVISIBLE)
	{
		// show the "Status" channel that lets them know they are logged in in invisible mode when logging in instead of no focused channels
		SetFocusedChannel(uint(-1), true);
	}
	else
	{
		// Try to join each of the auto join channels here
		for (sset_it it(m_setAutoJoinChannels.begin()), itEnd(m_setAutoJoinChannels.end()); it != itEnd; ++it)		
		{
			tstring sChannelName(*it);
			JoinChannel(sChannelName);
		}
	}

	SendAction(AC_DAILY_LOGINS);
}


/*====================
  CChatManager::HandleRejected
  ====================*/
void	CChatManager::HandleRejected(CPacket& pkt)
{
	EChatRejectReason eReason(static_cast<EChatRejectReason>(pkt.ReadByte(ECR_UNKNOWN)));

	switch (eReason)
	{
	case ECR_UNKNOWN:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_reject_unknown")));
		break;
	case ECR_BAD_VERSION:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_version_mismatch")));
		break;
	case ECR_AUTH_FAILED:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_reject_auth")));
		break;
	case ECR_ACCOUNT_SHARING:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_reject_account_sharing")));
		break;
	case ECR_ACCOUNT_SHARING_WARNING:
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_reject_account_sharing")));
		return;
	}

	Disconnect();
}


/*====================
  CChatManager::HandleChannelAuthEnabled
  ====================*/
void	CChatManager::HandleChannelAuthEnabled(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_auth_enabled")), GetChannelName(uiChannelID));
}


/*====================
  CChatManager::HandleChannelAuthDisabled
  ====================*/
void	CChatManager::HandleChannelAuthDisabled(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_auth_disabled")), GetChannelName(uiChannelID));
}


/*====================
  CChatManager::HandleChannelAddAuthUser
  ====================*/
void	CChatManager::HandleChannelAddAuthUser(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	wstring sName(pkt.ReadWString());

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_auth_add_success", L"name", sName), GetChannelName(uiChannelID));
}


/*====================
  CChatManager::HandleChannelAddAuthUserFailed
  ====================*/
void	CChatManager::HandleChannelAddAuthUserFailed(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	wstring sName(pkt.ReadWString());

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_auth_add_failure", L"name", sName), GetChannelName(uiChannelID));
}


/*====================
  CChatManager::HandleChannelRemoveAuthUser
  ====================*/
void	CChatManager::HandleChannelRemoveAuthUser(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	wstring sName(pkt.ReadWString());

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_auth_remove_success", L"name", sName), GetChannelName(uiChannelID));
}


/*====================
  CChatManager::HandleChannelRemoveAuthUserFailed
  ====================*/
void	CChatManager::HandleChannelRemoveAuthUserFailed(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	wstring sName(pkt.ReadWString());

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_auth_remove_failure"), _T("name"), sName), GetChannelName(uiChannelID));
}


/*====================
  CChatManager::HandleChannelListAuth
  ====================*/
void	CChatManager::HandleChannelListAuth(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	uint uiNumUsers(pkt.ReadInt());

	if (uiNumUsers == 0)
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_list_none")), GetChannelName(uiChannelID));
	else
	{
		for (uint i(0); i < uiNumUsers; ++i)
		{
			if (pkt.HasFaults())
				break;

			wstring sName(pkt.ReadWString());
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_list_entry"), _T("name"), sName), GetChannelName(uiChannelID));
		}
	}
}


/*====================
  CChatManager::HandleChannelSetPassword
  ====================*/
void	CChatManager::HandleChannelSetPassword(CPacket& pkt)
{
	uint uiChannelID(pkt.ReadInt());
	wstring sName(pkt.ReadWString());

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_password_change"), _T("name"), sName), GetChannelName(uiChannelID));
}


/*====================
  CChatManager::HandleChannelJoinPassword
  ====================*/
void	CChatManager::HandleChannelJoinPassword(CPacket& pkt)
{
	wstring sChannelName(pkt.ReadWString());
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_password_required"), _T("name"), sChannelName));
}


/*====================
  CChatManager::HandleClanInvite
  ====================*/
void	CChatManager::HandleClanInvite(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());
	wstring sClan(pkt.ReadWString());

	if (IsIgnored(sName) || IsInAClan(GetLocalAccountID()) || (GetFriendlyChat() && !IsCompanion(sName)))
	{
		CPacket pktReject;
		pktReject << CHAT_CMD_CLAN_ADD_REJECTED;
		m_sockChat.SendPacket(pktReject);
		return;
	}

	wsvector vMiniParams(2);
	vMiniParams[0] = sName;
	vMiniParams[1] = sClan;

	ChatClanInvite.Trigger(vMiniParams);

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_clan_invite_received", L"name", sName, L"clan", sClan));
}


/*====================
  CChatManager::HandleClanInviteRejected
  ====================*/
void	CChatManager::HandleClanInviteRejected(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_clan_invite_rejected", L"name", sName));
}


/*====================
  CChatManager::HandleClanInviteFailedOnline
  ====================*/
void	CChatManager::HandleClanInviteFailedOnline(CPacket& pkt)
{
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_invite_failed_online")));
}


/*====================
  CChatManager::HandleClanInviteFailedClan
  ====================*/
void	CChatManager::HandleClanInviteFailedClan(CPacket& pkt)
{
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_invite_failed_clan")));
}


/*====================
  CChatManager::HandleClanInviteFailedInvite
  ====================*/
void	CChatManager::HandleClanInviteFailedInvite(CPacket& pkt)
{
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_invite_failed_invite")));
}


/*====================
  CChatManager::HandleClanInviteFailedPermissions
  ====================*/
void	CChatManager::HandleClanInviteFailedPermissions(CPacket& pkt)
{
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_invite_failed_permissions")));
}


/*====================
  CChatManager::HandleClanInviteRejected
  ====================*/
void	CChatManager::HandleClanInviteFailedUnknown(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());

	if (!CompareNames(sName, m_mapUserList[m_uiAccountID].sName))
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_invite_failed_unknown"), _T("name"), sName));
	else
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_invite_failed_unknown_self")));
}


/*====================
  CChatManager::HandleClanCreateFailedClan
  ====================*/
void	CChatManager::HandleClanCreateFailedClan(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_fail_clan"), _T("name"), sName));
	ChatClanCreateFail.Trigger(Translate(_T("chat_clan_create_result_fail_clan"), _T("name"), sName));
}


/*====================
  CChatManager::HandleClanCreateFailedInvite
  ====================*/
void	CChatManager::HandleClanCreateFailedInvite(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_fail_invite"), _T("name"), sName));
	ChatClanCreateFail.Trigger(Translate(_T("chat_clan_create_result_fail_invite"), _T("name"), sName));
}


/*====================
  CChatManager::HandleClanCreateFailedNotFound
  ====================*/
void	CChatManager::HandleClanCreateFailedNotFound(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_clan_create_fail_not_found", L"name", sName));
	ChatClanCreateFail.Trigger(Translate(L"chat_clan_create_result_fail_not_found", L"name", sName));
}


/*====================
  CChatManager::HandleClanCreateFailedDuplicate
  ====================*/
void	CChatManager::HandleClanCreateFailedDuplicate(CPacket& pkt)
{
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_fail_dupliate")));
	ChatClanCreateFail.Trigger(Translate(_T("chat_clan_create_result_fail_dupliate")));
}


/*====================
  CChatManager::HandleClanCreateFailedParam
  ====================*/
void	CChatManager::HandleClanCreateFailedParam(CPacket& pkt)
{
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_fail_param")));
	ChatClanCreateFail.Trigger(Translate(_T("chat_clan_create_result_fail_param")));
}


/*====================
  CChatManager::HandleClanCreateFailedClanName
  ====================*/
void	CChatManager::HandleClanCreateFailedClanName(CPacket& pkt)
{
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_fail_clan_name")));
	ChatClanCreateFail.Trigger(Translate(_T("chat_clan_create_result_fail_clan_name")));
}


/*====================
  CChatManager::HandleClanCreateFailedTag
  ====================*/
void	CChatManager::HandleClanCreateFailedTag(CPacket& pkt)
{
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_fail_tag")));
	ChatClanCreateFail.Trigger(Translate(_T("chat_clan_create_result_fail_tag")));
}


/*====================
  CChatManager::HandleClanCreateFailedUnknown
  ====================*/
void	CChatManager::HandleClanCreateFailedUnknown(CPacket& pkt)
{
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_fail_unknown")));
	ChatClanCreateFail.Trigger(Translate(_T("chat_clan_create_result_fail_unknown")));
}


/*====================
  CChatManager::HandleNameChange
  ====================*/
void	CChatManager::HandleNameChange(CPacket& pkt)
{
	uint uiAccountID(pkt.ReadInt());
	wstring sName(pkt.ReadWString());

	ChatClientMap_it it(m_mapUserList.find(uiAccountID));

	if (it == m_mapUserList.end())
		return;

	if (uiAccountID != m_uiAccountID)
	{
		if (IsBuddy(uiAccountID))
			RefreshBuddyList();

		if (IsClanMember(uiAccountID))
			RefreshClanList();

		if (IsCafeMember(uiAccountID))
			RefreshCafeList();

		static tsvector vParams(18);
		static tsvector vMiniParams(2);

		for (uiset_it itChan(it->second.setChannels.begin()); itChan != it->second.setChannels.end(); ++itChan)
		{
			if (m_setChannelsIn.find(*itChan) == m_setChannelsIn.end())
				continue;

			// These stay the same throughout the rest of the function
			vParams[0] = vMiniParams[0] = GetChannelName(*itChan);

			vMiniParams[1] = _T("EraseListItemByValue('") + it->second.sName + _T("');");
			ChatUserEvent.Trigger(vMiniParams);

			if (it->second.yStatus > CHAT_CLIENT_STATUS_DISCONNECTED)
			{
				vParams[1] = sName;
				vParams[2] = XtoA(GetAdminLevel(*itChan, it->first));
				vParams[3] = XtoA(it->second.yStatus > CHAT_CLIENT_STATUS_CONNECTED, true);
				vParams[4] = XtoA((it->second.yFlags & CHAT_CLIENT_IS_PREMIUM) != 0, true);
				vParams[5] = XtoA(it->second.uiAccountID);
				vParams[6] = Host.GetChatSymbolTexturePath(it->second.uiChatSymbol);
				vParams[7] = Host.GetChatNameColorTexturePath(it->second.uiChatNameColor);
				vParams[8] = Host.GetChatNameColorString(it->second.uiChatNameColor);
				vParams[9] = Host.GetChatNameColorIngameString(it->second.uiChatNameColor);
				vParams[10] = Host.GetAccountIconTexturePath(it->second.uiAccountIcon, it->second.iAccountIconSlot, it->second.uiAccountID);
				vParams[11] = XtoA(it->second.uiSortIndex);
				vParams[12] = XtoA(Host.GetChatNameGlow(it->second.uiChatNameColor));
				vParams[13] = Host.GetChatNameGlowColorString(it->second.uiChatNameColor);
				vParams[14] = Host.GetChatNameGlowColorIngameString(it->second.uiChatNameColor);
				vParams[15] = XtoA(it->second.uiAscensionLevel);
				vParams[16] = Host.GetChatNameColorFont(it->second.uiChatNameColor);
				vParams[17] = XtoA(Host.GetChatNameBackgroundGlow(it->second.uiChatNameColor));
				ChatUserNames.Trigger(vParams);
			}

			vMiniParams[1] = _T("SortListboxSortIndex();");
			ChatUserEvent.Trigger(vMiniParams);
		}

		it->second.sName = sName;
	}
	else
	{
		// always refresh this and not the buddy list because a player in a clan is always listed on the clan panel
		RefreshClanList();

		it->second.sName = sName;

		UpdateChannels();
		
		CHostClient* pClient(Host.GetActiveClient());
		if (pClient != NULL)
			pClient->SetNickname(sName);
	}
}


/*====================
  CChatManager::HandleAutoMatchConnect
  ====================*/
void	CChatManager::HandleAutoMatchConnect(CPacket& pkt)
{
	EArrangedMatchType eArrangedMatchType(static_cast<EArrangedMatchType>(pkt.ReadByte()));
	uint uiMatchupID(pkt.ReadInt());
	wstring sAddress(pkt.ReadWString());
	ushort unPort(pkt.ReadShort());
	
	// The chat server sends a random integer at the end of the packet so when there are multiple people playing from the
	// same IP address their router/firewall won't start dropping the packets because it thinks they are duplicate/flooding
	uint uiRandomInteger(pkt.ReadInt());

	if (uiRandomInteger != -1)
		Console << L"Received first AutoMatchConnect";
	else
		Console << L"Received second AutoMatchConnect";

#if !defined(K2_STABLE)
	Console << L" for MatchupID#" << XtoA(uiMatchupID) << L" " << XtoA(sAddress) << L":" << XtoA(unPort) << L" (" << uiRandomInteger << ")..." << newl;
#else
    Console << L" for MatchupID#" << XtoA(uiMatchupID) << L" (" << uiRandomInteger << ")..." << newl;
#endif

	if (sAddress.empty() || unPort == 0 || Host.IsConnected() || pkt.HasFaults())
		return;

	if (IsMatchmakingType(eArrangedMatchType))
	{
		// Send a response back to the chat server for tracking purposes, depending on if this was the first connection message or the reminder
		if (uiRandomInteger != -1)
			SendAction(AC_MATCHMAKING_MATCH_READY);
		else
			SendAction(AC_MATCHMAKING_MATCH_READY_REMINDER);
	}

	MatchmakingConnect(sAddress, unPort, eArrangedMatchType);
}


/*====================
  CChatManager::MatchmakingConnect
  ====================*/
void	CChatManager::MatchmakingConnect(const wstring& sAddress, ushort unPort, EArrangedMatchType eMatchType)
{
#if !defined(K2_STABLE)
	Console << L"Connecting to " << XtoA(sAddress) << L":" << XtoA(unPort) << L"..." << newl;
#else
    Console << L"Connecting to game server..." << newl;
#endif

	UnFollow();

	if (IsMatchmakingType(eMatchType))
	{
		// Group gets deleted if we're solo queueing
		bool bSoloQueue(true);
#if !defined(K2_CHINESE)
		for (uint ui(0); ui < MAX_GROUP_SIZE; ++ui)
		{
			if (m_aGroupInfo[ui].uiAccountID != INVALID_INDEX && m_aGroupInfo[ui].uiAccountID != m_uiAccountID)
			{
				bSoloQueue = false;
				break;
			}
		}
#endif // !defined(K2_CHINESE)

		if (bSoloQueue)
			LeaveTMMGroup(true, _T("foundmatch"));
			
		TMMJoinMatch.Trigger(TSNULL, cc_forceTMMInterfaceUpdate);
	}
	else if (eMatchType == AM_SCHEDULED_MATCH)
	{
		ScheduledMatchServerInfo.Trigger(XtoA(SM_SERVER_READY));
	}

	Host.Connect(sAddress + L":" + XtoW(unPort), false, _T("loading_matchmaking_connecting"), true, false, m_bSpectatingScheduledMatch);
}


/*====================
  CChatManager::HandleServerNotIdle
  ====================*/
void	CChatManager::HandleServerNotIdle(CPacket& pkt)
{
	EArrangedMatchType eArrangedMatchType(static_cast<EArrangedMatchType>(pkt.ReadByte()));
	uint uiMatchupID(pkt.ReadInt());
	wstring sAddress(pkt.ReadWString());
	ushort unPort(pkt.ReadShort());

#if !defined(K2_STABLE)
	Console << L"Received ServerNotIdle for MatchupID#" << XtoA(uiMatchupID) << L" " << XtoA(sAddress) << L":" << XtoA(unPort) << L"..." << newl;
#else
    Console << L"Received ServerNotIdle for MatchupID#" << XtoA(uiMatchupID) << newl;
#endif

	if (sAddress.empty() || unPort == 0 || Host.IsConnected() || pkt.HasFaults())
		return;

	if (IsMatchmakingType(eArrangedMatchType))
	{
		SendAction(AC_MATCHMAKING_SERVER_NOT_IDLE);

		LeaveTMMGroup(true, _T("servernotidle"));

		TMMReset.Trigger(TSNULL, cc_forceTMMInterfaceUpdate);
	}
	else if (eArrangedMatchType == AM_SCHEDULED_MATCH)
	{
		Console << "Handle scheduled match server not idle here...";
	}
}


/*====================
  CChatManager::HandleChatRoll
  ====================*/
void	CChatManager::HandleChatRoll(CPacket& pkt)
{
	uint uiAccountID(pkt.ReadInt());
	uint uiChannelID(pkt.ReadInt());
	wstring sMessage(pkt.ReadWString());

	ChatClientMap_it it(m_mapUserList.find(uiAccountID));

	if (it == m_mapUserList.end() || sMessage.empty())
		return;

	if (IsIgnored(it->first))
		return;

	AddIRCChatMessage(CHAT_MESSAGE_ROLL, sMessage, GetChannelName(uiChannelID), true);
}


/*====================
  CChatManager::HandleChatEmote
  ====================*/
void	CChatManager::HandleChatEmote(CPacket& pkt)
{
	uint uiAccountID(pkt.ReadInt());
	uint uiChannelID(pkt.ReadInt());
	wstring sMessage(pkt.ReadWString());

	ChatClientMap_it it(m_mapUserList.find(uiAccountID));

	if (it == m_mapUserList.end() || sMessage.empty())
		return;

	if (IsIgnored(it->first))
		return;

	AddIRCChatMessage(CHAT_MESSAGE_EMOTE, sMessage, GetChannelName(uiChannelID), true);
}


/*====================
  CChatManager::HandleSetChatModeType
  ====================*/
void	CChatManager::HandleSetChatModeType(CPacket& pkt)
{
	byte yChatModeType(pkt.ReadByte());
	wstring sReason(pkt.ReadWString());
	
	m_yChatModeType = yChatModeType;
	
	switch (yChatModeType)
	{
		case CHAT_MODE_AVAILABLE:
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(sReason));
			break;
			
		case CHAT_MODE_AFK:
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_command_afk_message", L"reason", sReason));
			break;
			
		case CHAT_MODE_DND:				
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_command_dnd_message", L"reason", sReason));
			break;
			
		case CHAT_MODE_INVISIBLE:		
			break;
		
		default:
			break;	
	}
}


/*====================
  CChatManager::HandleChatModeAutoResponse
  ====================*/
void	CChatManager::HandleChatModeAutoResponse(CPacket& pkt)
{
	byte yChatModeType(pkt.ReadByte());
	wstring sTargetName(pkt.ReadWString());
	wstring sMessage(pkt.ReadWString());

	switch (yChatModeType)
	{
		case CHAT_MODE_AFK:
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_mode_afk_auto_response", L"target", sTargetName, L"message", sMessage), WSNULL, true);	
			break;
		
		case CHAT_MODE_DND:
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_mode_dnd_auto_response", L"target", sTargetName, L"message", sMessage), WSNULL, true);	
			break;		
			
		default:
			break;	
	}	
}


/*====================
  CChatManager::HandleUserCount
  ====================*/
void	CChatManager::HandleUserCount(CPacket& pkt)
{
	const uint uiTotalUserCount(pkt.ReadInt());
	const wstring sRegionalUserCounts(pkt.ReadWString());
	
	tsvector vUserCounts(2);
	vUserCounts[0] = XtoA(uiTotalUserCount);
	vUserCounts[1] = sRegionalUserCounts;
	
	// ChatUsersOnline.Trigger(vUserCounts);
}


/*====================
  CChatManager::UpdateReadyStatus
  ====================*/
void	CChatManager::UpdateReadyStatus()
{
	tsvector vReadyParams(5);
	vReadyParams[0] = XtoA(m_uiAccountID == m_uiTMMGroupLeaderID);
	vReadyParams[1] = XtoA(m_bTMMOtherPlayersReady);
	vReadyParams[2] = XtoA(m_bTMMAllPlayersReady);
	vReadyParams[3] = XtoA(m_aGroupInfo[m_uiTMMSelfGroupIndex].yReadyStatus > 0);
	vReadyParams[4] = XtoA(m_uiTMMStartTime != INVALID_TIME);

	TMMReadyStatus.Trigger(vReadyParams, cc_forceTMMInterfaceUpdate);
}


/*====================
  CChatManager::PendingMatchFrame
  ====================*/
void	CChatManager::PendingMatchFrame()
{
	// Update twice a second
	if (m_uiLastPendingMatchUpdate != INVALID_TIME && m_uiLastPendingMatchUpdate + 500 > Host.GetSystemTime())
		return;
	m_uiLastPendingMatchUpdate = Host.GetSystemTime();

	uint uiTimeLeft(0);
	if (Host.GetSystemTime() < m_uiPendingMatchTimeEnd)
		uiTimeLeft = m_uiPendingMatchTimeEnd - Host.GetSystemTime();

	bool bHasPendingMatch(uiTimeLeft > 0);
	if (bHasPendingMatch && !m_bHadPendingMatchLastUpdate)
		K2System.BringWindowToForeground();
	m_bHadPendingMatchLastUpdate = bHasPendingMatch;

	static tsvector vParams(5);
	vParams[0] = XtoA((uiTimeLeft + 500) / 1000);
	vParams[1] = XtoA(m_uiPendingMatchLength / 1000);
	vParams[2] = XtoA(m_uiPendingMatchNumPlayers);
	vParams[3] = XtoA(m_uiPendingMatchNumPlayersAccepted);
	vParams[4] = XtoA(m_bAcceptedPendingMatch);

	TMMMatchPendingAccept.Trigger(vParams);
}


/*====================
  CChatManager::HandleTMMPlayerUpdates
  ====================*/
void	CChatManager::HandleTMMPlayerUpdates(CPacket& pkt)
{
	m_uiTMMBanTime = INVALID_TIME;

	uint uiStartTime(K2System.Microseconds());

	// This handles new groups being created, and players getting kicked/leaving/joining from the groups because once the group 
	// changes another update would need to be sent anyways.  It is designed to be stateless so any update will always provide 
	// all the information required so we can avoid synchronization complications
	ETMMUpdateType eUpdateType(static_cast<ETMMUpdateType>(pkt.ReadByte()));
	uint uiAccountID(pkt.ReadInt());
	byte yGroupSize(pkt.ReadByte());
	ushort unAverageTMR(pkt.ReadShort());
	uint uiGroupLeaderAccountID(pkt.ReadInt());
	EArrangedMatchType eArrangedMatchType(static_cast<EArrangedMatchType>(pkt.ReadByte()));
	byte yGameType(pkt.ReadByte());
	tstring sMapName(pkt.ReadTString());
	tstring sGameModes(pkt.ReadTString());
	tstring sRegions(pkt.ReadTString());
	bool bRanked(pkt.ReadByte() != 0);
	uint uiMatchFidelity(pkt.ReadByte());
	byte yBotDifficulty(pkt.ReadByte());
	bool bRandomizeBots(pkt.ReadByte() != 0);
	tstring sRestrictedRegions(pkt.ReadTString());
	tstring sPlayerInvitationResponses(pkt.ReadTString());
	byte yTeamSize(pkt.ReadByte());
	byte yTMMType(pkt.ReadByte());

	if (pkt.HasFaults() || yGroupSize > MAX_GROUP_SIZE)
		return;

	if (cc_printTMMUpdates)
	{
		Console << L"Received TMM update " << g_sTMMUpdateTypes[eUpdateType] << newl;
	}
	
	// We need to store these out permanently so we can check for restricted regions
	m_yGroupSize = yGroupSize;

	m_yArrangedMatchType = eArrangedMatchType;
	m_yGameType = yGameType;
	m_sTMMMapName = sMapName;
	cc_TMMMatchFidelity = uiMatchFidelity;
	m_sRestrictedRegions = sRestrictedRegions;

	bool bHandleFullUpdate(eUpdateType == TMM_CREATE_GROUP || eUpdateType == TMM_FULL_GROUP_UPDATE || eUpdateType == TMM_PLAYER_JOINED_GROUP || eUpdateType == TMM_PLAYER_LEFT_GROUP || eUpdateType == TMM_PLAYER_KICKED_FROM_GROUP);

	if (bHandleFullUpdate)
	{
		for (uint ui(0); ui < MAX_GROUP_SIZE; ++ui)
			m_aGroupInfo[ui].Clear();
	}
	
	for (uint i(0); i < m_yGroupSize; ++i)
	{
		if (bHandleFullUpdate)
		{
			m_aGroupInfo[i].uiAccountID = pkt.ReadInt();
			m_aGroupInfo[i].sName = pkt.ReadTString();
			m_aGroupInfo[i].ySlot = pkt.ReadByte();
			m_aGroupInfo[i].uiCampaignMedalNormal = pkt.ReadInt();
			m_aGroupInfo[i].uiCampaignMedalCasual = pkt.ReadInt();
			m_aGroupInfo[i].iCampaignRankingNormal = pkt.ReadInt();
			m_aGroupInfo[i].iCampaignrankingCasual = pkt.ReadInt();
			m_aGroupInfo[i].bEligibleForCampaign = (pkt.ReadByte() != 0);
			m_aGroupInfo[i].unRating = pkt.ReadShort();

			// Don't check if eligible for campaign when the game is not campaign game
			if (yGameType != TMM_GAME_TYPE_CAMPAIGN_NORMAL && yGameType != TMM_GAME_TYPE_CAMPAIGN_CASUAL)
				m_aGroupInfo[i].bEligibleForCampaign = true;
		}

		m_aGroupInfo[i].yLoadingPercent = pkt.ReadByte();
		m_aGroupInfo[i].yReadyStatus = pkt.ReadByte();
		m_aGroupInfo[i].bInGame = (pkt.ReadByte() != 0);

		if (bHandleFullUpdate)
		{
			m_aGroupInfo[i].bEligibleForRanked = (pkt.ReadByte() != 0);
			m_aGroupInfo[i].uiChatNameColor = (Host.LookupChatNameColor(pkt.ReadTString()));
			GetAccountIconInfo(pkt.ReadTString(), m_aGroupInfo[i].uiAccountIcon, m_aGroupInfo[i].iAccountIconSlot);
			m_aGroupInfo[i].sCountry = pkt.ReadTString();
			m_aGroupInfo[i].bGameModeAccess = (pkt.ReadByte() != 0);
			m_aGroupInfo[i].sGameModeAccess = pkt.ReadWString();

			// Store this player info in our local user list
			ChatClientMap_it findit(m_mapUserList.find(m_aGroupInfo[i].uiAccountID));

			if (findit == m_mapUserList.end())
			{
				SChatClient structClient;
				structClient.uiAccountID = m_aGroupInfo[i].uiAccountID;
				findit = m_mapUserList.insert(ChatClientPair(m_aGroupInfo[i].uiAccountID, structClient)).first;
				m_mapNameToAccountID.insert(pair<wstring, uint>(LowerString(StripClanTag(m_aGroupInfo[i].sName)), m_aGroupInfo[i].uiAccountID));
			}

			findit->second.sName = m_aGroupInfo[i].sName;
			findit->second.uiChatNameColor = m_aGroupInfo[i].uiChatNameColor;
			findit->second.uiAccountIcon = m_aGroupInfo[i].uiAccountIcon;
			findit->second.iAccountIconSlot = m_aGroupInfo[i].iAccountIconSlot;
		}

		// If someone is leaving or being kicked from the group
		if (eUpdateType == TMM_PLAYER_LEFT_GROUP || eUpdateType == TMM_PLAYER_KICKED_FROM_GROUP)
		{
			// Don't display their information in the update as they aren't there anymore
			if (uiAccountID == m_aGroupInfo[i].uiAccountID)
				m_aGroupInfo[i].Clear();
		}
	}

	if (bHandleFullUpdate)
	{
		// This information is local to us, rather than generic to the group, so it needs to be read from the end
		for (uint i(0) ; i < m_yGroupSize ; ++i)
		{
			m_aGroupInfo[i].bFriend = (pkt.ReadByte() != 0);
		}
	}

	if (pkt.HasFaults())
		return;

	m_uiTMMGroupLeaderID = uiGroupLeaderAccountID;
	m_uiTMMSelfGroupIndex = 0;
	m_bTMMOtherPlayersReady = true;
	m_bTMMAllPlayersReady = true;

	for (uint i(0); i < m_yGroupSize; ++i)
	{
		if (m_aGroupInfo[i].uiAccountID == INVALID_INDEX)
			continue;

		if (m_aGroupInfo[i].uiAccountID == m_uiAccountID)
		{
			m_uiTMMSelfGroupIndex = i;

			CHostClient* pClient(Host.GetActiveClient());
			if (pClient != NULL)
			{
				if (m_aGroupInfo[i].unRating != INVALID_USHORT)
				{
					if (m_yGameType == 1)
						pClient->GetAccount().SetNormalMMR(m_aGroupInfo[i].unRating);
					else if (m_yGameType  == 2)
						pClient->GetAccount().SetCasualMMR(m_aGroupInfo[i].unRating);
				}
			}
		}

		if (m_aGroupInfo[i].yReadyStatus != 1)
		{
			if (m_aGroupInfo[i].uiAccountID != m_uiTMMGroupLeaderID)
				m_bTMMOtherPlayersReady = false;

			m_bTMMAllPlayersReady = false;
		}
	}

	tsvector vParams(42);

	uint uiIndex(0);
	for (uint i(0); i < m_yGroupSize; ++i)
	{
		const uint uiSlotAccountID(m_aGroupInfo[i].uiAccountID);

		vParams[uiIndex++] = XtoA(uiSlotAccountID);				// Slot Account ID
		vParams[uiIndex++] = m_aGroupInfo[i].sName;				// Slot Username
		vParams[uiIndex++] = XtoA(m_aGroupInfo[i].ySlot);			// Slot number
		vParams[uiIndex++] = XtoA(m_aGroupInfo[i].unRating);		// Slot TMR

		const byte yLoadingPercent(m_aGroupInfo[i].yLoadingPercent);
		const byte yReadyStatus(m_aGroupInfo[i].yReadyStatus);

		vParams[uiIndex++] = XtoA(XtoA(yLoadingPercent) + L"|" + XtoA(yReadyStatus));		// Player Loading TMM Status | Player Ready Status

		// If someone is leaving or being kicked from the group
		if (eUpdateType == TMM_PLAYER_LEFT_GROUP || eUpdateType == TMM_PLAYER_KICKED_FROM_GROUP)
		{
			// Don't display their information in the update as they aren't there anymore
			if (uiAccountID == uiSlotAccountID)
			{
				vParams[uiIndex - 5] = TSNULL;
				vParams[uiIndex - 4] = TSNULL;
				vParams[uiIndex - 3] = TSNULL;
				vParams[uiIndex - 2] = TSNULL;
				vParams[uiIndex - 1] = TSNULL;
			}
		}
	}

	vParams[25] = XtoA(eUpdateType);
	vParams[26] = XtoA(m_yGroupSize);
	vParams[27] = XtoA(unAverageTMR);
	vParams[28] = XtoA(uiGroupLeaderAccountID);
	vParams[29] = XtoA(m_yGameType);
	vParams[30] = m_sTMMMapName;
	vParams[31] = sGameModes;
	vParams[32] = sRegions;
	vParams[33] = TSNULL;
	vParams[34] = sPlayerInvitationResponses;
	vParams[35] = XtoA(yTeamSize);
	vParams[36] = TSNULL;
	vParams[37] = XtoA(false); // Unused verified Placeholder
	vParams[38] = XtoA(bRanked);
	vParams[39] = XtoA(yBotDifficulty);
	vParams[40] = XtoA(bRandomizeBots);
	vParams[41] = XtoA(yTMMType);

	bool bTriggerReset(false);

	if (eUpdateType == TMM_CREATE_GROUP)
	{
		Console << L"Created team TMM group..." << newl;

		m_bInGroup = true;
	}
	else if (eUpdateType == TMM_PARTIAL_GROUP_UPDATE || eUpdateType == TMM_FULL_GROUP_UPDATE)
	{
		Console << L"Received " << (eUpdateType == TMM_PARTIAL_GROUP_UPDATE ? L"partial" : L"full") << L" TMM group update..." << newl;

		m_bInGroup = true;
	}
	else if (eUpdateType == TMM_PLAYER_JOINED_GROUP)
	{
		Console << L"AccountID " << uiAccountID << L" connected to the TMM group..." << newl;
		
		m_bInGroup = true;

		if (uiAccountID == GetAccountID())
		{
			UnFollow();

			TMMJoinGroup.Trigger(TSNULL, cc_forceTMMInterfaceUpdate);
			Console << L"You (" << uiAccountID << L") joined the TMM group..." << newl;
		}
	}
	else if (eUpdateType == TMM_PLAYER_LEFT_GROUP)
	{
		Console << L"AccountID " << uiAccountID << L" left the TMM group..." << newl;
		
		if (uiAccountID == uiGroupLeaderAccountID)
		{
			LeaveTMMGroup(true, _T("disbanded"));
			Console << L"The group was disbanded by the group leader " << uiAccountID << newl;
		}
		
		if (uiAccountID == GetAccountID())
		{
			LeaveTMMGroup(true, _T("left"));
			Console << L"You (" << uiAccountID << L") left the TMM group..." << newl;
			bTriggerReset = true;
		}
	}
	else if (eUpdateType == TMM_PLAYER_KICKED_FROM_GROUP)
	{
		Console << L"AccountID " << uiAccountID << L" was kicked from TMM group..." << newl;
		
		if (uiAccountID == GetAccountID())
		{
			LeaveTMMGroup(true, _T("kicked"));
			Console << L"You (" << uiAccountID << L") were kicked from the TMM group..." << newl;
		}
	}
	
	if (m_bInGroup)
		TMMDisplay.Trigger(vParams, cc_forceTMMInterfaceUpdate);
	
	if (bTriggerReset)
	{
		TMMReset.Trigger(TSNULL, cc_forceTMMInterfaceUpdate);
	}

	UpdateReadyStatus();

	TMMNewPlayerStatus.Trigger(TSNULL, cc_forceTMMInterfaceUpdate);

	for (uint ui(0); ui < MAX_GROUP_SIZE; ++ui)
	{
		tsvector vPlayerParams(22);

		if (m_aGroupInfo[ui].uiAccountID != INVALID_INDEX)
		{
			vPlayerParams[0] = XtoA(m_aGroupInfo[ui].uiAccountID);
			vPlayerParams[1] = XtoA(m_aGroupInfo[ui].sName);
			vPlayerParams[2] = XtoA(m_aGroupInfo[ui].ySlot);
			vPlayerParams[3] = XtoA(m_aGroupInfo[ui].unRating);
			vPlayerParams[4] = XtoA(m_aGroupInfo[ui].yLoadingPercent);
			vPlayerParams[5] = XtoA(m_aGroupInfo[ui].yReadyStatus);
			vPlayerParams[6] = XtoA(m_aGroupInfo[ui].uiAccountID == m_uiTMMGroupLeaderID);
			vPlayerParams[7] = XtoA(true);
			vPlayerParams[8] = XtoA(m_aGroupInfo[ui].bEligibleForRanked);
			vPlayerParams[9] = XtoA(m_aGroupInfo[ui].bFriend);
			vPlayerParams[10] = Host.GetChatNameColorString(m_aGroupInfo[ui].uiChatNameColor);
			vPlayerParams[11] = Host.GetChatNameColorTexturePath(m_aGroupInfo[ui].uiChatNameColor);
			vPlayerParams[12] = XtoA(m_aGroupInfo[ui].bGameModeAccess);
			vPlayerParams[13] = XtoA(Host.GetChatNameGlow(m_aGroupInfo[ui].uiChatNameColor));
			vPlayerParams[14] = XtoA(m_aGroupInfo[ui].sGameModeAccess);
			vPlayerParams[15] = XtoA(m_aGroupInfo[ui].bInGame);
			vPlayerParams[16] = Host.GetChatNameGlowColorString(m_aGroupInfo[ui].uiChatNameColor);
			vPlayerParams[17] = XtoA(m_aGroupInfo[ui].uiCampaignMedalNormal); //rank level normal
			vPlayerParams[18] = XtoA(m_aGroupInfo[ui].uiCampaignMedalCasual); //rank level casual
			vPlayerParams[19] = XtoA(m_aGroupInfo[ui].iCampaignRankingNormal); //ranking normal
			vPlayerParams[20] = XtoA(m_aGroupInfo[ui].iCampaignrankingCasual); //ranking casual
			vPlayerParams[21] = XtoA(m_aGroupInfo[ui].bEligibleForCampaign);
		}
		else
		{
			vPlayerParams[7] = XtoA(ui < yTeamSize);
		}

		TMMPlayerStatus[ui]->Trigger(vPlayerParams, cc_forceTMMInterfaceUpdate);
	}

	if (chat_profile)
		Console << _T("HandleTMMPlayerUpdates - ") << K2System.Microseconds() - uiStartTime << _T(" us") << newl;
}


/*====================
  CChatManager::HandleTMMPopularityUpdates
  ====================*/
void	CChatManager::HandleTMMPopularityUpdates(CPacket& pkt)
{
	byte yTMMEnabled(pkt.ReadByte());
	wstring sAvailableMapNames(pkt.ReadWString());
	wstring sAvailableGameTypes(pkt.ReadWString());
	wstring sAvailableGameModes(pkt.ReadWString());
	wstring sAvailableRegions(pkt.ReadWString());
	wstring sDisabledGameModesByGameType(pkt.ReadWString());
	wstring sDisabledGameModesByRankType(pkt.ReadWString());
	wstring sDisabledGameModesByMap(pkt.ReadWString());

	// Reading the available and restricted regions so we can disable/enable them properly before they join a group
	m_sAvailableRegions = XtoA(sAvailableRegions);
	m_sRestrictedRegions = pkt.ReadWString();
	
	// We only send the actual country the first time we login so don't overwrite it with an empty string
	wstring sCountry(pkt.ReadWString());
	if (sCountry != WSNULL)
		m_sCountry = sCountry;
	
	wstring sLegend(pkt.ReadWString());

	if (pkt.HasFaults())
		return;

	m_vDisabledGameModesByGameType = TokenizeString(sDisabledGameModesByGameType, L'|');
	m_vDisabledGameModesByRankType = TokenizeString(sDisabledGameModesByRankType, L'|');
	m_vDisabledGameModesByMap = TokenizeString(sDisabledGameModesByMap, L'|');

	// Parse out the legend as sent from the chat server so we can lookup and display popularities correctly
	size_t zMapPos(sLegend.find(L"maps:"));
	size_t zGameModePos(sLegend.find(L"modes:"));
	size_t zRegionPos(sLegend.find(L"regions:"));

	if (zMapPos == string::npos || zGameModePos == string::npos || zRegionPos == string::npos)
		return;

	wsvector vMapLegend(TokenizeString(sLegend.substr(zMapPos + 5, zGameModePos - 6), L'|'));
	wsvector vGameModeLegend(TokenizeString(sLegend.substr(zGameModePos + 6, zRegionPos - zGameModePos - 6), L'|'));
	wsvector vRegionLegend(TokenizeString(sLegend.substr(zRegionPos + 8), L'|'));

	for (wsvector_cit citMapLegend(vMapLegend.begin()), citMapLegendEnd(vMapLegend.end()); citMapLegend != citMapLegendEnd; ++citMapLegend)
	{
		wsvector vMapInfo(TokenizeString((*citMapLegend), L'-'));

		if (vMapInfo.size() == 2)
			m_mapMapLookup[vMapInfo[0]] = AtoI(vMapInfo[1]);
	}

	for (wsvector_cit citGameModeLegend(vGameModeLegend.begin()), citGameModeLegendEnd(vGameModeLegend.end()); citGameModeLegend != citGameModeLegendEnd; ++citGameModeLegend)
	{
		wsvector vGameModeInfo(TokenizeString((*citGameModeLegend), L'-'));

		if (vGameModeInfo.size() == 2)
			m_mapGameModeLookup[vGameModeInfo[0]] = AtoI(vGameModeInfo[1]);
	}

	for (wsvector_cit citRegionLegend(vRegionLegend.begin()), citRegionLegendEnd(vRegionLegend.end()); citRegionLegend != citRegionLegendEnd; ++citRegionLegend)
	{
		wsvector vRegionInfo(TokenizeString((*citRegionLegend), L'-'));

		if (vRegionInfo.size() == 2)
			m_mapRegionLookup[vRegionInfo[0]] = AtoI(vRegionInfo[1]);
	}
	// Done parsing out legend


	// Reset all the popularities back to 0
	m_TMMPopularities.Clear();

	wsvector vMapNames(TokenizeString(sAvailableMapNames, L'|'));
	wsvector vGameTypes(TokenizeString(sAvailableGameTypes, L'|'));
	wsvector vGameModes(TokenizeString(sAvailableGameModes, L'|'));
	wsvector vRegions(TokenizeString(sAvailableRegions, L'|'));

	
	// Read popularities for maps
	for (wsvector_it itMapName(vMapNames.begin()), itEnd(vMapNames.end()); itMapName != itEnd; ++itMapName)
	{
		if (m_mapMapLookup.find(*itMapName) == m_mapMapLookup.end())
		{
			pkt.ReadByte();
			continue;
		}

		ETMMGameMaps eGameMap(static_cast<ETMMGameMaps>(m_mapMapLookup[*itMapName]));

		for (wsvector_it itGameType(vGameTypes.begin()), itEnd(vGameTypes.end()); itGameType != itEnd; ++itGameType)
		{
			ETMMGameTypes eGameType(static_cast<ETMMGameTypes>(AtoI(*itGameType)));

			for (uint uiRankType(0); uiRankType < TMM_NUM_OPTION_RANK_TYPES; ++uiRankType)
			{
				if (eGameMap != TMM_GAME_MAP_NONE && eGameMap < TMM_NUM_GAME_MAPS && eGameType != TMM_GAME_TYPE_NONE && eGameType < TMM_NUM_GAME_TYPES)
					m_TMMPopularities.ayGameMap[eGameMap][eGameType][uiRankType] = pkt.ReadByte();
				else
					pkt.ReadByte();
			}
		}
	}

	// Read popularities for game types
	for (wsvector_it itGameType(vGameTypes.begin()), itEnd(vGameTypes.end()); itGameType != itEnd; ++itGameType)
	{
		ETMMGameTypes eGameType(static_cast<ETMMGameTypes>(AtoI(*itGameType)));

		for (wsvector_it itMapName(vMapNames.begin()), itEnd(vMapNames.end()); itMapName != itEnd; ++itMapName)
		{
			if (m_mapMapLookup.find(*itMapName) == m_mapMapLookup.end())
			{
				pkt.ReadByte();
				continue;
			}

			ETMMGameMaps eGameMap(static_cast<ETMMGameMaps>(m_mapMapLookup[*itMapName]));

			for (uint uiRankType(0); uiRankType < TMM_NUM_OPTION_RANK_TYPES; ++uiRankType)
			{
				if (eGameType != TMM_GAME_TYPE_NONE && eGameType < TMM_NUM_GAME_TYPES && eGameMap != TMM_GAME_MAP_NONE && eGameMap < TMM_NUM_GAME_MAPS)
					m_TMMPopularities.ayGameType[eGameType][eGameMap][uiRankType] = pkt.ReadByte();
				else
					pkt.ReadByte();
			}
		}
	}

	// Read popularities for game modes
	for (wsvector_it itGameMode(vGameModes.begin()), itEnd(vGameModes.end()); itGameMode != itEnd; ++itGameMode)
	{
		if (m_mapGameModeLookup.find(*itGameMode) == m_mapGameModeLookup.end())
		{
			pkt.ReadByte();
			continue;
		}

		ETMMGameModes eGameMode(static_cast<ETMMGameModes>(m_mapGameModeLookup[*itGameMode]));

		for (wsvector_it itMapName(vMapNames.begin()), itEnd(vMapNames.end()); itMapName != itEnd; ++itMapName)
		{
			if (m_mapMapLookup.find(*itMapName) == m_mapMapLookup.end())
			{
				pkt.ReadByte();
				continue;
			}

			ETMMGameMaps eGameMap(static_cast<ETMMGameMaps>(m_mapMapLookup[*itMapName]));

			for (wsvector_it itGameType(vGameTypes.begin()), itEnd(vGameTypes.end()); itGameType != itEnd; ++itGameType)
			{
				ETMMGameTypes eGameType(static_cast<ETMMGameTypes>(AtoI(*itGameType)));

				for (uint uiRankType(0); uiRankType < TMM_NUM_OPTION_RANK_TYPES; ++uiRankType)
				{
					if (eGameMode != TMM_GAME_MODE_NONE && eGameMode < TMM_NUM_GAME_MODES && eGameMap != TMM_GAME_MAP_NONE && eGameMap < TMM_NUM_GAME_MAPS && eGameType != TMM_GAME_TYPE_NONE && eGameType < TMM_NUM_GAME_TYPES)
						m_TMMPopularities.ayGameMode[eGameMode][eGameMap][eGameType][uiRankType] = pkt.ReadByte();
					else
						pkt.ReadByte();
				}
			}
		}
	}

	// Read popularities for regions
	for (wsvector_it itRegion(vRegions.begin()), itEnd(vRegions.end()); itRegion != itEnd; ++itRegion)
	{
		if (m_mapRegionLookup.find(*itRegion) == m_mapRegionLookup.end())
		{
			pkt.ReadByte();
			continue;
		}

		ETMMGameRegions eRegion(static_cast<ETMMGameRegions>(m_mapRegionLookup[*itRegion]));

		for (wsvector_it itMapName(vMapNames.begin()), itEnd(vMapNames.end()); itMapName != itEnd; ++itMapName)
		{
			if (m_mapMapLookup.find(*itMapName) == m_mapMapLookup.end())
			{
				pkt.ReadByte();
				continue;
			}

			ETMMGameMaps eGameMap(static_cast<ETMMGameMaps>(m_mapMapLookup[*itMapName]));

			for (wsvector_it itGameType(vGameTypes.begin()), itEnd(vGameTypes.end()); itGameType != itEnd; ++itGameType)
			{
				ETMMGameTypes eGameType(static_cast<ETMMGameTypes>(AtoI(*itGameType)));

				for (uint uiRankType(0); uiRankType < TMM_NUM_OPTION_RANK_TYPES; ++uiRankType)
				{
					if (eRegion != TMM_GAME_REGION_NONE && eRegion < NUM_TMM_GAME_REGIONS && eGameMap != TMM_GAME_MAP_NONE && eGameMap < TMM_NUM_GAME_MAPS && eGameType != TMM_GAME_TYPE_NONE && eGameType < TMM_NUM_GAME_TYPES)
						m_TMMPopularities.ayRegion[eRegion][eGameMap][eGameType][uiRankType] = pkt.ReadByte();
					else
						pkt.ReadByte();
				}
			}
		}
	}
	

	m_uiMapSettingPhaseEndTime = 0;
	if (pkt.GetUnreadLength() > 0)
		m_uiMapSettingPhaseEndTime = pkt.ReadInt();

	if (m_uiMapSettingPhaseEndTime == 0)
		ChatServerMapSettingPhaseEndTime.Trigger(_T("-1"));
	else
	{
		time_t tEnd = m_uiMapSettingPhaseEndTime;

		tsvector vTime;

		struct tm * sTime = localtime(&tEnd);

		vTime.push_back(XtoA(sTime->tm_year+1900));
		vTime.push_back(XtoA(sTime->tm_mon + 1));
		vTime.push_back(XtoA(sTime->tm_mday));
		vTime.push_back(XtoA(sTime->tm_hour));
		vTime.push_back(XtoA(sTime->tm_min));
		vTime.push_back(XtoA(sTime->tm_sec));
		ChatServerMapSettingPhaseEndTime.Trigger(vTime);
	}

	static wsvector vOptionsAvailableParams(5);

	vOptionsAvailableParams[0] = sAvailableGameTypes;
	vOptionsAvailableParams[1] = sAvailableMapNames;
	vOptionsAvailableParams[2] = sAvailableGameModes;
	vOptionsAvailableParams[3] = sAvailableRegions;
	vOptionsAvailableParams[4] = XtoA(yTMMEnabled);
	
	TMMOptionsAvailable.Trigger(vOptionsAvailableParams, cc_forceTMMInterfaceUpdate);

	if (yTMMEnabled)
		m_bTMMEnabled = true;
	else
		m_bTMMEnabled = false;

	static wsvector vTMMAvailableParams(1);

	vTMMAvailableParams[0] = XtoA(m_bTMMEnabled);
	
	TMMAvailable.Trigger(vTMMAvailableParams, cc_forceTMMInterfaceUpdate);
}


/*====================
  CChatManager::HandleTMMQueueUpdates
  ====================*/
void	CChatManager::HandleTMMQueueUpdates(CPacket& pkt)
{
	const byte yUpdateType(pkt.ReadByte());
	
	if (yUpdateType == TMM_GROUP_QUEUE_UPDATE)
	{
		const uint uiAverageTimeQueued(pkt.ReadInt());

		if (pkt.HasFaults())
			return;

		m_uiTMMAverageQueueTime = uiAverageTimeQueued;
	}
	else if (yUpdateType == TMM_GROUP_FOUND_SERVER)
	{
		SendAction(AC_MATCHMAKING_SERVER_FOUND);

		TMMFoundServer.Trigger(TSNULL, cc_forceTMMInterfaceUpdate);

		m_uiPendingMatchTimeEnd = 0;
		m_uiLastPendingMatchUpdate = INVALID_TIME;

		Console << _T("Server found, waiting for response") << newl;
	}
	else if (yUpdateType == TMM_GROUP_NO_MATCHES_FOUND)
	{
		m_bTMMOtherPlayersReady = false;
		m_bTMMAllPlayersReady = false;
		m_uiTMMStartTime = INVALID_TIME;
		TMMNoMatchesFound.Trigger(TSNULL, cc_forceTMMInterfaceUpdate);
	}	
	else if (yUpdateType == TMM_GROUP_NO_SERVERS_FOUND)
	{
		TMMNoServersFound.Trigger(TSNULL, cc_forceTMMInterfaceUpdate);
	}
	else if (yUpdateType == TMM_MATCHMAKING_DISABLED)
	{
		LeaveTMMGroup(true, _T("disabled"));
	}
}


/*====================
  CChatManager::HandleTMMJoinQueue
  ====================*/
void	CChatManager::HandleTMMJoinQueue(CPacket& pkt)
{
	m_uiTMMStartTime = Host.GetTime();

	Console << L"Your group joined the TMM queue..." << newl;

	tstring sData = LogCollector.GetJsonKeyValueString(_T("GamePhase"), _T("JOIN_QUEUE"));
	LogCollector.HttpSend(ELCCmd_GamePhase, ELCLevel_Info, sData);

	TMMJoinQueue.Trigger(TSNULL, cc_forceTMMInterfaceUpdate);

	UpdateReadyStatus();
}


/*====================
  CChatManager::HandleTMMReJoinQueue
  ====================*/
void	CChatManager::HandleTMMReJoinQueue(CPacket& pkt)
{
	uint uiQueuedTime(pkt.ReadInt());

	if (pkt.HasFaults())
		return;

	Console << L"Your group was placed back into the queue at the previous wait time the group leader waited..." << newl;

	AddIRCChatMessage(CHAT_MESSAGE_GROUP, Translate(_CTS("chat_group_queue_rejoin")), TSNULL, true);

	m_uiTMMStartTime = Host.GetTime() - SecToMs(uiQueuedTime);
}



/*====================
  CChatManager::HandleTMMGenericResponse
  ====================*/
void	CChatManager::HandleTMMGenericResponse(CPacket& pkt)
{
	uint uiResponse(pkt.ReadInt());

	if (pkt.HasFaults())
		return;

	switch (uiResponse)
	{
	case GR_MAX_MATCH_FIDELITY_DIFFERENCE:
		AddIRCChatMessage(CHAT_MESSAGE_GROUP, Translate(_CTS("chat_group_max_match_fidelity_diff")), TSNULL, true);
		break;

	case GR_SCHEDULED_MATCH_FULL:
		LeaveScheduledMatch(true, _T("teamfull"));
		break;

	default:
		break;
	}
}


/*====================
  CChatManager::HandleTMMLeaveQueue
  ====================*/
void	CChatManager::HandleTMMLeaveQueue(CPacket& pkt)
{
	m_uiTMMStartTime = INVALID_TIME;
	m_uiTMMAverageQueueTime = INVALID_TIME;

	static tsvector vMiniParams(3);

	vMiniParams[0] = XtoA(0);
	vMiniParams[1] = XtoA(0);
	vMiniParams[2] = XtoA(0);

	TMMTime.Trigger(vMiniParams, cc_forceTMMInterfaceUpdate);	

	Console << L"Your group left the TMM queue..." << newl;

	TMMLeaveQueue.Trigger(TSNULL, cc_forceTMMInterfaceUpdate);

	UpdateReadyStatus();
}


/*====================
  CChatManager::HandleTMMInviteToGroup
  ====================*/
void	CChatManager::HandleTMMInviteToGroup(CPacket& pkt)
{
	wstring sInviter(pkt.ReadWString());
	uint uiAccountID(pkt.ReadInt());
	byte yStatus(pkt.ReadByte());
	byte yFlags(pkt.ReadByte());

	uint uiChatNameColor(Host.LookupChatNameColor(pkt.ReadTString()));

	uint uiAccountIcon(0);
	int iAccountIconSlot(0);		
	GetAccountIconInfo(pkt.ReadTString(), uiAccountIcon, iAccountIconSlot);

	uint uiAscensionLevel(pkt.ReadInt());

	ChatClientMap_it it(m_mapUserList.find(uiAccountID));
	if (it == m_mapUserList.end())
	{
		it = m_mapUserList.insert(ChatClientPair(uiAccountID, SChatClient())).first;
		m_mapNameToAccountID.insert(pair<wstring, uint>(LowerString(StripClanTag(sInviter)), uiAccountID));
	}

	it->second.sName = sInviter;
	it->second.uiAccountID = uiAccountID;
	it->second.yStatus = yStatus;
	it->second.yFlags = yFlags;
	it->second.uiChatNameColor = uiChatNameColor;
	it->second.uiAccountIcon = uiAccountIcon;
	it->second.iAccountIconSlot = iAccountIconSlot;
	it->second.uiAscensionLevel = uiAscensionLevel;

	wstring sMapName(pkt.ReadWString());
	byte yGameType(pkt.ReadByte());
	wstring sGameModes(pkt.ReadWString());
	wstring sRegions(pkt.ReadWString());
	
	if (pkt.HasFaults())
		return;
	
	if (IsIgnored(sInviter) || (GetFriendlyChat() && !IsCompanion(sInviter)))
		return;

	static wsvector vInvite(4);	
	vInvite[0] = sMapName;
	vInvite[1] = XtoA(yGameType);
	vInvite[2] = sGameModes;
	vInvite[3] = sRegions;
	
	Console << L"You were invited to join the TMM group by " << sInviter << newl;
	
	PushNotification(NOTIFY_TYPE_TMM_GROUP_INVITE, sInviter, TSNULL, TSNULL, vInvite);
}


/*====================
  CChatManager::HandleTMMInviteToGroupBroadcast
  ====================*/
void	CChatManager::HandleTMMInviteToGroupBroadcast(CPacket& pkt)
{
	const wstring sInvited(pkt.ReadWString());
	const wstring sInviter(pkt.ReadWString());
	
	if (pkt.HasFaults())
		return;
	
	Console << sInvited << L" was invited to join the TMM group by " << sInviter << L"..." << newl;

	AddIRCChatMessage(CHAT_MESSAGE_GROUP, Translate(_CTS("chat_group_invite_broadcast"), _CTS("invited"), sInvited, _CTS("inviter"), sInviter), TSNULL, true);
}


/*====================
  CChatManager::HandleTMMRejectInvite
  ====================*/
void	CChatManager::HandleTMMRejectInvite(CPacket& pkt)
{
	const wstring sInvited(pkt.ReadWString());
	const wstring sInviter(pkt.ReadWString());
	
	if (pkt.HasFaults())
		return;
	
	Console << sInvited << L" rejected the TMM group invite from " << sInviter << L"..." << newl;
}


/*====================
  CChatManager::HandleTMMMatchFound
  ====================*/
void	CChatManager::HandleTMMMatchFound(CPacket& pkt)
{
	const wstring sMapName(pkt.ReadWString());
	const byte yTeamSize(pkt.ReadByte());
	const byte yGameType(pkt.ReadByte());
	const wstring sGameMode(pkt.ReadWString());
	const wstring sRegion(pkt.ReadWString());
	wstring sExtraMatchInfo(pkt.ReadWString());
	
	if (pkt.HasFaults())
		return;

	SendAction(AC_MATCHMAKING_MATCH_FOUND);

	TMMFoundMatch.Trigger(TSNULL, cc_forceTMMInterfaceUpdate);

	wstring sOtherMatchInfo;
	sOtherMatchInfo = L"Game Type:" + XtoA(yGameType);
	sOtherMatchInfo += L"|Map Name:" + sMapName;
	sOtherMatchInfo += L"|Team Size:" + XtoA(yTeamSize);
	sOtherMatchInfo += L"|Game Mode:" + sGameMode;
	sOtherMatchInfo += L"|Region:" + sRegion + L"|";
	
	sExtraMatchInfo = sOtherMatchInfo + sExtraMatchInfo;

	TMMDebugInfo.Trigger(sExtraMatchInfo, cc_forceTMMInterfaceUpdate);
	
	// Just add in some newlines so the console appears properly
	sExtraMatchInfo = StringReplace(sExtraMatchInfo, L"Team1:", L"\nTeam1:");
	sExtraMatchInfo = StringReplace(sExtraMatchInfo, L"Team2:", L"\nTeam2:");
	sExtraMatchInfo = StringReplace(sExtraMatchInfo, L"Group Count:", L"\nGroup Count:");
	sExtraMatchInfo = StringReplace(sExtraMatchInfo, L"Average Matchup %:", L"\nAverage Matchup %:");
	sExtraMatchInfo = StringReplace(sExtraMatchInfo, L"Group Mismatch %:", L"\nGroup Mismatch %:");	

	Console << L"Your TMM group left the queue and was placed into a match!!" << newl;
	Console << L"Game Type: " << yGameType << newl;
	Console << L"Map Name: " << sMapName << L" Team Size:" << yTeamSize << newl;
	Console << L"Game Mode: " << sGameMode << newl;
	Console << L"Region: " << sRegion << newl;
	Console << L"Extra Match Info: " << sExtraMatchInfo << newl;

	K2System.ActivateWindow();
}


/*====================
  CChatManager::HandleTMMJoinFailed
  ====================*/
void	CChatManager::HandleTMMJoinFailed(CPacket& pkt)
{
	const byte yUpdateType(pkt.ReadByte());
	
	if (pkt.HasFaults())
		return;

	uint uiValue(0);
	tstring sReason;

	switch (yUpdateType)
	{
		case 0:
		{
			sReason = _CTS("isleaver");
			Console << _CTS("Players who are leavers are not allowed in TMM games.") << newl;
		}	break;

		case 1:
		{
			sReason = _CTS("disabled");
			Console << _CTS("TMM is currently disabled, please try back at a later time.") << newl;
		}	break;

		case 2:
		{
			sReason = _CTS("busy");
			Console << _CTS("TMM is currently too busy to accept new groups, please try again in a few minutes.") << newl;
		}	break;

		case 3:
		{
			sReason = _CTS("optionunavailable");
			Console << _CTS("You have tried to create a group using options that are disabled or invalid, please try again.") << newl;
		}	break;

		case 4:
		{
			sReason = _CTS("invalidversion");
			Console << _CTS("The version of your client is not compatible with the matchmaking server.  Please update your client and try again.") << newl;
		}	break;

		case 5:
		{
			sReason = _CTS("groupfull");
			Console << _CTS("Unable to join, the group you are trying to join is full.") << newl;
		}	break;

		case 6:
		{
			sReason = _CTS("badstats");
			Console << _CTS("Unable to join, invalid stats.") << newl;
		}	break;

		case 7:
		{
			sReason = _CTS("groupqueued");
			Console << _CTS("Unable to join, the group has already entered the queue.") << newl;
		}	break;

		case 8:
		{
			sReason = _CTS("istrial");
			Console << _CTS("Trial accounts are not allowed to join TMM games.") << newl;
		}	break;
		
		case 9:
		{
			sReason = _CTS("banned");
			
			uiValue = pkt.ReadInt();

			if (pkt.HasFaults())
				return;

			m_uiTMMBanTime = Host.GetTimeSeconds() + uiValue;
			
			Console << _CTS("You have been temporarily banned from matchmaking because you have failed to load properly multiple times, please try re-join matchmaking again in ") << uiValue << _CTS(" seconds.") << newl;
		}	break;
		case 12:
		{
			sReason = _CTS("noteligible");
			Console << _CTS("The player you invited is not eligible.") << newl;
			break;
		}
		default:
		{
			sReason = _CTS("unknown");
			Console << _CTS("Unable to join, unknown reason.") << newl;
		}	break;
	}

	LeaveTMMGroup(true, sReason, uiValue);
}


/*====================
  CChatManager::HandleRequestBuddyAddResponse
  ====================*/
void	CChatManager::HandleRequestBuddyAddResponse(CPacket& pkt)
{
	// Rather than having two separate methods for the requester and the requested,
	// these vars serve both purposes, so don't get confused
	byte yType(pkt.ReadByte());
	uint uiNotifyID(pkt.ReadInt());
	tstring sNickName(pkt.ReadTString());

	uint uiAccountID(0);
	byte yStatus(0);
	byte yFlags(0);
	int iClanID(0);
	wstring sClan;
	uint uiChatSymbol(0);
	uint uiChatNameColor(0);

	if (yType >= 1 && yType <= 2)
	{
		// Another client that we don't have information on requested us as a buddy, so populate their user info so we can display nice notifications for them
		uiAccountID = pkt.ReadInt();
		yStatus = pkt.ReadByte();
		yFlags = pkt.ReadByte();
		iClanID = pkt.ReadInt();
		sClan = pkt.ReadWString();
		uiChatSymbol = Host.LookupChatSymbol(pkt.ReadTString());
		uiChatNameColor = Host.LookupChatNameColor(pkt.ReadTString());
		
		uint uiAccountIcon(0);
		int iAccountIconSlot(0);
		GetAccountIconInfo(pkt.ReadTString(), uiAccountIcon, iAccountIconSlot);

		uint uiAscensionLevel(pkt.ReadInt());

		ChatClientMap_it it(m_mapUserList.find(uiAccountID));
		if (it == m_mapUserList.end())
		{
			it = m_mapUserList.insert(ChatClientPair(uiAccountID, SChatClient())).first;
			m_mapNameToAccountID.insert(pair<wstring, uint>(LowerString(StripClanTag(sNickName)), uiAccountID));
		}

		it->second.sName = sNickName;
		it->second.uiAccountID = uiAccountID;
		it->second.yStatus = yStatus;
		it->second.yFlags = yFlags;
		it->second.iClanID = iClanID;
		it->second.sClan = sClan;
		it->second.uiChatSymbol = uiChatSymbol;
		it->second.uiChatNameColor = uiChatNameColor;
		it->second.uiAccountIcon = uiAccountIcon;
		it->second.iAccountIconSlot = iAccountIconSlot;
		it->second.uiAscensionLevel = uiAscensionLevel;
	}

	if (pkt.HasFaults())
		return;
		
	if (yType == 1)
	{
		// This requester/adder is getting this message
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_requested_approval_adder"), _CTS("name"), sNickName));
		if (cc_showBuddyRequestNotification)
			PushNotification(NOTIFY_TYPE_BUDDY_REQUESTED_ADDER, sNickName, TSNULL, TSNULL, VSNULL, uiNotifyID);
	}
	else if (yType == 2)
	{
		// Don't show new buddy requests to someone with friendlychat enabled
		if (!GetFriendlyChat())
		{
			// The requested/added is getting this message
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_requested_approval_added"), _CTS("name"), sNickName));
			if (cc_showBuddyRequestNotification)
				PushNotification(NOTIFY_TYPE_BUDDY_REQUESTED_ADDED, sNickName, TSNULL, TSNULL, VSNULL, uiNotifyID);
		}
	}
	else if (yType == 3)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_requested_approval_duplicate"), _CTS("name"), sNickName));
	}
	else if (yType == 4)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_requested_approval_ignored"), _CTS("name"), sNickName));
	}
	else if (yType == 5)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_requested_approval_limit")));
	}
	else if (yType == 0)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_requested_approval_failed"), _CTS("name"), sNickName));
	}	
}


/*====================
  CChatManager::HandleRequestBuddyApproveResponse
  ====================*/
void	CChatManager::HandleRequestBuddyApproveResponse(CPacket& pkt)
{
	// rather than having two separate methods for the approver and the approved,
	// these vars serve both purposes, so don't get confused
	byte yType(pkt.ReadByte());
	uint uiAccountID(pkt.ReadInt());
	uint uiNotifyID(pkt.ReadInt());
	const tstring sAccountNickName(pkt.ReadTString());
	
	if (pkt.HasFaults())
		return;
		
	if (yType == 1)
	{
		// This requester/adder is getting this message
		AddBuddy(uiAccountID, sAccountNickName);
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_approved_buddy_adder"), _T("name"), sAccountNickName));
		if (cc_showBuddyAddNotification)
			PushNotification(NOTIFY_TYPE_BUDDY_ADDER, sAccountNickName, TSNULL, TSNULL, VSNULL, uiNotifyID);
	}
	else if (yType == 2)
	{
		// The requested/added is getting this message
		AddBuddy(uiAccountID, sAccountNickName);
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_approved_buddy_added"), _T("name"), sAccountNickName));
		if (cc_showBuddyAddNotification)
			PushNotification(NOTIFY_TYPE_BUDDY_ADDED, sAccountNickName, TSNULL, TSNULL, VSNULL, uiNotifyID);
	}
	else if (yType == 3)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_requested_approval_limit")));
	}
	else if (yType == 0)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_approving_buddy"), _T("name"), sAccountNickName));
	}
}


/*====================
  CChatManager::HandleStaffJoinMatchResponse
  ====================*/
void	CChatManager::HandleStaffJoinMatchResponse(CPacket& pkt)
{
	wstring sAddr(pkt.ReadWString());
	
	if (pkt.HasFaults() || sAddr.empty())
		return;
		
	Host.Connect(sAddr, false, _CTS("loading"), false, false, true);
}


/*====================
  CChatManager::HandleClanCreateAccept
  ====================*/
void	CChatManager::HandleClanCreateAccept(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_clan_create_accept", L"name", sName));
	ChatClanCreateAccept.Trigger(sName);
}


/*====================
  CChatManager::HandleClanCreateComplete
  ====================*/
void	CChatManager::HandleClanCreateComplete(CPacket& pkt)
{
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_success")));
	ChatClanCreateSuccess.Trigger(Translate(_T("chat_clan_create_result_success")));

	m_uiCreateTimeSent = INVALID_TIME;
}


/*====================
  CChatManager::HandleClanCreateRejected
  ====================*/
void	CChatManager::HandleClanCreateRejected(CPacket& pkt)
{
	wstring sName(pkt.ReadWString());
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_clan_create_reject", L"name", sName));
	ChatClanCreateFail.Trigger(Translate(L"chat_clan_create_result_reject", L"name", sName));
}


/*====================
  CChatManager::HandleNewClanMember
  ====================*/
void	CChatManager::HandleNewClanMember(CPacket& pkt)
{
	uint uiAccountID(pkt.ReadInt());

	ChatClientMap_it it(m_mapUserList.find(uiAccountID));

	if (it == m_mapUserList.end())
		return;

	if (uiAccountID != m_uiAccountID)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_new_user"), _T("name"), it->second.sName));
		if (cc_showClanAddNotification)
			PushNotification(NOTIFY_TYPE_CLAN_ADD, it->second.sName);

		AddClanMember(uiAccountID, it->second.sName);		
	}
	else
	{
		it->second.iClanID = pkt.ReadInt();
		it->second.sClan = pkt.ReadWString();
		it->second.sClanTag = pkt.ReadWString();

		m_setClanList.insert(uiAccountID);

		CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
		if (pHTTPRequest == NULL)
			return;

		pHTTPRequest->SetHost(Host.GetMasterServerAddress());
		pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=clan_list");
		pHTTPRequest->AddVariable(L"clan_id", it->second.iClanID);
		pHTTPRequest->SendPostRequest();

		SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_UPDATE_CLAN, 0, it->second.sClanTag));
		m_lHTTPRequests.push_back(pNewRequest);

		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_new_user_self"), _T("clan"), it->second.sClan));
		if (cc_showClanAddNotification)
			PushNotification(NOTIFY_TYPE_CLAN_ADD, RemoveClanTag(it->second.sName));
		
		// update the displayed login username on systembar, note clan tags are removed from name because the first time data is returned
		// there are no tags in the name, the next time there are though, this ensures whether its the 1st or 2nd time changing the name 
		// it will appear correctly.
		ChatUpdateName.Trigger(_T("[") + it->second.sClanTag + _T("]") + RemoveClanTag(it->second.sName));
	}
}


/*====================
  CChatManager::HandleClanRankChanged
  ====================*/
void	CChatManager::HandleClanRankChanged(CPacket& pkt)
{
	uint uiAccountID(pkt.ReadInt());
	byte yRank(pkt.ReadByte());
	uint uiChangerID(pkt.ReadInt());

	if (yRank >= NUM_CLAN_RANKS)
		return;

	ChatClientMap_it it(m_mapUserList.find(uiAccountID));

	if (it == m_mapUserList.end())
		return;

	ChatClientMap_it changeit(m_mapUserList.find(uiChangerID));

	if (changeit == m_mapUserList.end())
		return;

	if (yRank == CLAN_RANK_NONE && uiAccountID == uiChangerID)
	{
		// User removed
		if (uiAccountID != m_uiAccountID)
		{
			// Another player in the clan other than the one leaving it sees this message
			if (m_setClanList.find(uiAccountID) != m_setClanList.end())
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_left"), _T("name"), RemoveClanTag(it->second.sName)));
				if (cc_showClanRemoveNotification)
					PushNotification(NOTIFY_TYPE_CLAN_REMOVE, RemoveClanTag(it->second.sName));
				RemoveClanMember(uiAccountID);
			}
		}
		else
		{
			// We left the clan on our own
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_left_self")));
			if (cc_showClanRemoveNotification)
				PushNotification(NOTIFY_TYPE_CLAN_REMOVE, RemoveClanTag(it->second.sName));

			m_mapUserList[m_uiAccountID].iClanID = -1;
			m_mapUserList[m_uiAccountID].sClan = TSNULL;
			m_mapUserList[m_uiAccountID].sClanTag = TSNULL;
			m_mapUserList[m_uiAccountID].sName = RemoveClanTag(m_mapUserList[m_uiAccountID].sName);

			ClearClanList();
			RefreshClanList();
			
			// update the displayed login username on systembar
			ChatUpdateName.Trigger(RemoveClanTag(it->second.sName));
		}
	}
	else if (yRank == CLAN_RANK_NONE && uiAccountID != uiChangerID)
	{
		// User removed
		if (uiAccountID != m_uiAccountID)
		{
			// Members of the clan see this message when someone is kicked
			if (m_setClanList.find(uiAccountID) != m_setClanList.end())
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_kick"), _T("name"), RemoveClanTag(it->second.sName), _T("changer"), RemoveClanTag(changeit->second.sName)));
				RemoveClanMember(uiAccountID);
			}
			// keep this out here so the player that kicked the user gets the notification too
			if (cc_showClanRemoveNotification)
				PushNotification(NOTIFY_TYPE_CLAN_REMOVE, RemoveClanTag(it->second.sName), RemoveClanTag(changeit->second.sName));
			
			it->second.iClanID = -1;
			it->second.sClan = TSNULL;
			it->second.sClanTag = TSNULL;
		}
		else
		{
			// We were kicked from the clan by someone else
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_kick_self"), _T("changer"), RemoveClanTag(changeit->second.sName)));
			if (cc_showClanRemoveNotification)			
				PushNotification(NOTIFY_TYPE_CLAN_REMOVE, RemoveClanTag(it->second.sName), RemoveClanTag(changeit->second.sName));
			
			m_mapUserList[m_uiAccountID].iClanID = -1;
			m_mapUserList[m_uiAccountID].sClan = TSNULL;
			m_mapUserList[m_uiAccountID].sClanTag = TSNULL;
			m_mapUserList[m_uiAccountID].sName = RemoveClanTag(m_mapUserList[m_uiAccountID].sName);

			ClearClanList();
			RefreshClanList();

			// update the displayed login username on systembar
			ChatUpdateName.Trigger(RemoveClanTag(it->second.sName));		
		}
	}
	else
	{
		if (m_setClanList.find(uiAccountID) != m_setClanList.end())
		{
			if (uiAccountID != m_uiAccountID)
			{ 
				// If an officer or leader demoted/promoted a member the demoted member and other clan members see this
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_rank_change"), _T("name"), RemoveClanTag(it->second.sName), _T("rank"), Translate(g_sClanRankNames[yRank]), _T("changer"), RemoveClanTag(changeit->second.sName)));
				if (cc_showClanRankNotification)					
					PushNotification(NOTIFY_TYPE_CLAN_RANK, RemoveClanTag(it->second.sName), RemoveClanTag(changeit->second.sName), Translate(g_sClanRankNames[yRank]));
			}
			else
			{ 
				// If an officer or leader demoted/promoted a member the demoted member sees this
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_rank_change_self"), _T("rank"), Translate(g_sClanRankNames[yRank]), _T("changer"), RemoveClanTag(changeit->second.sName)));
				if (cc_showClanRankNotification)
					PushNotification(NOTIFY_TYPE_CLAN_RANK, RemoveClanTag(it->second.sName), TSNULL, Translate(g_sClanRankNames[yRank]));
			}
		}
	}

	if (yRank == CLAN_RANK_NONE)
		it->second.yFlags &= ~(CHAT_CLIENT_IS_OFFICER | CHAT_CLIENT_IS_CLAN_LEADER);
	else if (yRank == CLAN_RANK_MEMBER)
		it->second.yFlags &= ~(CHAT_CLIENT_IS_OFFICER | CHAT_CLIENT_IS_CLAN_LEADER);
	else if (yRank == CLAN_RANK_OFFICER)
	{
		it->second.yFlags &= ~CHAT_CLIENT_IS_CLAN_LEADER;
		it->second.yFlags |= CHAT_CLIENT_IS_OFFICER;
	}
	else
	{
		it->second.yFlags &= ~CHAT_CLIENT_IS_OFFICER;
		it->second.yFlags |= CHAT_CLIENT_IS_CLAN_LEADER;
	}

	RefreshClanList();
}


/*====================
  CChatManager::CheckClanName
  ====================*/
void	CChatManager::CheckClanName(const tstring& sName, const tstring& sTag)
{
	if (sName.empty() || sTag.empty())
		return;

	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=clan_nameCheck");
	pHTTPRequest->AddVariable(L"name", sName);
	pHTTPRequest->AddVariable(L"tag", sTag);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_CHECK_CLAN_NAME, sName, sTag));
	m_lHTTPRequests.push_back(pNewRequest);
}


/*====================
  CChatManager::AddBuddy
  ====================*/
void	CChatManager::AddBuddy(uint uiAccountID, const tstring& sName, byte yFlags, byte yReferralFlags, const tstring& sBuddyGroup)
{
	ChatClientMap_it it(m_mapUserList.find(uiAccountID));
	byte yNewFlags(yFlags);

	if (it == m_mapUserList.end())
	{
		it = m_mapUserList.insert(ChatClientPair(uiAccountID, SChatClient())).first;
		m_mapNameToAccountID.insert(pair<wstring, uint>(LowerString(StripClanTag(sName)), uiAccountID));
	}
	else
	{
		yNewFlags |= it->second.yFlags;
	}

	it->second.sName = sName;
	it->second.uiAccountID = uiAccountID;
	it->second.yFlags = yNewFlags;
	it->second.yReferralFlags = yReferralFlags;
	it->second.sBuddyGroup = sBuddyGroup;

	if (m_setBuddyList.find(uiAccountID) == m_setBuddyList.end())
		m_setBuddyList.insert(uiAccountID);

	RefreshBuddyList();
}


/*====================
  CChatManager::AddClanMember
  ====================*/
void	CChatManager::AddClanMember(uint uiAccountID, const tstring& sName, byte yFlags, byte yReferralFlags)
{
	ChatClientMap_it it(m_mapUserList.find(uiAccountID));
	byte yNewFlags(yFlags);

	if (it == m_mapUserList.end())
	{
		it = m_mapUserList.insert(ChatClientPair(uiAccountID, SChatClient())).first;
		m_mapNameToAccountID.insert(pair<wstring, uint>(LowerString(StripClanTag(sName)), uiAccountID));
	}
	else
	{
		yNewFlags |= it->second.yFlags;
	}

	it->second.sName = sName;
	it->second.uiAccountID = uiAccountID;
	it->second.yFlags = yNewFlags;
	it->second.yReferralFlags = yReferralFlags;
	it->second.sClan = m_mapUserList[m_uiAccountID].sClan;

	if (m_setClanList.find(uiAccountID) == m_setClanList.end())
		m_setClanList.insert(uiAccountID);

	RefreshClanList();
}


/*====================
  CChatManager::AddBan
  ====================*/
void	CChatManager::AddBan(uint uiAccountID, const tstring& sName, const tstring& sReason)
{
	ChatBanMap_it it(m_mapBanList.find(uiAccountID));

	if (it == m_mapBanList.end())
	{
		it = m_mapBanList.insert(ChatBanPair(uiAccountID, SChatBanned())).first;
	}

	if (it == m_mapBanList.end())
		return;

	it->second.sName = sName;
	it->second.uiAccountID = uiAccountID;
	it->second.sReason = sReason;
}


/*====================
  CChatManager::RemoveBuddy
  ====================*/
void	CChatManager::RemoveBuddy(uint uiAccountID)
{
	uiset_it itBuddy(m_setBuddyList.find(uiAccountID));
	if (itBuddy == m_setBuddyList.end())
		return;

	m_setBuddyList.erase(itBuddy);
	RefreshBuddyList();
}


/*====================
  CChatManager::RemoveClanMember
  ====================*/
void	CChatManager::RemoveClanMember(uint uiAccountID)
{
	uiset_it findit(m_setClanList.find(uiAccountID));

	if (findit == m_setClanList.end())
		return;

	m_setClanList.erase(findit);
	
	m_mapUserList[uiAccountID].sClan = TSNULL;
	m_mapUserList[uiAccountID].iClanID = -1;

	RefreshClanList();
}


/*====================
  CChatManager::RemoveClanMember
  ====================*/
void	CChatManager::RemoveClanMember(const tstring& sName)
{
	for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
	{
		if (!CompareNames(it->second.sName, sName))
			continue;

		RemoveClanMember(it->first);
		break;
	}
}

/*====================
  CChatManager::RemoveBan
  ====================*/
void	CChatManager::RemoveBan(uint uiAccountID)
{
	ChatBanMap_it it(m_mapBanList.find(uiAccountID));

	if (it == m_mapBanList.end())
		return;

	m_mapBanList.erase(it);
}


/*====================
  CChatManager::RemoveBan
  ====================*/
void	CChatManager::RemoveBan(const tstring& sName)
{
	ChatBanMap_it it(m_mapBanList.begin());

	while (it != m_mapBanList.end() && !CompareNames(it->second.sName, sName))
		it++;

	if (it == m_mapBanList.end())
		return;

	m_mapBanList.erase(it);
}


/*====================
  CChatManager::AddIgnore
  ====================*/
void	CChatManager::AddIgnore(uint uiAccountID, const tstring& sName)
{
	ChatIgnoreMap_it it(m_mapIgnoreList.find(uiAccountID));

	if (it == m_mapIgnoreList.end())
		m_mapIgnoreList.insert(ChatIgnorePair(uiAccountID, sName));
}


/*====================
  CChatManager::RemoveIgnore
  ====================*/
void	CChatManager::RemoveIgnore(uint uiAccountID)
{
	ChatIgnoreMap_it it(m_mapIgnoreList.find(uiAccountID));

	if (it == m_mapIgnoreList.end())
		return;

	m_mapIgnoreList.erase(it);
}


/*====================
  CChatManager::RemoveIgnore
  ====================*/
void	CChatManager::RemoveIgnore(const tstring& sName)
{
	ChatIgnoreMap_it it(m_mapIgnoreList.begin());

	while (it != m_mapIgnoreList.end() && !CompareNames(it->second, sName))
		it++;

	if (it == m_mapIgnoreList.end())
		return;

	m_mapIgnoreList.erase(it);
}


/*====================
  CChatManager::RequestBuddyAdd
  ====================*/
void	CChatManager::RequestBuddyAdd(const tstring& sBuddyNickName)
{
	if (sBuddyNickName.empty())
		return;

	if (CompareNames(sBuddyNickName, m_mapUserList[m_uiAccountID].sName))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_buddy_add_self")));
		return;
	}

	if (IsBuddy(sBuddyNickName))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_buddy_add_duplicate")));
		return;
	}

	CPacket pktSend;
	pktSend << CHAT_CMD_REQUEST_BUDDY_ADD << sBuddyNickName;
	m_sockChat.SendPacket(pktSend);	

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_requesting_buddy"), _T("name"), sBuddyNickName));
}


/*====================
  CChatManager::RequestBuddyApprove
  ====================*/
void	CChatManager::RequestBuddyApprove(const tstring& sBuddyNickName)
{
	if (sBuddyNickName.empty())
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_REQUEST_BUDDY_APPROVE << sBuddyNickName;
	m_sockChat.SendPacket(pktSend);	

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_approving_buddy"), _T("name"), sBuddyNickName));
}


/*====================
  CChatManager::RequestBuddyRemove
  ====================*/
void	CChatManager::RequestBuddyRemove(uint uiAccountID)
{
	if (m_uiAccountID == uiAccountID)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_buddy_remove_self")));
		return;
	}
	
	ChatClientMap_it itClient(m_mapUserList.find(uiAccountID));
	if (itClient == m_mapUserList.end())
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_buddy_remove_not_found")));
		return;
	}

	if (!IsBuddy(uiAccountID))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_buddy_remove"), _T("target"), itClient->second.sName));
		return;
	}

	// Send a request to delete a buddy
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=remove_buddy2");
	pHTTPRequest->AddVariable(L"account_id", m_uiAccountID);
	pHTTPRequest->AddVariable(L"buddy_id", uiAccountID);
	pHTTPRequest->AddVariable(L"cookie", m_sCookie);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_DELETE_BUDDY, uiAccountID));
	pNewRequest->sTarget = itClient->second.sName;
	m_lHTTPRequests.push_back(pNewRequest);
}


/*====================
  CChatManager::RequestBanlistAdd
  ====================*/
void	CChatManager::RequestBanlistAdd(const tstring& sName, const tstring& sReason)
{
	if (sName.empty())
		return;

	if (CompareNames(sName, m_mapUserList[m_uiAccountID].sName))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_banlist_add_self")));
		return;
	}

	if (IsBanned(sName))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_banlist_add_duplicate")));
		return;
	}

	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=nick2id");
	pHTTPRequest->AddVariable(L"nickname[0]", sName);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_ADD_BANNED_NICK2ID, sName, sReason));
	m_lHTTPRequests.push_back(pNewRequest);

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_adding_banlist"), _T("name"), sName));
}


/*====================
  CChatManager::RequestBanlistRemove
  ====================*/
void	CChatManager::RequestBanlistRemove(uint uiAccountID)
{
	ChatBanMap_it it(m_mapBanList.find(uiAccountID));

	if (it == m_mapBanList.end())
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_banlist_remove_not_found")));
		return;
	}

	// Send a request to delete a ban
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=remove_banned");
	pHTTPRequest->AddVariable(L"account_id", m_uiAccountID);
	pHTTPRequest->AddVariable(L"banned_id", uiAccountID);
	pHTTPRequest->AddVariable(L"cookie", m_sCookie);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_REMOVE_BANNED, uiAccountID));
	pNewRequest->sTarget = it->second.sName;
	m_lHTTPRequests.push_back(pNewRequest);
}


/*====================
  CChatManager::RequestIgnoreAdd
  ====================*/
void	CChatManager::RequestIgnoreAdd(const tstring& sName)
{
	if (sName.empty())
		return;

	if (CompareNames(sName, m_mapUserList[m_uiAccountID].sName))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_ignore_add_self")));
		return;
	}

	if (IsIgnored(sName))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_ignore_add_duplicate")));
		return;
	}

	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester);
	pHTTPRequest->AddVariable(L"f", L"nick2id");
	pHTTPRequest->AddVariable(L"nickname[0]", sName);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_ADD_IGNORED_NICK2ID, sName));
	m_lHTTPRequests.push_back(pNewRequest);

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_adding_ignore"), _T("name"), sName));
}


/*====================
  CChatManager::RequestIgnoreRemove
  ====================*/
void	CChatManager::RequestIgnoreRemove(uint uiAccountID)
{
	ChatIgnoreMap_it it(m_mapIgnoreList.find(uiAccountID));

	if (it == m_mapIgnoreList.end())
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_ignore_remove_not_found")));
		return;
	}

	//Send a request to delete the remove
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=remove_ignored");
	pHTTPRequest->AddVariable(L"account_id", m_uiAccountID);
	pHTTPRequest->AddVariable(L"ignored_id", uiAccountID);
	pHTTPRequest->AddVariable(L"cookie", m_sCookie);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_REMOVE_IGNORED, uiAccountID));
	pNewRequest->sTarget = it->second;
	m_lHTTPRequests.push_back(pNewRequest);
}


/*====================
  CChatManager::RequestPromoteClanMember
  ====================*/
void	CChatManager::RequestPromoteClanMember(const tstring& sName)
{
	if (!(m_mapUserList[m_uiAccountID].yFlags & CHAT_CLIENT_IS_CLAN_LEADER))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_error_clan_rank")));
		return;
	}

	if (sName.empty())
		return;

	for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
	{
		if (!CompareNames(it->second.sName, sName))
			continue;

		if (CompareNoCase(it->second.sClan, m_mapUserList[m_uiAccountID].sClan) != 0)
			return;

		if (it->second.yFlags & CHAT_CLIENT_IS_OFFICER || it->second.yFlags & CHAT_CLIENT_IS_CLAN_LEADER)
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_clan_promote_cannot_promote")));
			return;
		}

		CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
		if (pHTTPRequest == NULL)
			return;

		pHTTPRequest->SetHost(Host.GetMasterServerAddress());
		pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=set_rank");
		pHTTPRequest->AddVariable(L"cookie", m_sCookie);
		pHTTPRequest->AddVariable(L"target_id", it->second.uiAccountID);
		pHTTPRequest->AddVariable(L"clan_id", m_mapUserList[m_uiAccountID].iClanID);
		pHTTPRequest->AddVariable(L"rank", L"Officer");
		pHTTPRequest->SendPostRequest();

		SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_CLAN_PROMOTE, it->first));
		pNewRequest->sTarget = it->second.sName;
		m_lHTTPRequests.push_back(pNewRequest);

		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_promoting"), _T("name"), RemoveClanTag(it->second.sName)));
		return;
	}

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_error_clan_not_found"), _T("name"), sName));	
}


/*====================
  CChatManager::RequestDemoteClanMember
  ====================*/
void	CChatManager::RequestDemoteClanMember(const tstring& sName)
{
	if (!(m_mapUserList[m_uiAccountID].yFlags & CHAT_CLIENT_IS_CLAN_LEADER))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_error_clan_rank")));
		return;
	}

	if (sName.empty())
		return;

	for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
	{
		if (!CompareNames(it->second.sName, sName))
			continue;

		if (CompareNoCase(it->second.sClan, m_mapUserList[m_uiAccountID].sClan) != 0)
			return;

		if (!(it->second.yFlags & CHAT_CLIENT_IS_OFFICER) || it->second.yFlags & CHAT_CLIENT_IS_CLAN_LEADER)
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_clan_demote_cannot_demote")));
			return;
		}

		CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
		if (pHTTPRequest == NULL)
			return;

		pHTTPRequest->SetHost(Host.GetMasterServerAddress());
		pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=set_rank");
		pHTTPRequest->AddVariable(L"cookie", m_sCookie);
		pHTTPRequest->AddVariable(L"target_id", it->second.uiAccountID);
		pHTTPRequest->AddVariable(L"clan_id", m_mapUserList[m_uiAccountID].iClanID);
		pHTTPRequest->AddVariable(L"rank", L"Member");
		pHTTPRequest->SendPostRequest();

		SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_CLAN_DEMOTE, it->first));
		pNewRequest->sTarget = it->second.sName;
		m_lHTTPRequests.push_back(pNewRequest);

		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_demoting"), _T("name"), RemoveClanTag(it->second.sName)));
		return;
	}

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_error_clan_not_found"), _T("name"), sName));	
}


/*====================
  CChatManager::RequestRemoveClanMember
  ====================*/
void	CChatManager::RequestRemoveClanMember(const tstring& sName)
{
	if (sName.empty())
		return;

	if (!(m_mapUserList[m_uiAccountID].yFlags & (CHAT_CLIENT_IS_CLAN_LEADER | CHAT_CLIENT_IS_OFFICER)) && !CompareNames(sName, m_mapUserList[m_uiAccountID].sName))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_error_clan_rank")));
		return;
	}

	for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
	{
		if (!CompareNames(it->second.sName, sName))
			continue;

		if (CompareNoCase(it->second.sClan, m_mapUserList[m_uiAccountID].sClan) != 0)
			return;

		if ((it->second.yFlags & CHAT_CLIENT_IS_OFFICER || it->second.yFlags & CHAT_CLIENT_IS_CLAN_LEADER) && !CompareNames(sName, m_mapUserList[m_uiAccountID].sName))
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_clan_remove_demote")));
			return;
		}

		CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
		if (pHTTPRequest == NULL)
			return;

		pHTTPRequest->SetHost(Host.GetMasterServerAddress());
		pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=set_rank");
		pHTTPRequest->AddVariable(L"cookie", m_sCookie);
		pHTTPRequest->AddVariable(L"target_id", it->second.uiAccountID);
		pHTTPRequest->AddVariable(L"clan_id", m_mapUserList[m_uiAccountID].iClanID);
		pHTTPRequest->AddVariable(L"rank", L"Remove");
		pHTTPRequest->SendPostRequest();

		SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_CLAN_REMOVE, it->first));
		pNewRequest->sTarget = it->second.sName;
		m_lHTTPRequests.push_back(pNewRequest);

		if (it->second.uiAccountID != m_uiAccountID)
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_removing"), _T("name"), RemoveClanTag(it->second.sName)));
		else
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_removing_self"), _T("name"), RemoveClanTag(it->second.sName)));

		return;
	}

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_error_clan_not_found"), _T("name"), sName));	
}


/*====================
  CChatManager::InviteToClan
  ====================*/
void	CChatManager::InviteToClan(const tstring& sName)
{
	if (!(m_mapUserList[m_uiAccountID].yFlags & (CHAT_CLIENT_IS_CLAN_LEADER | CHAT_CLIENT_IS_OFFICER)))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_error_clan_rank")));
		return;
	}

	if (sName.empty())
		return;

	for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
	{
		if (!CompareNames(it->second.sName, sName))
			continue;

		if (!it->second.sClan.empty())
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_clan_invite_in_clan"), _T("name"), it->second.sName));
			return;
		}

		break;
	}

	CPacket pkt;
	pkt << CHAT_CMD_CLAN_ADD_MEMBER << sName;
	m_sockChat.SendPacket(pkt);

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_invite_sent"), _T("name"), sName));	
}


/*====================
  CChatManager::CreateClan
  ====================*/
void	CChatManager::CreateClan(const tstring& sName, const tstring& sTag, const tstring& sMember1, const tstring& sMember2, const tstring& sMember3, const tstring& sMember4)
{
	ChatClientMap_it itLocalClient(m_mapUserList.find(m_uiAccountID));
	if (itLocalClient == m_mapUserList.end())
		return;

	SChatClient& localClient(itLocalClient->second);

	if (!localClient.sClan.empty())
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_fail_clan")));
		ChatClanCreateFail.Trigger(Translate(_T("chat_clan_create_result_fail_clan")));
		return;
	}

	if (sName.empty() || sTag.empty() || sMember1.empty() || sMember2.empty() || sMember3.empty() || sMember4.empty())
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_fail_param")));
		ChatClanCreateFail.Trigger(Translate(_T("chat_clan_create_result_fail_param")));
		return;
	}

	if (CompareNames(localClient.sName, sMember1) ||
		CompareNames(localClient.sName, sMember2) ||
		CompareNames(localClient.sName, sMember3) ||
		CompareNames(localClient.sName, sMember4) ||
		CompareNames(sMember1, sMember2) ||
		CompareNames(sMember1, sMember3) ||
		CompareNames(sMember1, sMember4) ||
		CompareNames(sMember2, sMember3) ||
		CompareNames(sMember2, sMember4) ||
		CompareNames(sMember3, sMember4))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_fail_duplicate")));
		ChatClanCreateFail.Trigger(Translate(_T("chat_clan_create_result_fail_duplicate")));
		return;
	}

	if (sTag.size() > 4)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_fail_tag")));
		ChatClanCreateFail.Trigger(Translate(_T("chat_clan_create_result_fail_tag")));
		return;
	}

	for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
	{
		if (!CompareNames(it->second.sName, sMember1) && !CompareNames(it->second.sName, sMember2) && !CompareNames(it->second.sName, sMember3) && !CompareNames(it->second.sName, sMember4))
			continue;

		if (!it->second.sClan.empty())
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_fail_in_clan"), _T("name"), it->second.sName));
			ChatClanCreateFail.Trigger(Translate(_T("chat_clan_create_result_fail_in_clan"), _T("name"), it->second.sName));
			return;
		}
	}

	CPacket pkt;
	pkt << CHAT_CMD_CLAN_CREATE_REQUEST << sName << sTag << sMember1 << sMember2 << sMember3 << sMember4;
	m_sockChat.SendPacket(pkt);

	m_uiCreateTimeSent = K2System.Milliseconds();

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_create_sent")));	
}


/*====================
  CChatManager::RemoveClanTag
  ====================*/
tstring	CChatManager::RemoveClanTag(const tstring& sName) const
{
	size_t uiPos(sName.find(_T("]")));

	if (uiPos != tstring::npos)
		return sName.substr(uiPos + 1);

	return sName;
}


/*====================
  CChatManager::HasClanTag
  ====================*/
bool	CChatManager::HasClanTag(const tstring& sName) const
{
	size_t uiPos(sName.find(_T("]")));

	if (uiPos != tstring::npos)
		return true;
	
	return false;
}


/*====================
  CChatManager::CompareNames
  ====================*/
bool	CChatManager::CompareNames(const tstring& sOrig, const tstring& sName)
{
	if (CompareNoCase(sOrig, sName) == 0)
		return true;

	size_t uiPos(sOrig.find(_T("]")));
	size_t uiPos2(sName.find(_T("]")));

	if (uiPos != tstring::npos && uiPos2 != tstring::npos)
		return (CompareNoCase(sOrig.substr(uiPos + 1), sName.substr(uiPos2 + 1)) == 0);
	else if (uiPos != tstring::npos)
		return (CompareNoCase(sOrig.substr(uiPos + 1), sName) == 0);
	else if (uiPos2 != tstring::npos)
		return (CompareNoCase(sOrig, sName.substr(uiPos2 + 1)) == 0);

	return false;
}

bool	CChatManager::CompareNames(uint uiAccountID, const tstring& sName)
{
	ChatClientMap_it it(m_mapUserList.find(uiAccountID));

	if (it == m_mapUserList.end())
		return false;

	return CompareNames(it->second.sName, sName);
}


/*====================
  CChatManager::GetBanList
  ====================*/
void	CChatManager::GetBanList()
{
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=ban_list");
	pHTTPRequest->AddVariable(L"account_id", m_uiAccountID);
	pHTTPRequest->AddVariable(L"cookie", m_sCookie);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_GET_BANNED, INVALID_ACCOUNT));
	m_lHTTPRequests.push_back(pNewRequest);
}


/*====================
  CChatManager::AddToRecentlyPlayed
  ====================*/
void	CChatManager::AddToRecentlyPlayed(const tstring& sName)
{
	if (CompareNames(sName, m_mapUserList[m_uiAccountID].sName))
		return;

	if (m_eStatus < CHAT_CLIENT_STATUS_JOINING_GAME)
		return;

	if (m_eStatus == CHAT_CLIENT_STATUS_JOINING_GAME)
	{
		m_setRecentlyPlayed.insert(sName);
		return;
	}

	ChatBanMap_it it(m_mapBanList.begin());

	while (it != m_mapBanList.end() && !CompareNames(it->second.sName, sName))
		it++;
	
	// If we are currently in an arranged match don't show banlist info in the match channel
	if (it != m_mapBanList.end() && !m_yArrangedType)
		AddGameChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_user_on_banlist"), _T("name"), it->second.sName, _T("reason"), it->second.sReason));

	m_pRecentlyPlayed->NewNode(_T("player"));
	m_pRecentlyPlayed->AddProperty(_T("name"), sName);
	m_pRecentlyPlayed->EndNode();

	UpdateRecentlyPlayed();
}


/*====================
  CChatManager::UpdateRecentlyPlayed
  ====================*/
void	CChatManager::UpdateRecentlyPlayed()
{
	ChatRecentlyPlayedEvent.Trigger(_T("ClearItems"));

	if (m_uiAccountID == INVALID_ACCOUNT)
		return;

	if (m_eStatus == CHAT_CLIENT_STATUS_IN_GAME && m_bMatchStarted)
		m_pRecentlyPlayed->EndNode();

	bool bContinue = m_pRecentlyPlayed->TraverseChildrenReverse();
	bool bTraversed = bContinue;
	uint uiNumTraversed(0);

	tsvector vHeader(3);
	tsvector vPlayer(2);

	while (bContinue && uiNumTraversed < 5)
	{
		if (m_pRecentlyPlayed->GetNodeName() != "match")
		{
			bContinue = m_pRecentlyPlayed->TraversePrevChild();
			continue;
		}

		uiNumTraversed++;

		vHeader[0] = m_pRecentlyPlayed->GetProperty(_T("id"));
		vHeader[1] = m_pRecentlyPlayed->GetProperty(_T("time"));
		vHeader[2] = m_pRecentlyPlayed->GetProperty(_T("name"));

		ChatRecentlyPlayedHeader.Trigger(vHeader, true);

		bool bPlayersContinue = m_pRecentlyPlayed->TraverseChildren();
		bool bPlayersEndNode = bPlayersContinue;

		while (bPlayersContinue)
		{
			if (m_pRecentlyPlayed->GetNodeName() != "player")
			{
				bPlayersContinue = m_pRecentlyPlayed->TraverseNextChild();
				continue;
			}

			vPlayer[0] = m_pRecentlyPlayed->GetProperty(_T("name"));
			vPlayer[1] = vHeader[0] + vPlayer[0];

			ChatRecentlyPlayedPlayer.Trigger(vPlayer, true);

			bPlayersContinue = m_pRecentlyPlayed->TraverseNextChild();
		}

		if (bPlayersEndNode)
			m_pRecentlyPlayed->EndNode();

		bContinue = m_pRecentlyPlayed->TraversePrevChild();
	}

	if (bTraversed)
		m_pRecentlyPlayed->EndNode();

	if (m_eStatus == CHAT_CLIENT_STATUS_IN_GAME && m_bMatchStarted)
	{
		bool bTraversed = m_pRecentlyPlayed->TraverseChildren();
		bool bFound = false;

		if (bTraversed)
		{
			bFound = (m_pRecentlyPlayed->GetProperty(_T("id")) == XtoA(m_uiMatchID));

			while (!bFound && m_pRecentlyPlayed->TraverseNextChild())
				bFound = (m_pRecentlyPlayed->GetProperty(_T("id")) == XtoA(m_uiMatchID));

			if (!bFound)
			{
				m_pRecentlyPlayed->EndNode();

				m_cDate = CDate(true);

				m_pRecentlyPlayed->NewNode(_T("match"));
				m_pRecentlyPlayed->AddProperty(_T("name"), m_sGameName);
				m_pRecentlyPlayed->AddProperty(_T("id"), XtoA(m_uiMatchID));
				m_pRecentlyPlayed->AddProperty(_T("time"), m_cDate.GetTimeString(TIME_NO_SECONDS) + _T(" ") + m_cDate.GetDateString(DATE_SHORT_YEAR | DATE_MONTH_FIRST));
			}
		}
	}
}


/*====================
  CChatManager::ShowPostGameStats
  ====================*/
void	CChatManager::ShowPostGameStats(uint uiMatchID)
{
	m_bWaitingToShowStats = true;
	m_uiShowStatsMatchID = uiMatchID;
	ChatShowPostGameStats.Trigger(TSNULL);
}


/*====================
  CChatManager::JoinGame
  ====================*/
bool	CChatManager::JoinGame(const tstring& sName, bool bPlayerSpectate, bool bStaffSpectate)
{
	if (sName.empty())
		return false;

	for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
	{
		if (!CompareNames(it->second.sName, sName))
			continue;

		if (it->second.sServerAddressPort.empty())
			break;

		Host.Connect(it->second.sServerAddressPort, false, _CTS("loading"), false, bPlayerSpectate, bStaffSpectate);
		return true;
	}

	if (bStaffSpectate)
	{
		// We don't have the target in our chat client map, ask the chatserver for the proper address
		CPacket pktSend;
		pktSend << CHAT_CMD_STAFF_JOIN_MATCH_REQUEST << sName;
		m_sockChat.SendPacket(pktSend);
		
		return true;
	}

	return false;
}


/*====================
  CChatManager::SpectatePlayer
  ====================*/
bool	CChatManager::SpectatePlayer(const tstring& sName, bool bMentor)
{
	if (sName.empty())
		return false;

	// Grab the server info
	tstring sServerInfo;
	for (ChatClientMap_cit it(m_mapUserList.begin()); it != m_mapUserList.end(); ++it)
	{
		if (!CompareNames(it->second.sName, sName))
			continue;

		sServerInfo = it->second.sServerAddressPort;
		break;
	}

	if (sServerInfo.empty())
		return false;

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_sending_player_spectate_request"), _T("name"), sName));

	tstring sCleanName(LowerString(StripClanTag(sName)));

	Host.SetPendingPlayerSpectate(sName, bMentor);
		
	CPacket pktSend;
	pktSend << CHAT_CMD_PLAYER_SPECTATE_REQUEST << byte(PLAYER_SPECTATE_REQUEST) << sCleanName << BYTE_BOOL(bMentor) << sServerInfo;
	m_sockChat.SendPacket(pktSend);

	return true;
}


/*====================
  CChatManager::HasServerInfo
  ====================*/
bool	CChatManager::HasServerInfo(const tstring& sName) const
{
	if (sName.empty())
		return false;

	for (ChatClientMap_cit it(m_mapUserList.begin()); it != m_mapUserList.end(); ++it)
	{
		if (!CompareNames(it->second.sName, sName))
			continue;

		return !it->second.sServerAddressPort.empty();
	}

	return false;
}


/*====================
  CChatManager::UpdateUserList
  ====================*/
void	CChatManager::UpdateUserList(uint uiChannelID)
{
	if (uiChannelID == -1)
		return;

	tstring sChannel(GetChannelName(uiChannelID));

	if (sChannel.empty())
		return;

	uint uiStartTime(K2System.Microseconds());

	static tsvector vParams(18);
	static tsvector vMiniParams(2);
	
	// These stay the same throughout the rest of the function
	vParams[0] = vMiniParams[0] = sChannel;	
	
	vMiniParams[1] = _T("ClearItems();");	
	ChatUserEvent.Trigger(vMiniParams);
	
	for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
	{
		if (it->second.yStatus < CHAT_CLIENT_STATUS_CONNECTED || it->second.setChannels.find(uiChannelID) == it->second.setChannels.end())
			continue;

		vParams[1] = it->second.sName;
		vParams[2] = XtoA(GetAdminLevel(uiChannelID, it->first));
		vParams[3] = XtoA(it->second.yStatus > CHAT_CLIENT_STATUS_CONNECTED, true);
		vParams[4] = XtoA((it->second.yFlags & CHAT_CLIENT_IS_PREMIUM) != 0, true);
		vParams[5] = XtoA(it->second.uiAccountID);
		vParams[6] = Host.GetChatSymbolTexturePath(it->second.uiChatSymbol);
		vParams[7] = Host.GetChatNameColorTexturePath(it->second.uiChatNameColor);
		vParams[8] = Host.GetChatNameColorString(it->second.uiChatNameColor);
		vParams[9] = Host.GetChatNameColorIngameString(it->second.uiChatNameColor);
		vParams[10] = Host.GetAccountIconTexturePath(it->second.uiAccountIcon, it->second.iAccountIconSlot, it->second.uiAccountID);
		vParams[11] = XtoA(it->second.uiSortIndex);
		vParams[12] = XtoA(Host.GetChatNameGlow(it->second.uiChatNameColor));
		vParams[13] = Host.GetChatNameGlowColorString(it->second.uiChatNameColor);
		vParams[14] = Host.GetChatNameGlowColorIngameString(it->second.uiChatNameColor);
		vParams[15] = XtoA(it->second.uiAscensionLevel);
		vParams[16] = Host.GetChatNameColorFont(it->second.uiChatNameColor);
		vParams[17] = XtoA(Host.GetChatNameBackgroundGlow(it->second.uiChatNameColor));
		ChatUserNames.Trigger(vParams);
	}

	vMiniParams[1] = _T("SortListboxSortIndex();");
	ChatUserEvent.Trigger(vMiniParams);

	if (chat_profile)
		Console << _T("UpdateUserList - ") << K2System.Microseconds() - uiStartTime << _T(" us") << newl;
}


/*====================
  CChatManager::UpdateBuddyList
  ====================*/
void	CChatManager::UpdateBuddyList()
{
	const uint uiStartTime(K2System.Microseconds());
	uint uiTotalOnline(0);

	ChatBuddyEvent.Trigger(_T("ClearItems"));

	static tsvector vParams(11);

	for (uiset_it it(m_setBuddyList.begin()), itEnd(m_setBuddyList.end()); it != itEnd; it++)
	{
		ChatClientMap_it itClient(m_mapUserList.find(*it));
		if (itClient == m_mapUserList.end())
			continue;

		vParams[0] = itClient->second.sName;
		vParams[1] = XtoA((itClient->second.yFlags & CHAT_CLIENT_IS_VERIFIED) != 0);
		vParams[2] = Host.GetChatSymbolTexturePath(itClient->second.uiChatSymbol);
		vParams[3] = Host.GetChatNameColorTexturePath(itClient->second.uiChatNameColor);
		vParams[4] = Host.GetChatNameColorString(itClient->second.uiChatNameColor);
		vParams[5] = Host.GetChatNameColorIngameString(itClient->second.uiChatNameColor);
		vParams[6] = Host.GetAccountIconTexturePath(itClient->second.uiAccountIcon, itClient->second.iAccountIconSlot, itClient->second.uiAccountID);
		vParams[7] = XtoA(Host.GetChatNameGlow(itClient->second.uiChatNameColor));
		vParams[8] = itClient->second.sBuddyGroup;
		vParams[9] = Host.GetChatNameGlowColorString(itClient->second.uiChatNameColor);
		vParams[10] = Host.GetChatNameGlowColorIngameString(itClient->second.uiChatNameColor);

		if (itClient->second.yStatus == CHAT_CLIENT_STATUS_CONNECTED)
		{
			ChatBuddyOnline.Trigger(vParams);
			uiTotalOnline++;
		}
		else if (itClient->second.yStatus > CHAT_CLIENT_STATUS_CONNECTED)
		{
			ChatBuddyGame.Trigger(vParams);
			uiTotalOnline++;
		}
		else
			ChatBuddyOffline.Trigger(vParams);
	}

	ChatBuddyEvent.Trigger(_T("SortListboxSortIndex"));
	
	ChatTotalFriends.Trigger(XtoA(INT_SIZE(m_setBuddyList.size())));
	ChatOnlineFriends.Trigger(XtoA(uiTotalOnline));

	if (chat_profile)
		Console << _T("UpdateBuddyList - ") << K2System.Microseconds() - uiStartTime << _T(" us") << newl;
}


/*====================
  CChatManager::UpdateClanList
  ====================*/
void	CChatManager::UpdateClanList()
{
	const uint uiStartTime(K2System.Microseconds());
	ChatClanEvent.Trigger(_T("ClearItems"));

	static tsvector vParams(13);

	for (uiset_it it(m_setClanList.begin()), itEnd(m_setClanList.end()); it != itEnd; it++)
	{
		ChatClientMap_it itClient(m_mapUserList.find(*it));
		if (itClient == m_mapUserList.end())
			continue;

		vParams[0] = itClient->second.sName;
		vParams[1] = XtoA((itClient->second.yFlags & CHAT_CLIENT_IS_VERIFIED) != 0);
		vParams[2] = Host.GetChatSymbolTexturePath(itClient->second.uiChatSymbol);
		vParams[3] = Host.GetChatNameColorTexturePath(itClient->second.uiChatNameColor);
		vParams[4] = Host.GetChatNameColorString(itClient->second.uiChatNameColor);
		vParams[5] = Host.GetChatNameColorIngameString(itClient->second.uiChatNameColor);
		vParams[6] = Host.GetAccountIconTexturePath(itClient->second.uiAccountIcon, itClient->second.iAccountIconSlot, itClient->second.uiAccountID);
		vParams[7] = XtoA(Host.GetChatNameGlow(itClient->second.uiChatNameColor));
		vParams[8] = Host.GetChatNameGlowColorString(itClient->second.uiChatNameColor);
		vParams[9] = Host.GetChatNameGlowColorIngameString(itClient->second.uiChatNameColor);
		vParams[10] = XtoA((itClient->second.yFlags & CHAT_CLIENT_IS_OFFICER) != 0);
		vParams[11] = XtoA((itClient->second.yFlags & CHAT_CLIENT_IS_CLAN_LEADER) != 0);
		vParams[12] = XtoA(Host.GetChatNameBackgroundGlow(itClient->second.uiChatNameColor));

		if (itClient->second.yStatus == CHAT_CLIENT_STATUS_CONNECTED)
		{
			ChatClanOnline.Trigger(vParams);
		}
		else if (itClient->second.yStatus > CHAT_CLIENT_STATUS_CONNECTED)
		{
			ChatClanGame.Trigger(vParams);
		}
		else if (itClient->second.yStatus < CHAT_CLIENT_STATUS_CONNECTED)
			ChatClanOffline.Trigger(vParams);
	}

	ChatClanEvent.Trigger(_T("SortListboxSortIndex"));

	ChatTotalClanMembers.Trigger(XtoA(INT_SIZE(m_setClanList.size())));

	if (chat_profile)
		Console << _T("UpdateClanList - ") << K2System.Microseconds() - uiStartTime << _T(" us") << newl;
}


/*====================
  CChatManager::UpdateCompanionList
  ====================*/
void	CChatManager::UpdateCompanionList()
{
	const uint uiStartTime(K2System.Microseconds());

	ChatCompanionEvent.Trigger(_CTS("true"));

	uiset setCompanions(m_setBuddyList);
#if !defined(NEWUICODE)
	setCompanions.insert(m_setClanList.begin(), m_setClanList.end());
#endif
	setCompanions.insert(m_setCafeList.begin(), m_setCafeList.end());

	static tsvector vParams(22);

	for (uiset_cit it(setCompanions.begin()), itEnd(setCompanions.end()); it != itEnd; ++it)
	{
		ChatClientMap_it itClient(m_mapUserList.find(*it));
		if (itClient == m_mapUserList.end())
			continue;

		vParams[0] = itClient->second.sName;
		vParams[1] = XtoA((itClient->second.yFlags & CHAT_CLIENT_IS_VERIFIED) != 0);
		vParams[2] = Host.GetChatSymbolTexturePath(itClient->second.uiChatSymbol);
		vParams[3] = Host.GetChatNameColorTexturePath(itClient->second.uiChatNameColor);
		vParams[4] = Host.GetChatNameColorString(itClient->second.uiChatNameColor);
		vParams[5] = Host.GetChatNameColorIngameString(itClient->second.uiChatNameColor);
		vParams[6] = Host.GetAccountIconTexturePath(itClient->second.uiAccountIcon, itClient->second.iAccountIconSlot, itClient->second.uiAccountID);
		vParams[7] = XtoA(Host.GetChatNameGlow(itClient->second.uiChatNameColor));
		vParams[8] = itClient->second.sBuddyGroup;
		vParams[9] = XtoA(itClient->second.yStatus >= CHAT_CLIENT_STATUS_CONNECTED);
		vParams[10] = XtoA(itClient->second.yStatus >= CHAT_CLIENT_STATUS_JOINING_GAME);
		vParams[11] = XtoA(m_setBuddyList.find(*it) != m_setBuddyList.end());
		
		bool bClanMember(m_setClanList.find(*it) != m_setClanList.end());
		vParams[12] = XtoA(bClanMember);
		vParams[13] = XtoA((itClient->second.yFlags & CHAT_CLIENT_IS_OFFICER) != 0);
		vParams[14] = XtoA((itClient->second.yFlags & CHAT_CLIENT_IS_CLAN_LEADER) != 0);
		vParams[15] = XtoA((itClient->second.yReferralFlags & CHAT_CLIENT_REFERRAL_NEW) != 0);
		vParams[16] = XtoA((itClient->second.yReferralFlags & CHAT_CLIENT_REFERRAL_INACTIVE) != 0);

		vParams[17] = Host.GetChatNameGlowColorString(itClient->second.uiChatNameColor);
		vParams[18] = Host.GetChatNameGlowColorIngameString(itClient->second.uiChatNameColor);
		vParams[19] = XtoA(itClient->second.uiAscensionLevel);

		bool bSameCafe(m_setCafeList.find(*it) != m_setCafeList.end());
		vParams[20] = XtoA(bSameCafe);
		vParams[21] = XtoA(Host.GetChatNameBackgroundGlow(itClient->second.uiChatNameColor));

		ChatCompanion.Trigger(vParams);
	}

	ChatCompanionEvent.Trigger(_CTS("false"));

	if (chat_profile)
		Console << _T("UpdateCompanionList - ") << K2System.Microseconds() - uiStartTime << _T(" us") << newl;
}

/*====================
  CChatManager::InviteUser
  ====================*/
void	CChatManager::InviteUser(const tstring& sName)
{
	if (GetStatus() >= CHAT_CLIENT_STATUS_JOINING_GAME)
	{
		CHostClient* pClient(Host.GetActiveClient());
		if (pClient != NULL)
		{
			pClient->InviteUser(sName);
		}
	}
	else
	{
		// TODO: Invite to current channel
	}
}


/*====================
  CChatManager::IsBuddy
  ====================*/
bool	CChatManager::IsBuddy(const tstring& sName)
{
	if (CompareNames(sName, m_mapUserList[m_uiAccountID].sName))
		return true;

	for (uiset_it it(m_setBuddyList.begin()); it != m_setBuddyList.end(); it++)
		if (CompareNames(m_mapUserList[*it].sName, sName))
			return true;

	return false;
}

/*====================
  CChatManager::IsClanMember
  ====================*/
bool	CChatManager::IsClanMember(const tstring& sName)
{
	if (CompareNames(sName, m_mapUserList[m_uiAccountID].sName))
		return true;

	for (uiset_it it(m_setClanList.begin()); it != m_setClanList.end(); it++)
		if (CompareNames(m_mapUserList[*it].sName, sName))
			return true;

	return false;
}

/*====================
  CChatManager::IsCafeMember
  ====================*/
bool	CChatManager::IsCafeMember(const tstring& sName)
{
	if (CompareNames(sName, m_mapUserList[m_uiAccountID].sName))
		return true;

	for (uiset_it it(m_setCafeList.begin()); it != m_setCafeList.end(); it++)
		if (CompareNames(m_mapUserList[*it].sName, sName))
			return true;

	return false;
}

/*====================
  CChatManager::IsBuddy
  ====================*/
bool	CChatManager::IsBuddy(uint uiAccountID) 
{
	return m_setBuddyList.find(uiAccountID) != m_setBuddyList.end();
}


/*====================
  CChatManager::IsClanMember
  ====================*/
bool	CChatManager::IsClanMember(uint uiAccountID) 
{
	return m_setClanList.find(uiAccountID) != m_setClanList.end();
}

/*====================
  CChatManager::IsCafeMember
  ====================*/
bool	CChatManager::IsCafeMember(uint uiAccountID) 
{
	return m_setCafeList.find(uiAccountID) != m_setCafeList.end();
}

/*====================
  CChatManager::IsBanned
  ====================*/
bool	CChatManager::IsBanned(uint uiAccountID)
{
	ChatBanMap_it it(m_mapBanList.find(uiAccountID));

	if (it == m_mapBanList.end())
		return false;

	return true;
}


/*====================
  CChatManager::IsBanned
  ====================*/
bool	CChatManager::IsBanned(const tstring& sName)
{
	ChatBanMap_it it(m_mapBanList.begin());

	while (it != m_mapBanList.end() && !CompareNames(it->second.sName, sName))
		it++;

	if (it == m_mapBanList.end())
		return false;

	return true;
}


/*====================
  CChatManager::IsIgnored
  ====================*/
bool	CChatManager::IsIgnored(uint uiAccountID)
{
	ChatIgnoreMap_it it(m_mapIgnoreList.find(uiAccountID));

	if (it == m_mapIgnoreList.end())
		return false;

	return true;
}

/*====================
  CChatManager::IsAccountInactive
  ====================*/
bool	CChatManager::IsAccountInactive(const tstring& sName)
{
	for (ChatClientMap_it itUser(m_mapUserList.begin()), itEnd(m_mapUserList.end()); itUser != itEnd; ++itUser)
	{
		if (CompareNames(itUser->second.sName, sName))
			return (itUser->second.yReferralFlags & CHAT_CLIENT_REFERRAL_INACTIVE) != 0;		
	}

	return false;
}

bool	CChatManager::IsAccountInactive(uint uiAccountID)
{
	ChatClientMap_it itUser(m_mapUserList.find(uiAccountID));
	if (itUser != m_mapUserList.end())
	{
		return (itUser->second.yReferralFlags & CHAT_CLIENT_REFERRAL_INACTIVE) != 0;
	}
	return false;
}


/*====================
  CChatManager::IsIgnored
  ====================*/
bool	CChatManager::IsIgnored(const tstring& sName)
{
	ChatIgnoreMap_it it(m_mapIgnoreList.begin());

	while (it != m_mapIgnoreList.end() && !CompareNames(it->second, sName))
		it++;

	if (it == m_mapIgnoreList.end())
		return false;

	return true;
}


/*====================
  CChatManager::SetChatModeType
  ====================*/
void	CChatManager::SetChatModeType(byte yChatModeType, const tstring& sReason, bool bSetDefaultMode)
{
	// they are just logging into the chat server, so default the chat mode to available
	if (bSetDefaultMode)
	{
		m_yChatModeType = yChatModeType;
		return;		
	}

	if (GetChatModeType() == CHAT_MODE_INVISIBLE)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_mode_switch_fail")));
		return;
	}
		
	CPacket pktSend;
	pktSend << CHAT_CMD_SET_CHAT_MODE_TYPE << yChatModeType << sReason.substr(0, CHAT_MESSAGE_MAX_LENGTH);
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::HasFlags
  ====================*/
bool	CChatManager::HasFlags(uint uiAccountID, byte yFlags)
{
	ChatClientMap_it it(m_mapUserList.find(uiAccountID));

	if (it == m_mapUserList.end())
		return false;

	return ((it->second.yFlags & yFlags) == yFlags);
}


/*====================
  CChatManager::HasFlags
  ====================*/
bool	CChatManager::HasFlags(const tstring& sName, byte yFlags)
{
	ChatClientMap_it it(m_mapUserList.begin());

	while (it != m_mapUserList.end())
	{
		if (CompareNames(it->second.sName, sName))
			break;

		it++;
	}

	if (it == m_mapUserList.end())
		return false;

	return ((it->second.yFlags & yFlags) == yFlags);
}

/*====================
  CChatManager::IsInAClan
  ====================*/
bool	CChatManager::IsInAClan(uint uiAccountID)
{
	ChatClientMap_it it(m_mapUserList.find(uiAccountID));

	if (it == m_mapUserList.end())
		return false;

	return HasClanTag(it->second.sName);
}

/*====================
  CChatManager::IsInAClan
  ====================*/
bool	CChatManager::IsInAClan(const tstring& sName)
{
	ChatClientMap_it it(m_mapUserList.begin());

	while (it != m_mapUserList.end())
	{
		if (CompareNames(it->second.sName, sName))
			break;

		it++;
	}

	if (it == m_mapUserList.end())
		return false;

	return HasClanTag(it->second.sName);
}


/*====================
  CChatManager::GetBanReason
  ====================*/
tstring	CChatManager::GetBanReason(uint uiAccountID)
{
	ChatBanMap_it it(m_mapBanList.find(uiAccountID));

	if (it == m_mapBanList.end())
		return TSNULL;

	return it->second.sReason;
}


/*====================
  CChatManager::GetBanReason
  ====================*/
tstring	CChatManager::GetBanReason(const tstring& sName)
{
	ChatBanMap_it it(m_mapBanList.begin());

	while (it != m_mapBanList.end() && !CompareNames(it->second.sName, sName))
		it++;

	if (it == m_mapBanList.end())
		return TSNULL;

	return it->second.sReason;
}


/*====================
  CChatManager::PlaySound
  ====================*/
void	CChatManager::PlaySound(const tstring& sSoundName)
{
	PROFILE("CChatManager::PlaySound");

	CStringTable* pSounds(g_ResourceManager.GetStringTable(m_hStringTable));

	if (pSounds == NULL)
		return;

	ResHandle hHandle(g_ResourceManager.Register(pSounds->Get(sSoundName), RES_SAMPLE));

	if (hHandle == INVALID_RESOURCE)
		return;

	K2SoundManager.Play2DSound(hHandle);
}


/*====================
  CChatManager::GetChannelName
  ====================*/
const tstring&	CChatManager::GetChannelName(uint uiChannelID)
{
	return m_mapChannels[uiChannelID].sChannelName;
}


/*====================
  CChatManager::GetChannelID
  ====================*/
uint	CChatManager::GetChannelID(const tstring& sChannel)
{
	if (sChannel.empty())
		return -1;

	for (ChatChannelMap_it it(m_mapChannels.begin()), itEnd(m_mapChannels.end()); it != itEnd; ++it)
	{
		if (CompareNames(it->second.sChannelName, sChannel))
			return it->first;
	}

	return -1;
}


/*====================
  CChatManager::IsSavedChannel
  ====================*/
bool	CChatManager::IsSavedChannel(const tstring& sChannel)
{
	if (sChannel.empty())
		return false;

	for (sset_it it(m_setAutoJoinChannels.begin()), itEnd(m_setAutoJoinChannels.end()); it != itEnd; ++it)
	{
		const tstring sChannelName(*it);
		if (CompareNoCase(sChannel, sChannelName) == 0)
			return true;
	}

	return false;
}
	
	
/*====================
  CChatManager::SaveChannelLocal
  ====================*/
void	CChatManager::SaveChannelLocal(const tstring& sChannel)
{
	if (sChannel.empty())
		return;

	// this is used to temporarily store the list of auto join channels retrieved from master server in CClientAccount::ProcessLoginResponse()
	m_setAutoJoinChannels.insert(sChannel.substr(0, CHAT_CHANNEL_MAX_LENGTH));
}


/*====================
  CChatManager::RemoveChannelLocal
  ====================*/
void	CChatManager::RemoveChannelLocal(const tstring& sChannel)
{
	if (sChannel.empty())
		return;

	// this is used to remove the given auto join channel after the master server returns the response from CChatManager::RemoveChannel()
	m_setAutoJoinChannels.erase(sChannel.substr(0, CHAT_CHANNEL_MAX_LENGTH));
}


/*====================
  CChatManager::RemoveChannelsLocal
  ====================*/
void	CChatManager::RemoveChannelsLocal()
{
	for (sset_it it(m_setAutoJoinChannels.begin()), itEnd(m_setAutoJoinChannels.end()); it != itEnd; ++it)
	{
		const tstring sChannelName(*it);
		LeaveChannel(sChannelName);
	}

	// this is used to remove all channels when a user right clicks the auto connect checkbox for the channel
	m_setAutoJoinChannels.clear();	
}


/*====================
  CChatManager::SaveChannel
  ====================*/
void	CChatManager::SaveChannel(const tstring& sChannel)
{
	if (sChannel.empty())
		return;

	// Don't save off channels with wierd % and ^ in them, or if they are group/match channels
	if (sChannel.find(L"%") != tstring::npos || sChannel.find(L"^") != tstring::npos || sChannel.substr(0, 6) == L"Group " || sChannel.substr(0, 6) == L"Match " || sChannel.substr(0, 7) == L"Stream " || sChannel.substr(0, 16) == L"Scheduled Match ")
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_invalid_channel", L"channel", sChannel));
		return;
	}
		
	// this is used to save the specified channel to the db
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=add_room");
	pHTTPRequest->AddVariable(L"account_id", m_uiAccountID);
	pHTTPRequest->AddVariable(L"chatroom_name", sChannel);
	pHTTPRequest->AddVariable(L"cookie", m_sCookie);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_SAVE_CHANNEL, sChannel));
	m_lHTTPRequests.push_back(pNewRequest);

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_saving_channel", L"channel", sChannel));
}


/*====================
  CChatManager::RemoveChannel
  ====================*/
void	CChatManager::RemoveChannel(const tstring& sChannel, bool bRemoveAll)
{
	if (sChannel.empty() && !bRemoveAll)
		return;

	// this is used to remove the specified channel from the db
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + (bRemoveAll ? "?f=clear_rooms" : "?f=remove_room"));

	if (!bRemoveAll)
	{
		pHTTPRequest->AddVariable(L"chatroom_name", sChannel);
	}
		
	pHTTPRequest->AddVariable(L"account_id", m_uiAccountID);
	pHTTPRequest->AddVariable(L"cookie", m_sCookie);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_REMOVE_CHANNEL, sChannel));
	m_lHTTPRequests.push_back(pNewRequest);

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_removing_channel"), _T("channel"), sChannel));
}


/*====================
  CChatManager::JoinChannel
  ====================*/
void	CChatManager::JoinChannel(const tstring& sChannel)
{
	if (!IsConnected() || sChannel.empty())
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_JOIN_CHANNEL << sChannel.substr(0, CHAT_CHANNEL_MAX_LENGTH);

	m_sockChat.SendPacket(pktSend);
}

void	CChatManager::JoinChannel(const tstring& sChannel, const tstring& sPassword)
{
	if (!IsConnected() || sChannel.empty() || sPassword.empty())
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_JOIN_CHANNEL_PASSWORD << sChannel.substr(0, CHAT_CHANNEL_MAX_LENGTH) << sPassword;

	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::JoinStreamChannel
  ====================*/
void	CChatManager::JoinStreamChannel(const tstring& sChannel)
{
	if (!IsConnected() || sChannel.empty())
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_JOIN_STREAM_CHANNEL << sChannel.substr(0, CHAT_CHANNEL_MAX_LENGTH);

	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::LeaveChannel
  ====================*/
void	CChatManager::LeaveChannel(const tstring& sChannel)
{
	if (!IsConnected() || sChannel.empty())
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_LEAVE_CHANNEL << sChannel.substr(0, CHAT_CHANNEL_MAX_LENGTH);

	m_sockChat.SendPacket(pktSend);

	uint uiChannelID(GetChannelID(sChannel));

	m_setChannelsIn.erase(uiChannelID);

	if (uiChannelID != -1)
		ChatLeftChannel.Trigger(XtoA(uiChannelID));
}


/*====================
  CChatManager::RequestChannelList
  ====================*/
void	CChatManager::RequestChannelList()
{
	if (!IsConnected())
		return;

	ChatChannelList.Execute(_T("ClearData();"));

	CPacket pktSend;
	pktSend << NET_CHAT_CL_GET_CHANNEL_LIST;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::RequestChannelSublist
  ====================*/
void	CChatManager::RequestChannelSublist(const tstring& sHead)
{
	if (!IsConnected())
		return;

	tstring sLowerHead(LowerString(sHead));

	// If the last finished list is a super-set of the requested list use the finished set instead
	if (m_bFinishedList && sLowerHead.compare(0, m_sFinishedListHead.length(), m_sFinishedListHead) == 0)
	{
		for (ChatChannelInfoMap_it it(m_mapChannelList.begin()); it != m_mapChannelList.end(); ++it)
		{
			if (it->second.sLowerName.compare(0, sLowerHead.length(), sLowerHead) != 0)
				continue;

			ChatAutoCompleteAdd.Trigger(it->second.sName);
		}

		return;
	}

	++m_yListStartSequence;
	
	CPacket pktSend;
	pktSend << NET_CHAT_CL_GET_CHANNEL_SUBLIST << m_yListStartSequence << sLowerHead;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::ChannelSublistCancel
  ====================*/
void	CChatManager::ChannelSublistCancel()
{
	m_mapChannelList.clear();
	m_bFinishedList = false;
	m_sFinishedListHead.clear();
	m_yListStartSequence = 0xff;
	m_yProcessingListSequence = 0xff;
	m_sProcessingListHead.clear();
}


/*====================
  CChatManager::SendChannelMessage
  ====================*/
bool	CChatManager::SendChannelMessage(const tstring& sMessage, uint uiChannelID, uint eChatMessageType)
{
	if (sMessage.empty())
		return true;

	if (!IsConnected())
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_not_connected")));
		return false;
	}

	CPacket pktSend;
	
	if (eChatMessageType == CHAT_MESSAGE_IRC)
	{
		pktSend << CHAT_CMD_CHANNEL_MSG << sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH) << uiChannelID;
	}
	/* else if (eChatMessageType == CHAT_MESSAGE_ROLL)
	{
		pktSend << CHAT_CMD_CHAT_ROLL << sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH) << uiChannelID;
	} */
	/* else if (eChatMessageType == CHAT_MESSAGE_EMOTE)
	{
		pktSend << CHAT_CMD_CHAT_EMOTE << sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH) << uiChannelID;
	} */

	m_sockChat.SendPacket(pktSend);

	// If players are muted, don't allow players to talk in channels, but allow them to talk in their own clan channel
	if (IsChatMuted())
	{
		bool bCanSendMessage(false);
		CHostClient* pClient(Host.GetActiveClient());
		if (pClient != NULL)
		{
			wstring sChannelName(GetChannelName(uiChannelID));
			wstring sClanChannelName(_T("Clan ") + pClient->GetAccount().GetClanName());

			// We found the clan name in the channel name, so it's their own clan's channel
			if (sChannelName.find(sClanChannelName) != wstring::npos)
			{
				bCanSendMessage = true;
			}
		}

		if (!bCanSendMessage)
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_muted")));
			return true;
		}
	}

	if (eChatMessageType == CHAT_MESSAGE_IRC)
	{
		if (cc_playChatSounds)
			PlaySound(_T("SentChannelMessage"));
		AddIRCChatMessage(CHAT_MESSAGE_IRC, Translate(_T("chat_channel_message_sent"), _T("name"), m_mapUserList[m_uiAccountID].sName, _T("message"), sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH)), GetChannelName(uiChannelID), true, true);
	}
	/* else if (eChatMessageType == CHAT_MESSAGE_ROLL)
	{
		AddIRCChatMessage(CHAT_MESSAGE_ROLL, sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH), GetChannelName(uiChannelID), true, true);
	} */
	else if (eChatMessageType == CHAT_MESSAGE_EMOTE)
	{
		AddIRCChatMessage(CHAT_MESSAGE_EMOTE, sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH), GetChannelName(uiChannelID), true, true);
	}

	return true;
}


/*====================
  CChatManager::SendWhisper
  ====================*/
bool	CChatManager::SendWhisper(const tstring& sTarget, const tstring& sMessage)
{
	if (sMessage.empty())
		return true;

	if (!IsConnected())
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_not_connected")));
		return false;
	}

	if (CompareNames(sTarget, m_mapUserList[m_uiAccountID].sName))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_whisper_failed_self")));
		return true;
	}

	if (GetChatModeType() == CHAT_MODE_INVISIBLE)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_invisible")));
		return true;		
	}

	if (IsChatMuted() && !IsBuddy(sTarget) && !IsClanMember(sTarget) && !IsCafeMember(sTarget))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_muted")));
		return false;
	}
	
	// the sender is currently in AFK/DND mode, automatically set their status to available so they can receive messages
	if (GetChatModeType() == CHAT_MODE_AFK || GetChatModeType() == CHAT_MODE_DND) 
		SetChatModeType(CHAT_MODE_AVAILABLE, _T("chat_command_available_message"));
		
	CPacket pktSend;
	pktSend << CHAT_CMD_WHISPER << sTarget << sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH);

	m_sockChat.SendPacket(pktSend);

	if (cc_playChatSounds)
		PlaySound(_T("SentWhisper"));
	AddIRCChatMessage(CHAT_MESSAGE_WHISPER, Translate(_T("chat_whisper_sent"), _T("target"), sTarget, _T("message"), sMessage), TSNULL, true, true);

	return true;
}


/*====================
  CChatManager::SendIM
  ====================*/
bool	CChatManager::SendIM(const tstring& sOrigTarget, const tstring& sMessage)
{
	if (sMessage.empty() || sOrigTarget.empty())
		return false;

	tstring sTarget(RemoveClanTag(sOrigTarget));

	if (LowerString(sTarget) == LowerString(RemoveClanTag(m_mapUserList[m_uiAccountID].sName)))
		return false;

	if (!IsConnected())
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_not_connected")));
		return false;
	}
	
	if (GetChatModeType() == CHAT_MODE_INVISIBLE)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_invisible")));
		return false;		
	}	

	CPacket pktSend;
	bool bFound(m_mapIMs.find(LowerString(sTarget)) != m_mapIMs.end());

	// This player has sent the other player an IM before, we should have their information
	if (bFound)
	{
		ChatClientMap_cit citChatClient(m_mapUserList.end());

		map<wstring, uint>::const_iterator citAccountID(m_mapNameToAccountID.find(sTarget));
		if (citAccountID != m_mapNameToAccountID.end())
			citChatClient = m_mapUserList.find(citAccountID->second);

		if (citChatClient != m_mapUserList.end())
		{
			// We have all their information so let the chat server know it doesn't need to send extra information to the other client by sending byte(0) at the end
			pktSend << CHAT_CMD_IM << RemoveClanTag(citChatClient->second.sName) << sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH) << byte(0);

			// The chat server will send an IM message request that once the message was delivered successfully to both player that will populate this information
			m_cDate = CDate(true);
			tstring sFinal(_T("^770[") + m_cDate.GetTimeString(TIME_NO_SECONDS) + _T("] ") + Translate(_T("chat_im_sent"), _T("name"), RemoveClanTag(m_mapUserList[m_uiAccountID].sName), _T("message"), sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH)));

			m_mapIMs[LowerString(sTarget)].push_back(sFinal);

			static tsvector vParams(3);
			vParams[0] = sTarget;
			vParams[1] = sFinal;
			vParams[2] = _T("0");	
			ChatWhisperUpdate.Trigger(vParams);
			
			PlaySound(_T("SentIM"));	
			
			ChatSentIMCount.Trigger(XtoA(AddSentIM()));
			ChatOpenIMCount.Trigger(XtoA(GetOpenIMCount()));
		}
		else
		{
			// We don't have all their information so let the chat server know by sending byte(1) at the end
			pktSend << CHAT_CMD_IM << sTarget << sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH) << byte(1);
		}
	}
	else
	{
		// We don't have all their information so let the chat server know by sending byte(1) at the end
		pktSend << CHAT_CMD_IM << sTarget << sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH) << byte(1);
	}

	m_sockChat.SendPacket(pktSend);
	return true;
}


/*====================
  CChatManager::SendClanWhisper
  ====================*/
bool	CChatManager::SendClanWhisper(const tstring& sMessage)
{
	if (sMessage.empty())
		return true;

	if (!IsConnected())
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_not_connected")));
		return false;
	}
	
	if (GetChatModeType() == CHAT_MODE_INVISIBLE)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_invisible")));
		return true;		
	}		

	CPacket pktSend;
	pktSend << CHAT_CMD_CLAN_WHISPER << sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH);

	m_sockChat.SendPacket(pktSend);

	AddIRCChatMessage(CHAT_MESSAGE_CLAN, Translate(_T("chat_clan_whisper_sent"), _T("name"), m_mapUserList[m_uiAccountID].sName, _T("message"), sMessage), TSNULL, false, true);

	tstring sIM(Translate(_T("chat_clan_im_sent"), _T("name"), m_mapUserList[m_uiAccountID].sName, _T("message"), sMessage));

	m_vClanWhispers.push_back(sIM);
	ChatClanWhisperUpdate.Trigger(sIM);
	PlaySound(_T("SentClanMessage"));

	return true;
}


/*====================
  CChatManager::UpdateWhispers
  ====================*/
void	CChatManager::UpdateWhispers(const tstring& sOrigName)
{
	const tstring sName(RemoveClanTag(sOrigName));

	IMMap_it findit(m_mapIMs.find(LowerString(sName)));

	if (findit != m_mapIMs.end())
	{
		static tsvector vParams(3);
		vParams[0] = sName;
		vParams[2] = _T("0");

		for (tsvector_it it(findit->second.begin()); it != findit->second.end(); it++)
		{
			vParams[1] = *it;
			ChatWhisperUpdate.Trigger(vParams, true);
		}
	}
}


/*====================
  CChatManager::UpdateClanWhispers
  ====================*/
void	CChatManager::UpdateClanWhispers()
{
	for (tsvector::iterator it(m_vClanWhispers.begin()); it != m_vClanWhispers.end(); it++)
		ChatClanWhisperUpdate.Trigger(*it);
}


/*====================
  CChatManager::UpdateHoverInfo
  ====================*/
bool	CChatManager::UpdateHoverInfo(const tstring& sName)
{
	if (sName.empty())
		return false;

	ChatClientMap_it itFind;
	bool bFound(false);

	for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
	{
		if (CompareNames(it->second.sName, sName))
		{
			itFind = it;
			bFound = true;
			break;
		}
	}

	if (bFound)
	{
		ChatHoverName.Trigger(itFind->second.sName);

		if (itFind->second.sClan.empty())
			ChatHoverClan.Trigger(_T("None"));
		else
			ChatHoverClan.Trigger(itFind->second.sClan);

		ChatHoverServer.Trigger(_T("None"));
		ChatHoverGameTime.Trigger(_T("N/A"));
	}

	return bFound;
}


/*====================
  CChatManager::IsUserInGame
  ====================*/
bool	CChatManager::IsUserInGame(const tstring& sName)
{
	if (sName.empty())
		return false;

	ChatClientMap_it it(m_mapUserList.begin());

	while (it != m_mapUserList.end())
	{
		if (CompareNames(it->second.sName, sName))
			break;

		it++;
	}

	if (it != m_mapUserList.end())
		return (it->second.yStatus > CHAT_CLIENT_STATUS_CONNECTED);

	return false;
}


/*====================
  CChatManager::IsUserInCurrentGame
  ====================*/
bool	CChatManager::IsUserInCurrentGame(const tstring& sName)
{
	if (sName.empty())
		return false;

	ChatClientMap_it it(m_mapUserList.begin());

	while (it != m_mapUserList.end())
	{
		if (CompareNames(it->second.sName, sName))
			break;

		it++;
	}

	if (it != m_mapUserList.end())
	{
		for (uiset_it channelit(it->second.setChannels.begin()); channelit != it->second.setChannels.end(); channelit++)
			if (m_mapChannels.find(*channelit) != m_mapChannels.end() && m_mapChannels[*channelit].uiFlags & CHAT_CHANNEL_FLAG_SERVER && m_setChannelsIn.find(*channelit) != m_setChannelsIn.end())
				return true;
	}		

	return false;
}


/*====================
  CChatManager::SaveNotes
  ====================*/
void	CChatManager::SaveNotes()
{
	CFile* pFile(FileManager.GetFile(_T("~/notes.txt"), FILE_WRITE | FILE_TEXT));

	if (pFile == NULL || !pFile->IsOpen())
		return;

	uint uiNum(0);

	while (uiNum < m_vNotes.size() && uiNum < m_vNoteTimes.size())
	{
		tstring sValue(m_vNotes[uiNum]);

		if (uiNum + 1 != m_vNotes.size())
			sValue += newl;

		pFile->WriteString(XtoA(uiNum + 1) + _T("|") + m_vNoteTimes[uiNum] + _T("|") + sValue);
		uiNum++;
	}

	pFile->Close();
	SAFE_DELETE(pFile);
}


/*====================
  CChatManager::AdminKick
  ====================*/
void	CChatManager::AdminKick(const tstring& sName, uint uiSecondsToBan)
{
	if (!IsConnected() || !IsStaff())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_ADMIN_KICK << sName << uiSecondsToBan;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::GetCurrentChatHistory
  ====================*/
tstring	CChatManager::GetCurrentChatHistory()
{
	if (m_uiHistoryPos == 0 || m_vChatHistory.size() < m_uiHistoryPos - 1)
		return TSNULL;

	return m_vChatHistory[m_uiHistoryPos - 1];
}


/*====================
  CChatManager::AcceptClanInvite
  ====================*/
void	CChatManager::AcceptClanInvite()
{
	CPacket pkt;
	pkt << CHAT_CMD_CLAN_ADD_ACCEPTED;
	m_sockChat.SendPacket(pkt);
}


/*====================
  CChatManager::RejectClanInvite
  ====================*/
void	CChatManager::RejectClanInvite()
{
	CPacket pkt;
	pkt << CHAT_CMD_CLAN_ADD_REJECTED;
	m_sockChat.SendPacket(pkt);
}


/*====================
  CChatManager::IsUserOnline
  ====================*/
bool	CChatManager::IsUserOnline(const tstring& sName)
{
	if (sName.empty())
		return false;

	ChatClientMap_it it(m_mapUserList.begin());

	while (it != m_mapUserList.end())
	{
		if (CompareNames(it->second.sName, sName))
			break;

		it++;
	}

	if (it != m_mapUserList.end())
		return (it->second.yStatus >= CHAT_CLIENT_STATUS_CONNECTED);

	return false;
}


/*====================
  CChatManager::JoiningGame
  ====================*/
void	CChatManager::JoiningGame(const tstring& sAddr)
{
	if (m_eStatus != CHAT_CLIENT_STATUS_CONNECTED)
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_JOINING_GAME << sAddr;

	m_sockChat.SendPacket(pktSend);

	m_eStatus = CHAT_CLIENT_STATUS_JOINING_GAME;

	SetCurrentChatMessage(_T(""));
	m_setRecentlyPlayed.clear();

	m_bMatchStarted = false;
	m_bWaitingToShowStats = false;
	m_uiPendingMatchTimeEnd = 0;

	UpdateChannels();
}


/*====================
  CChatManager::FinishedJoiningGame
  ====================*/
void	CChatManager::FinishedJoiningGame(const tstring& sName, uint uiMatchID, byte yArrangedType, bool bJoinChatChannel)
{
	if (m_eStatus != CHAT_CLIENT_STATUS_JOINING_GAME)
		return;

	m_uiMatchID = uiMatchID;
	m_sGameName = sName;
	m_yArrangedType = yArrangedType;

	CPacket pktSend;
	pktSend << CHAT_CMD_JOINED_GAME << sName << uiMatchID << BYTE_BOOL(bJoinChatChannel);

	m_sockChat.SendPacket(pktSend);

	m_eStatus = CHAT_CLIENT_STATUS_IN_GAME;

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_joining_game"), _T("name"), sName), TSNULL, true);

	GameMatchID.Trigger(XtoA(uiMatchID));

	UpdateChannels();
}


/*====================
  CChatManager::MatchStarted
  ====================*/
void	CChatManager::MatchStarted()
{
	if (m_eStatus != CHAT_CLIENT_STATUS_IN_GAME)
		return;

	if (m_uiMatchID == -1)
		return;

	if (m_bMatchStarted)
		return;

	bool bTraversed = m_pRecentlyPlayed->TraverseChildren();
	bool bFound = false;

	if (bTraversed)
	{
		bFound = (m_pRecentlyPlayed->GetProperty(_T("id")) == XtoA(m_uiMatchID));

		while (!bFound && m_pRecentlyPlayed->TraverseNextChild())
			bFound = (m_pRecentlyPlayed->GetProperty(_T("id")) == XtoA(m_uiMatchID));

		if (!bFound)
			m_pRecentlyPlayed->EndNode();
		else
			m_pRecentlyPlayed->DeleteNode();
	}

	m_cDate = CDate(true);

	m_pRecentlyPlayed->NewNode(_T("match"));
	m_pRecentlyPlayed->AddProperty(_T("name"), m_sGameName);
	m_pRecentlyPlayed->AddProperty(_T("id"), XtoA(m_uiMatchID));
	m_pRecentlyPlayed->AddProperty(_T("time"), m_cDate.GetTimeString(TIME_NO_SECONDS) + _T(" ") + m_cDate.GetDateString(DATE_SHORT_YEAR | DATE_MONTH_FIRST));

	for (sset_it it(m_setRecentlyPlayed.begin()); it != m_setRecentlyPlayed.end(); it++)
		AddToRecentlyPlayed(*it);

	m_setRecentlyPlayed.clear();

	m_bMatchStarted = true;
}


/*====================
  CChatManager::ResetGame
  ====================*/
void	CChatManager::ResetGame()
{
	cc_curGameChannel = _T("");
	cc_curGameChannelID = uint(-1);
}


/*====================
  CChatManager::LeaveMatchChannels
  ====================*/
void	CChatManager::LeaveMatchChannels()
{
	uiset setChannelsIn(m_setChannelsIn); // Copy
	for (uiset_it it(setChannelsIn.begin()); it != setChannelsIn.end(); ++it)
	{
		ChatChannelMap_it itFind(m_mapChannels.find(*it));
		if (itFind == m_mapChannels.end())
			continue;

		if (itFind->second.uiFlags & CHAT_CHANNEL_FLAG_SERVER)
			LeaveChannel(itFind->second.sChannelName);
	}
}


/*====================
  CChatManager::JoinGameLobby
  ====================*/
void	CChatManager::JoinGameLobby(bool bAllConnected)
{
	m_bInGameLobby = true;
	ChatNewGame.Trigger(TSNULL);
	SetFocusedChannel(uint(-2), true);
}


/*====================
  CChatManager::LeaveGameLobby
  ====================*/
void	CChatManager::LeaveGameLobby()
{
	m_bInGameLobby = false;
	ChatLeftGame.Trigger(TSNULL);
}


/*====================
  CChatManager::LeftGame
  ====================*/
void	CChatManager::LeftGame()
{
	if (m_eStatus <= CHAT_CLIENT_STATUS_CONNECTED)
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_LEFT_GAME;

	m_sockChat.SendPacket(pktSend);

	if (m_eStatus == CHAT_CLIENT_STATUS_IN_GAME && m_bMatchStarted)
	{
		m_pRecentlyPlayed->EndNode();
		m_pRecentlyPlayed->WriteFile(_T("~/recentlyplayed.xml"));
	}

	m_eStatus = CHAT_CLIENT_STATUS_CONNECTED;

	ChatClientMap_it itFind(m_mapUserList.find(m_uiAccountID));

	if (itFind != m_mapUserList.end())
		itFind->second.yStatus = byte(m_eStatus);
	
	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_left_game")), TSNULL, true);

	m_setRecentlyPlayed.clear();

	UpdateChannels();

	if (m_bMatchStarted)
		m_bMatchStarted = false;
}


/*====================
  CChatManager::SendServerInvite
  ====================*/
void	CChatManager::SendServerInvite(const tstring& sName)
{
	if (GetStatus() < CHAT_CLIENT_STATUS_JOINING_GAME)
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_INVITE_USER_NAME << sName;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::SendServerInvite
  ====================*/
void	CChatManager::SendServerInvite(int iAccountID)
{
	if (GetStatus() < CHAT_CLIENT_STATUS_JOINING_GAME)
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_INVITE_USER_ID << iAccountID;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::RejectServerInvite
  ====================*/
void	CChatManager::RejectServerInvite(int iAccountID)
{
	CPacket pktSend;
	pktSend << CHAT_CMD_INVITE_REJECTED << iAccountID;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::GetUserInfo
  ====================*/
void	CChatManager::GetUserInfo(const tstring& sName)
{
	if (sName.empty())
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_USER_INFO << sName;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::RequestUserStatus
  ====================*/
void	CChatManager::RequestUserStatus(const tstring& sName)
{
	if (sName.empty())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_GET_USER_STATUS << sName;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::GetAccountIDFromName
  ====================*/
uint	CChatManager::GetAccountIDFromName(const wstring& sName)
{
	for (ChatClientMap_it itUser(m_mapUserList.begin()), itEnd(m_mapUserList.end()); itUser != itEnd; ++itUser)
	{
		if (CompareNames(itUser->second.sName, sName))
			return itUser->second.uiAccountID;
	}

	for (ChatBanMap_it itUser(m_mapBanList.begin()), itEnd(m_mapBanList.end()); itUser != itEnd; ++itUser)
	{
		if (CompareNames(itUser->second.sName, sName))
			return itUser->second.uiAccountID;
	}
	
	for (ChatIgnoreMap_it itUser(m_mapIgnoreList.begin()), itEnd(m_mapIgnoreList.end()); itUser != itEnd; ++itUser)
	{
		if (CompareNames(itUser->second, sName))
			return itUser->first;
	}

	return INVALID_ACCOUNT;
}


/*====================
  CChatManager::GetAccountNameFromID
  ====================*/
const tstring&	CChatManager::GetAccountNameFromID(uint uiAccountID)
{
	for (ChatClientMap_it itUser(m_mapUserList.begin()), itEnd(m_mapUserList.end()); itUser != itEnd; ++itUser)
	{
		if (itUser->second.uiAccountID == uiAccountID)
			return itUser->second.sName;
	}

	for (ChatBanMap_it itUser(m_mapBanList.begin()), itEnd(m_mapBanList.end()); itUser != itEnd; ++itUser)
	{
		if (itUser->second.uiAccountID == uiAccountID)
			return itUser->second.sName;
	}
	
	for (ChatIgnoreMap_it itUser(m_mapIgnoreList.begin()), itEnd(m_mapIgnoreList.end()); itUser != itEnd; ++itUser)
	{
		if (itUser->first == uiAccountID)
			return itUser->second;
	}

	return TSNULL;
}


/*====================
  CChatManager::UpdateChannels
  ====================*/
void	CChatManager::UpdateChannels()
{
	if (chat_debugInterface)
		Console.UI << _T("UpdateChannels") << newl;

	if (m_bInGameLobby)
		ChatNewGame.Trigger(TSNULL);
	
	static tsvector vParams(18);
	static tsvector vMiniParams(2);	

	vMiniParams[0] = _T("irc_status_chan");
	vMiniParams[1] = K2System.GetGameName() + _T(" - v") + K2_Version(K2System.GetVersionString());
	ChatChanTopic.Trigger(vMiniParams);

	for (uiset_it it(m_setChannelsIn.begin()); it != m_setChannelsIn.end(); it++)
	{
		if (m_mapChannels[*it].uiFlags & CHAT_CHANNEL_FLAG_HIDDEN)
			continue;

		ChatChannelMap_it itChannel(m_mapChannels.find(*it));

		if (itChannel == m_mapChannels.end())
			continue;

		vMiniParams[0] = XtoA(*it);
		vMiniParams[1] = itChannel->second.sChannelName;
		ChatNewChannel.Trigger(vMiniParams, true);

		if (chat_debugInterface)
			Console.UI << _T("UpdateChannels - ChatNewChannel ") << *it << _T(" ") << QuoteStr(GetChannelName(*it)) << newl;

		vMiniParams[0] = itChannel->second.sChannelName;
		vMiniParams[1] = itChannel->second.sTopic;
		ChatChanTopic.Trigger(vMiniParams);

		vMiniParams[0] = itChannel->second.sChannelName;
		vMiniParams[1] = XtoA(itChannel->second.uiUserCount);
		ChatChanNumUsers.Trigger(vMiniParams);
	}

	vMiniParams[0] = TSNULL;
	vMiniParams[1] = _T("ClearItems();");
	ChatUserEvent.Trigger(vMiniParams);			
		
	for (ChatClientMap_it userit(m_mapUserList.begin()); userit != m_mapUserList.end(); ++userit)
	{
		if (userit->second.yStatus < CHAT_CLIENT_STATUS_CONNECTED)
			continue;

		vParams[1] = userit->second.sName;

		for (uiset_it it(userit->second.setChannels.begin()); it != userit->second.setChannels.end(); ++it)
		{
			if (m_mapChannels[*it].uiFlags & CHAT_CHANNEL_FLAG_HIDDEN)
				continue;

			if (GetChannelName(*it).empty())
				continue;

			vParams[0] = GetChannelName(*it);
			vParams[2] = XtoA(GetAdminLevel(*it, userit->first));
			vParams[3] = XtoA(userit->second.yStatus > CHAT_CLIENT_STATUS_CONNECTED, true);
			vParams[4] = XtoA((userit->second.yFlags & CHAT_CLIENT_IS_PREMIUM) != 0, true);
			vParams[5] = XtoA(userit->second.uiAccountID);
			vParams[6] = Host.GetChatSymbolTexturePath(userit->second.uiChatSymbol);
			vParams[7] = Host.GetChatNameColorTexturePath(userit->second.uiChatNameColor);
			vParams[8] = Host.GetChatNameColorString(userit->second.uiChatNameColor);
			vParams[9] = Host.GetChatNameColorIngameString(userit->second.uiChatNameColor);
			vParams[10] = Host.GetAccountIconTexturePath(userit->second.uiAccountIcon, userit->second.iAccountIconSlot, userit->second.uiAccountID);
			vParams[11] = XtoA(userit->second.uiSortIndex);
			vParams[12] = XtoA(Host.GetChatNameGlow(userit->second.uiChatNameColor));
			vParams[13] = Host.GetChatNameGlowColorString(userit->second.uiChatNameColor);
			vParams[14] = Host.GetChatNameGlowColorIngameString(userit->second.uiChatNameColor);
			vParams[15] = XtoA(userit->second.uiAscensionLevel);
			vParams[16] = Host.GetChatNameColorFont(userit->second.uiChatNameColor);
			vParams[17] = XtoA(Host.GetChatNameBackgroundGlow(userit->second.uiChatNameColor));
			ChatUserNames.Trigger(vParams);
		}
	}

	vMiniParams[0] = TSNULL;
	vMiniParams[1] = _T("SortListboxSortIndex();");
	ChatUserEvent.Trigger(vMiniParams);
}


/*====================
  CChatManager::UpdateChannel
  ====================*/
void	CChatManager::UpdateChannel(uint uiChannelID)
{
	if (chat_debugInterface)
		Console.UI << _T("UpdateChannel") << newl;

	ChatChannelMap_it it(m_mapChannels.find(uiChannelID));

	if (it == m_mapChannels.end())
		return;

	if (it->second.uiFlags & CHAT_CHANNEL_FLAG_HIDDEN)
		return;

	static tsvector vParams(18);
	static tsvector vMiniParams(2);
	
	// These stay the same throughout the rest of the function
	vParams[0] = vMiniParams[0] = it->second.sChannelName;	
	
	vMiniParams[1] = _T("ClearItems();");

	if (!chat_debugInterface2)
	{
		ChatUserEvent.Trigger(vMiniParams);
		
		for (ChatClientMap_it userit(m_mapUserList.begin()); userit != m_mapUserList.end(); userit++)
		{
			if (userit->second.yStatus < CHAT_CLIENT_STATUS_CONNECTED || userit->second.setChannels.find(uiChannelID) == userit->second.setChannels.end())
				continue;

			vParams[1] = userit->second.sName;
			vParams[2] = XtoA(GetAdminLevel(uiChannelID, userit->first));
			vParams[3] = XtoA(userit->second.yStatus > CHAT_CLIENT_STATUS_CONNECTED, true);
			vParams[4] = XtoA((userit->second.yFlags & CHAT_CLIENT_IS_PREMIUM) != 0, true);
			vParams[5] = XtoA(userit->second.uiAccountID);
			vParams[6] = Host.GetChatSymbolTexturePath(userit->second.uiChatSymbol);
			vParams[7] = Host.GetChatNameColorTexturePath(userit->second.uiChatNameColor);
			vParams[8] = Host.GetChatNameColorString(userit->second.uiChatNameColor);
			vParams[9] = Host.GetChatNameColorIngameString(userit->second.uiChatNameColor);
			vParams[10] = Host.GetAccountIconTexturePath(userit->second.uiAccountIcon, userit->second.iAccountIconSlot, userit->second.uiAccountID);
			vParams[11] = XtoA(userit->second.uiSortIndex);
			vParams[12] = XtoA(Host.GetChatNameGlow(userit->second.uiChatNameColor));
			vParams[13] = Host.GetChatNameGlowColorString(userit->second.uiChatNameColor);
			vParams[14] = Host.GetChatNameGlowColorIngameString(userit->second.uiChatNameColor);
			vParams[15] = XtoA(userit->second.uiAscensionLevel);
			vParams[16] = Host.GetChatNameColorFont(userit->second.uiChatNameColor);
			vParams[17] = XtoA(Host.GetChatNameBackgroundGlow(userit->second.uiChatNameColor));
			ChatUserNames.Trigger(vParams);
		}

		vMiniParams[1] = it->second.sTopic;
		ChatChanTopic.Trigger(vMiniParams);

		vMiniParams[1] = XtoA(it->second.uiUserCount);
		ChatChanNumUsers.Trigger(vMiniParams);

		vMiniParams[1] = _T("SortListboxSortIndex();");
		ChatUserEvent.Trigger(vMiniParams);
	}
}


/*====================
  CChatManager::RebuildChannels
  ====================*/
void	CChatManager::RebuildChannels()
{
	UpdateChannels();

	uint uiFocusedChannel(m_uiFocusedChannel);

	m_uiFocusedChannel = -9001;

	SetFocusedChannel(uiFocusedChannel);
}


/*====================
  CChatManager::Translate
  ====================*/
tstring	CChatManager::Translate(const tstring& sKey, const tstring& sParamName1, const tstring& sParamValue1, const tstring& sParamName2, const tstring& sParamValue2, const tstring& sParamName3, const tstring& sParamValue3, const tstring& sParamName4, const tstring& sParamValue4)
{
	CHostClient* pClient(Host.GetActiveClient());

	if (pClient == NULL)
		return TSNULL;

	tsmapts mapParams;

	if (sParamName1 != TSNULL)
		mapParams[sParamName1] = sParamValue1;

	if (sParamName2 != TSNULL)
		mapParams[sParamName2] = sParamValue2;

	if (sParamName3 != TSNULL)
		mapParams[sParamName3] = sParamValue3;

	if (sParamName4 != TSNULL)
		mapParams[sParamName4] = sParamValue4;
		
	return pClient->Translate(sKey, mapParams);
}

tstring	CChatManager::Translate(const tstring& sKey, const tsmapts& mapParams)
{
	CHostClient* pClient(Host.GetActiveClient());

	if (pClient == NULL)
		return TSNULL;

	return pClient->Translate(sKey, mapParams);
}


/*====================
  CChatManager::AutoCompleteNick
  ====================*/
void	CChatManager::AutoCompleteNick(const tstring& sName)
{
	ChatAutoCompleteClear.Trigger(TSNULL);

	if (sName.length() < 5)
		return;

	// Cancel any current auto-complete requests
	for (ChatRequestList_it it(m_lHTTPRequests.begin()); it != m_lHTTPRequests.end();)
	{
		if ((*it)->eType != REQUEST_COMPLETE_NICK)
		{
			++it;
			continue;
		}

		m_pHTTPManager->KillRequest((*it)->pRequest);
		K2_DELETE(*it);
		STL_ERASE(m_lHTTPRequests, it);
	}


	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=autocompleteNicks");
	pHTTPRequest->AddVariable(L"nickname", sName);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_COMPLETE_NICK, sName));

	m_lHTTPRequests.push_back(pNewRequest);
}


/*====================
  CChatManager::AutoCompleteClear
  ====================*/
void	CChatManager::AutoCompleteClear()
{
	ChatAutoCompleteClear.Trigger(TSNULL);
}


/*====================
  CChatManager::AddWidgetReference
  ====================*/
void	CChatManager::AddWidgetReference(CTextBuffer* pBuffer, bool bIsGameChat, tstring sChannelName)
{
	m_vWidgets.push_back(SChatWidget(pBuffer, bIsGameChat, sChannelName));
}


/*====================
  CChannelManager::IsAdmin
  ====================*/
bool	CChatManager::IsAdmin(uint uiChannelID, uint uiAccountID, EAdminLevel eMinLevel)
{
	ChatChannelMap_it findit(m_mapChannels.find(uiChannelID));

	if (findit == m_mapChannels.end())
		return false;

	ChatAdminMap_it it(findit->second.mapAdmins.find(uiAccountID));

	if (it == findit->second.mapAdmins.end())
		return false;

	if (it->second < eMinLevel)
		return false;

	return true;
}


/*====================
  CChannelManager::IsAdmin
  ====================*/
bool	CChatManager::IsAdmin(uint uiChannelID, const tstring& sName, EAdminLevel eMinLevel)
{
	ChatChannelMap_it findit(m_mapChannels.find(uiChannelID));

	if (findit == m_mapChannels.end())
		return false;

	ChatClientMap_it it(m_mapUserList.begin());

	while (it != m_mapUserList.end())
	{
		if (CompareNames(it->second.sName, sName))
			break;

		it++;
	}

	if (it == m_mapUserList.end())
		return false;

	ChatAdminMap_it adminit(findit->second.mapAdmins.find(it->second.uiAccountID));

	if (adminit == findit->second.mapAdmins.end())
		return false;

	if (adminit->second < eMinLevel)
		return false;

	return true;
}


/*====================
  CChannelManager::GetAdminLevel
  ====================*/
EAdminLevel	CChatManager::GetAdminLevel(uint uiChannelID, const tstring& sName)
{
	ChatChannelMap_it findit(m_mapChannels.find(uiChannelID));

	if (findit == m_mapChannels.end())
		return CHAT_CLIENT_ADMIN_NONE;

	ChatClientMap_it it(m_mapUserList.begin());

	while (it != m_mapUserList.end())
	{
		if (CompareNames(it->second.sName, sName))
			break;

		it++;
	}

	if (it == m_mapUserList.end())
		return CHAT_CLIENT_ADMIN_NONE;

	ChatAdminMap_it adminit(findit->second.mapAdmins.find(it->second.uiAccountID));

	if (adminit == findit->second.mapAdmins.end())
		return CHAT_CLIENT_ADMIN_NONE;

	return EAdminLevel(adminit->second);
}


/*====================
  CChannelManager::GetAdminLevel
  ====================*/
EAdminLevel	CChatManager::GetAdminLevel(uint uiChannelID, uint uiAccountID)
{
	ChatChannelMap_it findit(m_mapChannels.find(uiChannelID));

	if (findit == m_mapChannels.end())
		return CHAT_CLIENT_ADMIN_NONE;

	ChatAdminMap_it it(findit->second.mapAdmins.find(uiAccountID));

	if (it == findit->second.mapAdmins.end())
		return CHAT_CLIENT_ADMIN_NONE;

	return EAdminLevel(it->second);
}


/*====================
  CChatManager::AddUnreadChannel
  ====================*/
void	CChatManager::AddUnreadChannel(uint uiChannelID)
{
	if (m_uiFocusedChannel == uiChannelID)
		return;

	uint uiNumUnread(0);

	m_mapChannels[uiChannelID].bUnread = true;

	tsvector vParams(2);
	vParams[0] = m_mapChannels[uiChannelID].sChannelName;
	vParams[1] = XtoA(true, true);
	ChatUnreadChannel.Trigger(vParams);

	for (uiset_it it(m_setChannelsIn.begin()); it != m_setChannelsIn.end(); it++)
	{
		if (!m_mapChannels[*it].bUnread)
			continue;

		uiNumUnread++;
	}

	ChatNumUnreadChannels.Trigger(XtoA(uiNumUnread));
}


/*====================
  CChatManager::RemoveUnreadChannel
  ====================*/
void	CChatManager::RemoveUnreadChannel(uint uiChannelID)
{
	uint uiNumUnread(0);

	m_mapChannels[uiChannelID].bUnread = false;

	tsvector vParams(2);
	vParams[0] = m_mapChannels[uiChannelID].sChannelName;
	vParams[1] = XtoA(false, true);
	ChatUnreadChannel.Trigger(vParams);

	for (uiset_it it(m_setChannelsIn.begin()); it != m_setChannelsIn.end(); it++)
	{
		if (!m_mapChannels[*it].bUnread)
			continue;

		uiNumUnread++;
	}

	ChatNumUnreadChannels.Trigger(XtoA(uiNumUnread));
}


/*====================
  CChatManager::SetFocusedChannel
  ====================*/
void	CChatManager::SetFocusedChannel(const tstring& sChannel, bool bForceFocus)
{
	SetFocusedChannel(GetChannelID(sChannel), bForceFocus);
}


/*====================
  CChatManager::SetFocusedChannel
  ====================*/
void	CChatManager::SetFocusedChannel(uint uiChannel, bool bForceFocus)
{
	// when logging in in invisible mode, the default uiChannel is going to be the same as the m_uiFocusedChannel
	// bypass the return here and force the UI to update the channel to show the user is in the "Status" channel when logging in	
	if (bForceFocus)
	{
		tsvector vParam(2);
		vParam[0] = XtoA((int)uiChannel);
		vParam[1] = GetChannelName(uiChannel);
		ChatSetFocusChannel.Trigger(vParam);
		return;	
	}	
	
	if (uiChannel == m_uiFocusedChannel)
		return;

	uint uiOldFocus(m_uiFocusedChannel);

	m_uiFocusedChannel = uiChannel;

	if (m_uiFocusedChannel != -1)
		RemoveUnreadChannel(m_uiFocusedChannel);

	tsvector vParam(2);
	vParam[0] = XtoA((int)uiChannel);
	vParam[1] = GetChannelName(uiChannel);
	ChatSetFocusChannel.Trigger(vParam);

	if (uiOldFocus != -1)
		RemoveUnreadChannel(uiOldFocus);

	if (chat_debugInterface)
		Console.UI << _T("SetFocusedChannel ") << uiChannel << _T(" ") << QuoteStr(GetChannelName(uiChannel)) << newl;

	ChatChannelMap_it itFind(m_mapChannels.find(uiChannel));
	if (itFind != m_mapChannels.end())
		itFind->second.uiFocusPriority = m_uiFocusCount++;
}


/*====================
  CChatManager::SetNextFocusedChannel
  ====================*/
void	CChatManager::SetNextFocusedChannel()
{
	uint uiMaxFocusPriority(0);
	uint uiMaxChannelID(uint(-1));

	for (uiset_it it(m_setChannelsIn.begin()); it != m_setChannelsIn.end(); ++it)
	{
		ChatChannelMap_it itFind(m_mapChannels.find(*it));
		if (itFind == m_mapChannels.end())
			continue;

		if (itFind->second.uiFocusPriority >= uiMaxFocusPriority)
		{
			uiMaxChannelID = *it;
			uiMaxFocusPriority = itFind->second.uiFocusPriority;
		}
	}

	SetFocusedChannel(uiMaxChannelID);
}


/*====================
  CChatManager::SetFocusedIM
  ====================*/
void	CChatManager::SetFocusedIM(const tstring& sName)
{
	if (CompareNoCase(sName, m_sFocusedIM) == 0)
		return;

	if (chat_debugInterface)
		Console.UI << _T("SetFocusedIM - ") << sName << newl;

	tsvector vParams(2);
	vParams[0] = sName;
	vParams[1] = m_sFocusedIM;

	m_sFocusedIM = sName;
	ChatFocusedIM.Trigger(vParams);

	if (!sName.empty())
		m_mapIMFocusPriority[sName] = m_uiFocusCount++;
}


/*====================
  CChatManager::SetNextFocusedIM
  ====================*/
void	CChatManager::SetNextFocusedIM()
{
	uint uiMaxFocusPriority(0);
	tstring sMaxFocusedIM;

	for (IMCountMap_it it(m_mapIMFocusPriority.begin()); it != m_mapIMFocusPriority.end(); ++it)
	{
		if (it->second >= uiMaxFocusPriority)
		{
			sMaxFocusedIM = it->first;
			uiMaxFocusPriority = it->second;
		}
	}

	if (sMaxFocusedIM.empty())
		return;

	SetFocusedIM(sMaxFocusedIM);
}


/*====================
  CChatManager::CloseIM
  ====================*/
void	CChatManager::CloseIM(const tstring& sName)
{
	IMCountMap_it it(m_mapIMFocusPriority.find(sName));
	if (it != m_mapIMFocusPriority.end())
		m_mapIMFocusPriority.erase(it);

	ChatCloseIM.Trigger(sName);

	if (sName == m_sFocusedIM)
		m_sFocusedIM.clear();
}


/*====================
  CChatManager::SetChannelTopic
  ====================*/
void	CChatManager::SetChannelTopic(uint uiChannelID, const tstring& sTopic)
{
	// limit the topic length to avoid some channel name + topic UI problems
	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_TOPIC << uiChannelID << sTopic.substr(0, CHAT_CHANNEL_TOPIC_MAX_LENGTH);
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::KickUserFromChannel
  ====================*/
void	CChatManager::KickUserFromChannel(uint uiChannelID, const tstring& sName)
{
	ChatClientMap_it it(m_mapUserList.begin());

	while (it != m_mapUserList.end())
	{
		if (CompareNames(it->second.sName, sName))
			break;

		it++;
	}

	if (it == m_mapUserList.end())
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_KICK << uiChannelID << it->first;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::BanUserFromChannel
  ====================*/
void	CChatManager::BanUserFromChannel(uint uiChannelID, const tstring& sName)
{
	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_BAN << uiChannelID << sName;
	m_sockChat.SendPacket(pktSend);
}

/*====================
  CChatManager::UnbanUserFromChannel
  ====================*/
void	CChatManager::UnbanUserFromChannel(uint uiChannelID, const tstring& sName)
{
	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_UNBAN << uiChannelID << sName;
	m_sockChat.SendPacket(pktSend);
}

/*====================
  CChatManager::SilenceChannelUser
  ====================*/
void	CChatManager::SilenceChannelUser(uint uiChannelID, const tstring& sName, uint uiDuration)
{
	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_SILENCE_USER << uiChannelID << sName << uiDuration;
	m_sockChat.SendPacket(pktSend);
}

/*====================
  CChatManager::PromoteUserInChannel
  ====================*/
void	CChatManager::PromoteUserInChannel(uint uiChannelID, uint uiAccountID)
{
	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_PROMOTE << uiChannelID << uiAccountID;
	m_sockChat.SendPacket(pktSend);
}

/*====================
  CChatManager::PromoteUserInChannel
  ====================*/
void	CChatManager::PromoteUserInChannel(uint uiChannelID, const tstring& sName)
{
	uint uiAccountID(-1);

	for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
	{
		if (!CompareNames(it->second.sName, sName))
			continue;

		uiAccountID = it->first;
		break;
	}

	if (uiAccountID == -1)
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_PROMOTE << uiChannelID << uiAccountID;
	m_sockChat.SendPacket(pktSend);
}

/*====================
  CChatManager::DemoteUserInChannel
  ====================*/
void	CChatManager::DemoteUserInChannel(uint uiChannelID, uint uiAccountID)
{
	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_DEMOTE << uiChannelID << uiAccountID;
	m_sockChat.SendPacket(pktSend);
}

/*====================
  CChatManager::DemoteUserInChannel
  ====================*/
void	CChatManager::DemoteUserInChannel(uint uiChannelID, const tstring& sName)
{
	uint uiAccountID(-1);

	for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
	{
		if (!CompareNames(it->second.sName, sName))
			continue;

		uiAccountID = it->first;
		break;
	}

	if (uiAccountID == -1)
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_DEMOTE << uiChannelID << uiAccountID;
	m_sockChat.SendPacket(pktSend);
}

/*====================
  CChatManager::RequestAuthEnable
  ====================*/
void	CChatManager::RequestAuthEnable(uint uiChannelID)
{
	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_SET_AUTH << uiChannelID;
	m_sockChat.SendPacket(pktSend);
}

/*====================
  CChatManager::RequestAuthDisable
  ====================*/
void	CChatManager::RequestAuthDisable(uint uiChannelID)
{
	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_REMOVE_AUTH << uiChannelID;
	m_sockChat.SendPacket(pktSend);
}

/*====================
  CChatManager::RequestAuthAdd
  ====================*/
void	CChatManager::RequestAuthAdd(uint uiChannelID, const tstring& sName)
{
	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_ADD_AUTH_USER << uiChannelID << sName;
	m_sockChat.SendPacket(pktSend);
}

/*====================
  CChatManager::RequestAuthRemove
  ====================*/
void	CChatManager::RequestAuthRemove(uint uiChannelID, const tstring& sName)
{
	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_REMOVE_AUTH_USER << uiChannelID << sName;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::RequestAuthList
  ====================*/
void	CChatManager::RequestAuthList(uint uiChannelID)
{
	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_LIST_AUTH << uiChannelID;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::SetChannelPassword
  ====================*/
void	CChatManager::SetChannelPassword(uint uiChannelID, const tstring& sPassword)
{
	CPacket pktSend;
	pktSend << CHAT_CMD_CHANNEL_SET_PASSWORD << uiChannelID << sPassword;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::SendGlobalMessage
  ====================*/
void	CChatManager::SendGlobalMessage(byte yType, uint uiValue, const tstring& sMessage)
{
	CPacket pktSend;
	pktSend << CHAT_CMD_MESSAGE_ALL << yType << uiValue << sMessage;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::IsInChannel
  ====================*/
bool	CChatManager::IsInChannel(uint uiChannelID)
{
	return m_setChannelsIn.find(uiChannelID) != m_setChannelsIn.end();
}


/*====================
  CChatManager::InitCensor
  ====================*/
void	CChatManager::InitCensor()
{
	tstring sPath(L"/curse_words_");
	sPath += host_language + L".txt";
	int iFileOptions(FILE_TEXT | FILE_READ | FILE_UTF8);
	CFileHandle curseFile(sPath, iFileOptions);

	if (!curseFile.IsOpen() && host_language != L"en")
	{
		sPath.clear();
		sPath = L"/curse_words_en.txt";
		curseFile.Open(sPath, iFileOptions);
	}

	if (curseFile.IsOpen())
	{
		while(!curseFile.IsEOF())
		{
			tstring curseWord(curseFile.ReadLine());
			if ( !curseWord.empty())
			{
				m_mapCensor.insert(pair<tstring, tstring>(curseWord, _T("****")));
			}
		}
		curseFile.Close();
	}
	else
	{
		Console << "Failed to load a curse_word text file.\n" ;
	}
}


/*====================
  CChatManager::CensorChat

  TODO: Make this function more thorough!
  ====================*/
bool	CChatManager::CensorChat(tstring& sMessage, bool bInGameChat)
{
	bool bCensored(false);
	bool bFound(true);
	tstring::size_type pos(0);
	tstring sStartingChars;
	
	// Strip off everything before the space - ""^444PlayerName:^* "
	if (!bInGameChat)
	{
		pos = sMessage.find_first_of(_T(" "));
		if (pos != tstring::npos)
		{
			sStartingChars = sMessage.substr(0, pos);
			sMessage = sMessage.substr(pos);
		}
		
		pos = 0;
	}
	
	// TODO: We can't strip color codes then use the index from the stripped version in the unstripped.
	//		This needs to do a search ignoring color codes instead.
	//tstring sLower(StripColorCodes(LowerString(sMessage)));	
	
	while (bFound)
	{
		bFound = false;

		for (tsmapts::reverse_iterator it(m_mapCensor.rbegin()); it != m_mapCensor.rend(); it++)
		{
			pos = sMessage.find(it->first);

			if (pos != tstring::npos)
			{
				// Only censor if it is not part of a larger word
				if ((pos == 0 || sMessage[pos - 1] == _T(' ') || (IsNotDigit(sMessage[pos - 1]) && !IsLetter(sMessage[pos - 1]))) &&
					(pos + it->first.length() == sMessage.length() || sMessage[pos + it->first.length()] == _T(' ') || 
					(IsNotDigit(sMessage[pos + it->first.length()]) && !IsLetter(sMessage[pos + it->first.length()]))))
				{
					//sLower.erase(pos, it->first.length());
					sMessage.erase(pos, it->first.length());

					//sLower.insert(pos, it->second);
					sMessage.insert(pos, it->second);

					bFound = true;
					bCensored = true;
				}
			}
		}
	}
	
	// Re-add ""^444PlayerName:^* " back to the beginning
	if (!bInGameChat)
	{
		sMessage = sStartingChars + sMessage;
	}

	return bCensored;
}


/*====================
  CChatManager::AddUnreadIM
  ====================*/
uint	CChatManager::AddUnreadIM(const tstring& sName)
{
	ChatUnreadIM.Trigger(sName);

	IMCountMap_it it(m_mapIMUnreadCount.find(sName));
	if (it != m_mapIMUnreadCount.end())
	{
		++it->second;
		return it->second;
	}
	else
	{
		m_mapIMUnreadCount[sName] = 1;
		return 1;
	}
}


/*====================
  CChatManager::RemoveUnreadIMs
  ====================*/
uint	CChatManager::RemoveUnreadIMs(const tstring& sName)
{	
	IMCountMap_it it(m_mapIMUnreadCount.find(sName));
	if (it != m_mapIMUnreadCount.end())
	{
		uint uiUnread(it->second);
		STL_ERASE(m_mapIMUnreadCount, it);

		return uiUnread;
	}
	else
	{
		return 0;
	}
}


/*====================
  CChatManager::IsFollowing
  ====================*/
bool	CChatManager::IsFollowing(const tstring& sName)
{
	if (m_bFollow && m_sFollowName == RemoveClanTag(sName))
		return true;
		
	return false;
}


/*====================
  CChatManager::RequestRefreshUpgrades
  ====================*/
void	CChatManager::RequestRefreshUpgrades()
{
	CPacket pktSend;
	pktSend << NET_CHAT_CL_REFRESH_UPGRADES;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::SubmitChatMessage

  PLEASE NOTE: I know this function is messy. It was copied directly from
  CIRCManager and modified slightly to retain functionality. It will be
  cleaned up and restructured shortly. *FIX ME*
  ====================*/
bool	CChatManager::SubmitChatMessage(const tstring& sMessage, uint uiChannelID)
{
	if (!IsConnected())
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_not_connected")));
		return false;
	}

	//Note that the commands can all be concatinated
	//when they come to us, so we have to tokenize them
	//again... Hence the seemingly useless line.
	tsvector vsTokens(TokenizeString(sMessage, ' '));

	if (vsTokens.size() < 1)
	{
		m_vChatHistory.push_front(sMessage);
		m_uiHistoryPos = 0;
		return true;
	}

	//If they want to whisper, do so
	if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_whisper"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_whisper_short"))) == 0)
	{
		if (vsTokens.size() >= 3 && !vsTokens[1].empty())
		{
			m_vChatHistory.push_front(sMessage);
			m_uiHistoryPos = 0;

			return SendWhisper(vsTokens[1], ConcatinateArgs(vsTokens.begin() + 2, vsTokens.end()));
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_whisper_help")));
		}
	}
	//Replying to last whisper
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_reply"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_reply_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			if (!m_lLastWhispers.empty())
			{
				m_vChatHistory.push_front(sMessage);
				m_uiHistoryPos = 0;
					
				return SendWhisper(m_lLastWhispers.front(), ConcatinateArgs(vsTokens.begin() + 1, vsTokens.end()));
			}
			else
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_reply_invalid")));
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_reply_help")));
		}
	}
	//Buddy list commands
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_buddy"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_buddy_short"))) == 0 ||
			 CompareNoCase(vsTokens[0], Translate(_T("chat_command_friend"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_friend_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_buddy_list"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_buddy_list_short"))) == 0)
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_buddy_list_message")));

				if (m_setBuddyList.size() > 0)
				{
					for (uiset_it it(m_setBuddyList.begin()); it != m_setBuddyList.end(); it++)
						if (m_mapUserList[*it].yStatus == CHAT_CLIENT_STATUS_CONNECTED)
							AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_user_info_online"), _T("name"), m_mapUserList[*it].sName));

					for (uiset_it it(m_setBuddyList.begin()); it != m_setBuddyList.end(); it++)
						if (m_mapUserList[*it].yStatus > CHAT_CLIENT_STATUS_CONNECTED)
							AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_user_info_in_game"), _T("name"), m_mapUserList[*it].sName, _T("game"), m_mapUserList[*it].sGameName));

					for (uiset_it it(m_setBuddyList.begin()); it != m_setBuddyList.end(); it++)
						if (m_mapUserList[*it].yStatus < CHAT_CLIENT_STATUS_CONNECTED)
							AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_user_info_offline"), _T("name"), m_mapUserList[*it].sName));
				}
				else
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_buddy_list_none")));
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_buddy_message"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_buddy_message_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
				{				
					if (GetChatModeType() == CHAT_MODE_INVISIBLE)
					{
						AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_failed_invisible")));
						return true;		
					}	
				
					tstring sMessage(ConcatinateArgs(vsTokens.begin() + 2, vsTokens.end()));

					CPacket pktSend;
					pktSend << CHAT_CMD_WHISPER_BUDDIES << sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH);
					m_sockChat.SendPacket(pktSend);

					AddIRCChatMessage(CHAT_MESSAGE_WHISPER_BUDDIES, Translate(_T("chat_whisper_buddies"), _T("message"), sMessage.substr(0, CHAT_MESSAGE_MAX_LENGTH)), TSNULL, false, true);
				}
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_buddy_message_help")));
				}
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_buddy_add"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_buddy_add_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
					RequestBuddyAdd(vsTokens[2]);
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_buddy_add_help")));
				}
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_buddy_delete"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_buddy_delete_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
					RequestBuddyRemove(vsTokens[2]);
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_buddy_delete_help")));
				}
			}
			else
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_buddy_delete_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_buddy_add_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_buddy_list_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_buddy_message_help")));
			}
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_buddy_delete_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_buddy_add_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_buddy_list_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_buddy_message_help")));
		}
	}
	//Clan list commands
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_clan"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_clan_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_list"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_list_short"))) == 0)
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_list_message")));

				if (m_setClanList.size() > 0)
				{
					for (uiset_it it(m_setClanList.begin()); it != m_setClanList.end(); it++)
						if (m_mapUserList[*it].yStatus == CHAT_CLIENT_STATUS_CONNECTED)
							AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_user_info_online"), _T("name"), m_mapUserList[*it].sName));

					for (uiset_it it(m_setClanList.begin()); it != m_setClanList.end(); it++)
						if (m_mapUserList[*it].yStatus > CHAT_CLIENT_STATUS_CONNECTED)
							AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_user_info_in_game"), _T("name"), m_mapUserList[*it].sName, _T("game"), m_mapUserList[*it].sGameName));

					for (uiset_it it(m_setClanList.begin()); it != m_setClanList.end(); it++)
						if (m_mapUserList[*it].yStatus < CHAT_CLIENT_STATUS_CONNECTED)
							AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_user_info_offline"), _T("name"), m_mapUserList[*it].sName));
				}
				else
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_list_none")));
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_message"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_message_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
				{
					m_vChatHistory.push_front(sMessage);
					m_uiHistoryPos = 0;

					return SendClanWhisper(ConcatinateArgs(vsTokens.begin() + 2, vsTokens.end()));
				}
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_message_help")));
				}
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_promote"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_promote_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
				{
					RequestPromoteClanMember(vsTokens[2]);
				}
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_promote_help")));
				}
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_demote"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_demote_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
				{
					RequestDemoteClanMember(vsTokens[2]);
				}
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_demote_help")));
				}
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_remove"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_remove_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
				{
					RequestRemoveClanMember(vsTokens[2]);
				}
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_remove_help")));
				}
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_invite"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_invite_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
				{
					InviteToClan(vsTokens[2]);
				}
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_invite_help")));
				}
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_leave"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_clan_leave_short"))) == 0)
			{
				if (!m_mapUserList[m_uiAccountID].sClan.empty())
					RequestRemoveClanMember(m_mapUserList[m_uiAccountID].sName);
				else
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_clan_no_clan")));
			}
			else
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_list_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_message_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_promote_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_demote_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_remove_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_invite_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_leave_help")));
			}
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_list_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_message_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_promote_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_demote_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_remove_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_invite_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_clan_leave_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_clear"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_clear_short"))) == 0)
	{
		AddIRCChatMessage(CHAT_MESSAGE_CLEAR);
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_join"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_join_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			tstring sChannel(ConcatinateArgs(vsTokens.begin() + 1, vsTokens.end()));
			const size_t zPos(sChannel.find(_T("\"")));
			
			// if no password enclosed in quotes found, join normally
			if (zPos == tstring::npos)
				JoinChannel(sChannel);
			else
			{
				// grab full channel name by taking everything after the / command up until the space before the first quote
				tstring sTempChannel(sChannel);
				sChannel = Trim(StringReplace(sChannel.substr(0, zPos-1), _T("\""), _T("")));
				
				// anything in quotes at the end becomes the password
				tstring sPassword(Trim(StringReplace(sTempChannel.substr(zPos), _T("\""), _T(""))));
				if (!sChannel.empty() && !sPassword.empty())
					JoinChannel(sChannel, sPassword);
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_join_help")));
				}
			}
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_join_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_invite"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_invite_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
			InviteUser(vsTokens[1]);
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invite_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_whois"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_whois_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
			GetUserInfo(vsTokens[1]);
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_whois_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_stats"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_stats_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			if (vsTokens.size() >= 3 && CompareNoCase(vsTokens[2], Translate(_T("chat_command_stats_pop"))) == 0)
			{
				Console.Execute(_T("ShowCCPanel"));
				Console.Execute(_T("ShowCCStatistics true"));
			}
			else
				SetRetrievingStats(true);

			Console.Execute(_T("GetPlayerStatsName ") + vsTokens[1]);
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_stats_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_joingame"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_joingame_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_joingame_buddy"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_joingame_buddy_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
				{
					if (!JoinGame(vsTokens[2]))
						AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_joingame_buddy_failed")));
				}
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_joingame_buddy_help")));
				}				
			}
/*			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_joingame_name"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_joingame_name_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
				{
					// TODO: Join game by name here
				}
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_IRC, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_IRC, Translate(_T("chat_command_joingame_name_help")));
				}
			}*/
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_joingame_ip"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_joingame_ip_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
				{
					Host.Connect(vsTokens[2]);
				}
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_joingame_ip_help")));
				}
			}
			else
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_joingame_buddy_help")));
				//AddIRCChatMessage(CHAT_MESSAGE_IRC, Translate(_T("chat_command_joingame_name_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_joingame_ip_help")));
			}
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_joingame_buddy_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_joingame_name_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_joingame_ip_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_topic"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_topic_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			if (IsAdmin(uiChannelID, m_uiAccountID))
				SetChannelTopic(uiChannelID, ConcatinateArgs(vsTokens.begin() + 1, vsTokens.end()));
			else
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_not_operator")));
		}
		else if (uiChannelID != -1)
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_topic"), _T("topic"), m_mapChannels[uiChannelID].sTopic), GetChannelName(uiChannelID));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_kick"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_kick_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			if (IsAdmin(uiChannelID, m_uiAccountID))
				KickUserFromChannel(uiChannelID, vsTokens[1]);
			else
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_not_operator")));
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_kick_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_ban"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			if (IsAdmin(uiChannelID, m_uiAccountID))
				BanUserFromChannel(uiChannelID, vsTokens[1]);
			else
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_not_operator")));
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ban_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_unban"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			if (IsAdmin(uiChannelID, m_uiAccountID))
				UnbanUserFromChannel(uiChannelID, vsTokens[1]);
			else
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_not_operator")));
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_unban_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_silence"))) == 0)
	{
		if (vsTokens.size() >= 3)
		{
			if (IsAdmin(uiChannelID, m_uiAccountID))
				SilenceChannelUser(uiChannelID, vsTokens[1], MinToMs(uint(AtoI(vsTokens[2]))));
			else
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_not_operator")));
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_silence_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_message_all"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_message_all_long"))) == 0)
	{
		if (vsTokens.size() >= 2)
			SendGlobalMessage(0, 0, ConcatinateArgs(vsTokens.begin() + 1, vsTokens.end()));
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_banlist"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_banlist_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_banlist_list"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_banlist_list_short"))) == 0)
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_list_message")));

				if (m_mapBanList.size() > 0)
				{
					for (ChatBanMap_it it(m_mapBanList.begin()); it != m_mapBanList.end(); it++)
						AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_list_entry"), _T("id"), XtoA(it->first), _T("name"), it->second.sName, _T("reason"), it->second.sReason));
				}
				else
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_list_none")));
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_banlist_add"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_banlist_add_short"))) == 0)
			{
				if (vsTokens.size() >= 4)
					RequestBanlistAdd(vsTokens[2], ConcatinateArgs(vsTokens.begin() + 3, vsTokens.end()));
				else
				{
					if (vsTokens.size() >= 3)
						RequestBanlistAdd(vsTokens[2], _T("None"));
					else
					{
						AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
						AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_add_help")));
					}
				}
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_banlist_delete"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_banlist_delete_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
					RequestBanlistRemove(vsTokens[2]);
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_delete_help")));
				}
			}
			else
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_delete_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_add_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_list_help")));
			}
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_delete_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_add_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_list_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_banlist"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_banlist_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_banlist_list"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_banlist_list_short"))) == 0)
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_list_message")));

				if (m_mapBanList.size() > 0)
				{
					for (ChatBanMap_it it(m_mapBanList.begin()); it != m_mapBanList.end(); it++)
						AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_list_entry"), _T("id"), XtoA(it->first), _T("name"), it->second.sName, _T("reason"), it->second.sReason));
				}
				else
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_list_none")));
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_banlist_add"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_banlist_add_short"))) == 0)
			{
				if (vsTokens.size() >= 4)
					RequestBanlistAdd(vsTokens[2], ConcatinateArgs(vsTokens.begin() + 3, vsTokens.end()));
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_add_help")));
				}
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_banlist_delete"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_banlist_delete_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
					RequestBanlistRemove(vsTokens[2]);
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_delete_help")));
				}
			}
			else
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_delete_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_add_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_list_help")));
			}
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_delete_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_add_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_banlist_list_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_ignore"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_ignore_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_ignore_list"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_ignore_list_short"))) == 0)
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignore_list_message")));

				if (m_mapIgnoreList.size() > 0)
				{
					for (ChatIgnoreMap_it it(m_mapIgnoreList.begin()); it != m_mapIgnoreList.end(); it++)
						AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignore_list_entry"), _T("id"), XtoA(it->first), _T("name"), it->second));
				}
				else
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignore_list_none")));
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_ignore_add"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_ignore_add_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
					RequestIgnoreAdd(vsTokens[2]);
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignore_add_help")));
				}
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_ignore_delete"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_ignore_delete_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
					RequestIgnoreRemove(vsTokens[2]);
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignore_delete_help")));
				}
			}
			else
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignore_delete_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignore_add_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignore_list_help")));
			}
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignore_delete_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignore_add_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignore_list_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_notes"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_notes_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_notes_list"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_notes_list_short"))) == 0)
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_list_message")));

				if (!m_vNotes.empty())
				{
					uint uiNoteNum(1);

					for (tsvector_it it(m_vNotes.begin()); it != m_vNotes.end(); it++)
						AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_list_entry"), _T("note"), *it, _T("id"), XtoA(uiNoteNum++)));
				}
				else
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_list_none")));
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_notes_add"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_notes_add_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
				{
					m_cDate = CDate(true);

					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_add_success")));
					m_vNotes.push_back(ConcatinateArgs(vsTokens.begin() + 2, vsTokens.end()));
					m_vNoteTimes.push_back(m_cDate.GetDateString(DATE_SHORT_YEAR | DATE_YEAR_LAST | DATE_MONTH_FIRST) + _T(" @ ") + m_cDate.GetTimeString());
					SaveNotes();
				}
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_add_help")));
				}
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_notes_delete"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_notes_delete_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
				{
					uint uiNoteNum(AtoI(vsTokens[2]));

					if (uiNoteNum > 0 && INT_SIZE(m_vNotes.size()) >= uiNoteNum)
					{
						AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_delete_success")));
						m_vNotes.erase(m_vNotes.begin() + (uiNoteNum - 1));
						SaveNotes();
					}
					else
						AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_delete_invalid")));
				}
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_delete_help")));
				}
			}
			else
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_delete_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_add_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_list_help")));
			}
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_delete_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_add_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_notes_list_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_promote"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_promote_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			uint uiAccountID(-1);
			bool bFound(false);

			for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
			{
				if (!CompareNames(it->second.sName, vsTokens[1]))
					continue;

				if (it->second.setChannels.find(uiChannelID) == it->second.setChannels.end())
					break;

				bFound = true;

				if (GetAdminLevel(uiChannelID, m_uiAccountID) > GetAdminLevel(uiChannelID, it->second.uiAccountID) + 1)
					uiAccountID = it->second.uiAccountID;

				break;
			}

			if (uiAccountID != -1)
				PromoteUserInChannel(uiChannelID, uiAccountID);
			else if (bFound)
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_promote_failure")));
			else
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_not_in_channel")));
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_promote_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_demote"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_demote_short"))) == 0)
	{
		if (vsTokens.size() >= 2)
		{
			uint uiAccountID(-1);
			bool bFound(false);
			bool bLowest(false);

			for (ChatClientMap_it it(m_mapUserList.begin()); it != m_mapUserList.end(); it++)
			{
				if (!CompareNames(it->second.sName, vsTokens[1]))
					continue;

				if (it->second.setChannels.find(uiChannelID) == it->second.setChannels.end())
					break;

				bFound = true;
				bLowest = (GetAdminLevel(uiChannelID, it->second.uiAccountID) == CHAT_CLIENT_ADMIN_NONE);

				if (GetAdminLevel(uiChannelID, m_uiAccountID) > GetAdminLevel(uiChannelID, it->second.uiAccountID))
					uiAccountID = it->second.uiAccountID;

				break;
			}

			if (bLowest)
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_demote_failure_too_low")));
			else if (uiAccountID != -1)
				DemoteUserInChannel(uiChannelID, uiAccountID);
			else if (bFound)
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_demote_failure")));
			else
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_not_in_channel")));
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_demote_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_auth"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_auth_short"))) == 0)
	{
		if (GetAdminLevel(uiChannelID, m_uiAccountID) < CHAT_CLIENT_ADMIN_LEADER)
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_no_permissions")));
		}
		else if (vsTokens.size() >= 2)
		{
			if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_auth_list"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_auth_list_short"))) == 0)
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_list_message")));
				RequestAuthList(uiChannelID);
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_auth_add"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_auth_add_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
					RequestAuthAdd(uiChannelID, vsTokens[2]);
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_add_help")));
				}
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_auth_delete"))) == 0 || CompareNoCase(vsTokens[1], Translate(_T("chat_command_auth_delete_short"))) == 0)
			{
				if (vsTokens.size() >= 3)
					RequestAuthRemove(uiChannelID, vsTokens[2]);
				else
				{
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format")));
					AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_delete_help")));
				}
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_auth_enable"))) == 0)
			{
				RequestAuthEnable(uiChannelID);
			}
			else if (CompareNoCase(vsTokens[1], Translate(_T("chat_command_auth_disable"))) == 0)
			{
				RequestAuthDisable(uiChannelID);
			}
			else
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_delete_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_add_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_list_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_enable_help")));
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_disable_help")));
			}
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_format_multi")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_delete_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_add_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_list_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_enable_help")));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_auth_disable_help")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_password"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_password_short"))) == 0)
	{
		if (!IsAdmin(uiChannelID, m_uiAccountID))
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_not_operator")));
		else if (vsTokens.size() >= 2)
			SetChannelPassword(uiChannelID, ConcatinateArgs(vsTokens.begin() + 1, vsTokens.end()));
		else
			SetChannelPassword(uiChannelID, TSNULL);
	}
	/* else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_roll"))) == 0)
	{	
		if (vsTokens.size() == 2)
		{
			if (AtoF(vsTokens[1]) <= 0 || AtoF(vsTokens[1]) > 32767)
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_roll_help")));
			}
			else
			{				
				const uint uiRand(M_Randnum(1, AtoI(vsTokens[1])));
				
				ChatClientMap_it findit(m_mapUserList.find(m_uiAccountID));
				
				const wstring sRollMessage = Translate(_T("chat_roll_message"), _T("player"), findit->second.sName, _T("low"), _T("1"), _T("high"), XtoA(AtoI(vsTokens[1])), _T("number"), XtoA(uiRand));
				
				if (!sRollMessage.empty() && uiChannelID != -1)
				{
					m_vChatHistory.push_front(vsTokens[0] + _T(" ") + vsTokens[1]);
					m_uiHistoryPos = 0;

					return SendChannelMessage(sRollMessage, uiChannelID, CHAT_MESSAGE_ROLL);
				}
			}
		}	
		else if (vsTokens.size() >= 3)
		{
			if (AtoI(vsTokens[1]) <= 0 || AtoI(vsTokens[2]) <= 0 || AtoI(vsTokens[2]) <= AtoI(vsTokens[1]) || AtoI(vsTokens[2]) > 32767)
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_roll_help")));
			}
			else
			{				
				const uint uiRand(M_Randnum(AtoI(vsTokens[1]), AtoI(vsTokens[2])));
				
				ChatClientMap_it findit(m_mapUserList.find(m_uiAccountID));
				
				const wstring sRollMessage = Translate(_T("chat_roll_message"), _T("player"), findit->second.sName, _T("low"), XtoA(vsTokens[1]), _T("high"), XtoA(vsTokens[2]), _T("number"), XtoA(uiRand));				
				
				if (!sRollMessage.empty() && uiChannelID != -1)
				{
					m_vChatHistory.push_front(vsTokens[0] + _T(" ") + vsTokens[1] + _T(" ") + vsTokens[2]);
					m_uiHistoryPos = 0;

					return SendChannelMessage(sRollMessage, uiChannelID, CHAT_MESSAGE_ROLL);
				}
			}
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_roll_help")));
		}	
	} */	
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_emote"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_emote_short"))) == 0)
	{
		if (vsTokens.size() > 1)
		{		
			ChatClientMap_it findit(m_mapUserList.find(m_uiAccountID));
						
			const wstring sEmoteMessage = findit->second.sName + _T(" ") + ConcatinateArgs(vsTokens.begin() + 1, vsTokens.end());
			
			if (!sEmoteMessage.empty() && uiChannelID != -1)
			{
				m_vChatHistory.push_front(ConcatinateArgs(vsTokens.begin(), vsTokens.end()));
				m_uiHistoryPos = 0;

				return SendChannelMessage(sEmoteMessage, uiChannelID, CHAT_MESSAGE_EMOTE);
			}
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_emote_help")));
		}	
	}		
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_available"))) == 0)
	{
		SetChatModeType(CHAT_MODE_AVAILABLE, _T("chat_command_available_message"));	
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_afk"))) == 0)
	{
		if (vsTokens.size() == 1)
		{
			if (GetChatModeType() == CHAT_MODE_AFK) 
			{
				SetChatModeType(CHAT_MODE_AVAILABLE, _T("chat_command_available_message"));	
			}
			else
			{
				SetChatModeType(CHAT_MODE_AFK, Translate(_T("chat_mode_afk_default_response")));
			}		
		}					
		else if (vsTokens.size() > 1)
		{					
			SetChatModeType(CHAT_MODE_AFK, ConcatinateArgs(vsTokens.begin() + 1, vsTokens.end()));
		}
	}	
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_dnd"))) == 0)
	{
		if (vsTokens.size() == 1)
		{
			if (GetChatModeType() == CHAT_MODE_DND) 
			{
				SetChatModeType(CHAT_MODE_AVAILABLE, _T("chat_command_available_message"));	
			}
			else
			{
				SetChatModeType(CHAT_MODE_DND, Translate(_T("chat_mode_dnd_default_response")));
			}		
		}					
		else if (vsTokens.size() > 1)
		{					
			SetChatModeType(CHAT_MODE_DND, ConcatinateArgs(vsTokens.begin() + 1, vsTokens.end()));
		}
	}	
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_time"))) == 0)
	{
		m_cDate = CDate(true);
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_current_time"), _T("time"), m_cDate.GetTimeString() + _T(" ") + m_cDate.GetDateString(DATE_MONTH_FIRST)));
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_ignorechat"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_ignorechat_short"))) == 0)
	{
		if (GetIgnoreChat() == CHAT_IGNORE_NONE) 
		{
			SetIgnoreChat(CHAT_IGNORE_ENEMY_ALL); // ignore enemy all chat
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignorechat_enemy_all_on")));
		}
		else if (GetIgnoreChat() == CHAT_IGNORE_ENEMY_ALL) 
		{
			SetIgnoreChat(CHAT_IGNORE_ALL); // ignore all chat
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignorechat_all_on")));
		}
		else if (GetIgnoreChat() == CHAT_IGNORE_ALL) 
		{
			SetIgnoreChat(CHAT_IGNORE_TEAM); // ignore team chat
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignorechat_team_on")));
		}
		else if (GetIgnoreChat() == CHAT_IGNORE_TEAM)
		{
			SetIgnoreChat(CHAT_IGNORE_EVERYONE); // ignore every one
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignorechat_every_on")));
		}
		else if (GetIgnoreChat() == CHAT_IGNORE_EVERYONE)
		{
			SetIgnoreChat(CHAT_IGNORE_NONE); // ignore no chat
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_ignorechat_off")));
		}
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_friendlychat"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_friendlychat_short"))) == 0)
	{
		if (GetFriendlyChat())
		{
			SetFriendlyChat(false);
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_friendlychat_off")));
		}
		else
		{
			SetFriendlyChat(true);
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_friendlychat_on")));
		}
	}
	else if (CompareNoCase(vsTokens[0], _T("/adminkick")) == 0)
	{
		if (vsTokens.size() == 2)
			AdminKick(vsTokens[1]);
		else if (vsTokens.size() == 3)
			AdminKick(vsTokens[1], AtoI(vsTokens[2]));
	}	
	else if (CompareNoCase(vsTokens[0], _T("/forcegroupmatchup")) == 0)
	{
		if (vsTokens.size() == 3)
			ForceGroupMatchup(vsTokens[1], vsTokens[2]);
	}	
	else if (CompareNoCase(vsTokens[0], _T("/endmatch")) == 0)
	{
		if (vsTokens.size() == 2)
			EndMatch(AtoI(vsTokens[1]), 0);
		else if (vsTokens.size() == 3)
			EndMatch(AtoI(vsTokens[1]), AtoI(vsTokens[2]));
	}	
	else if (CompareNoCase(vsTokens[0], _T("/setmatchmakingversion")) == 0)
	{
		if (vsTokens.size() == 2)
			SetMatchmakingVersion(vsTokens[1]);
	}	
	else if (CompareNoCase(vsTokens[0], _T("/hontour")) == 0)
	{
		if (vsTokens.size() == 1)
			RequestScheduledMatchInfo(TSNULL);
		else if (vsTokens.size() == 2)
			RequestScheduledMatchInfo(vsTokens[1]);
	}	
	else if (CompareNoCase(vsTokens[0], _T("/hontourbroadcast")) == 0 || CompareNoCase(vsTokens[0], _T("/htb")) == 0)
	{
		if (vsTokens.size() >= 3)
			SendGlobalMessage(1, AtoI(vsTokens[1]), ConcatinateArgs(vsTokens.begin() + 2, vsTokens.end()));
	}	
	else if (CompareNoCase(vsTokens[0], _T("/hontourmatchlobbyinfo")) == 0 || CompareNoCase(vsTokens[0], _T("/htmli")) == 0)
	{
		if (vsTokens.size() >= 2)
			RequestScheduledMatchLobbyInfo(AtoI(vsTokens[1]));
	}	
	else if (CompareNoCase(vsTokens[0], _T("/blockphrase")) == 0)
	{
		if (vsTokens.size() == 2)
			BlockPhrase(vsTokens[1]);
	}
	else if (CompareNoCase(vsTokens[0], _T("/unblockphrase")) == 0)
	{
		if (vsTokens.size() == 2)
			UnblockPhrase(vsTokens[1]);
	}
	else if (CompareNoCase(vsTokens[0], Translate(_T("chat_command_help"))) == 0 || CompareNoCase(vsTokens[0], Translate(_T("chat_command_help_short"))) == 0)
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_valid")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_whisper")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_reply")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_buddy")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_clan")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_join")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_clear")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_invite")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_whois")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_stats")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_joingame")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_topic")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_kick")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_ban")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_unban")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_silence")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_banlist")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_ignore")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_ignorechat")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_friendlychat")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_notes")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_promote")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_demote")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_auth")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_password")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_password_clear")));
		/* AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_roll"))); */
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_emote")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_time")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_ping")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_available")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_afk")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_dnd")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_matchup")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_gameinfo")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_misc")));
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_help_weather")));
	}
	else if (vsTokens[0].substr(0, 1) == Translate(_T("chat_command_character")))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_command_invalid_command"), _T("helpcommand"), Translate(_T("chat_command_help"))));
	}
	else
	{
		if (!sMessage.empty() && uiChannelID != -1)
		{
			m_vChatHistory.push_front(sMessage);
			m_uiHistoryPos = 0;

			return SendChannelMessage(sMessage, uiChannelID);
		}
	}

	m_vChatHistory.push_front(sMessage);
	m_uiHistoryPos = 0;

	return true;
}


/*====================
  CChatManager::UpdateFollow
  ====================*/
void CChatManager::UpdateFollow(const wstring& sServer)
{
	if (!m_bFollow)
		return;

	ChatClientMap_it itLocalClient(m_mapUserList.find(m_uiAccountID));
	
	if (sServer.empty())
		Host.Disconnect(DISCONNECT_SERVER_INVALID);
	else if (sServer != itLocalClient->second.sServerAddressPort)// && Host.GetConnectedAddress() != itLocalClient->second.sServerAddressPort)
		Host.Connect(sServer);
}


/*====================
  CChatManager::SetFollowing
  ====================*/
bool CChatManager::SetFollowing(const tstring& sName)
{
	if (IsBuddy(sName) || IsClanMember(sName) || IsCafeMember(sName))
	{
		m_bFollow = true;
		m_sFollowName = sName;
		const wstring sCleanName(RemoveClanTag(sName));
		
		// Loop over all the clients we have chat info on
		for (ChatClientMap_it it(m_mapUserList.begin()), itEnd(m_mapUserList.end()); it != itEnd; ++it)
		{
			// If one of the clients we have chat info on matches the player we are trying to follow
			if (RemoveClanTag(it->second.sName) == sCleanName)
			{
				// If the server/address is set, then try to connect to it but check to makee sure we aren't already 
				// connected to the server the one we are following is connected to
				if (!it->second.sServerAddressPort.empty())// && Host.GetConnectedAddress() != it->second.sServerAddressPort)
					Host.Connect(it->second.sServerAddressPort);
				else
					Host.Disconnect(DISCONNECT_SERVER_INVALID);
			}
		}
		
		return true;
	}
	
	return false;
}


/*====================
  CChatManager::GetFollowing
  ====================*/
tstring CChatManager::GetFollowing()
{
	if (!m_bFollow)
		return _T("");

	return m_sFollowName;
}


/*====================
  CChatManager::UnFollow
  ====================*/
void CChatManager::UnFollow()
{
	m_bFollow = false;
	m_sFollowName = _T("");
}


/*====================
  CChatManager::TabChatMessage
  ====================*/
tstring CChatManager::TabChatMessage(const tstring& sMessage)
{
	tstring m_sCurrentMessage(sMessage);

	if (m_bWhisperMode)
	{
		++m_uiTabNumber;
		if (m_uiTabNumber > m_lLastWhispers.size() - 1)
			m_uiTabNumber = 0;
		
		uint uiTempCount(0);
		list<tstring>::iterator it(m_lLastWhispers.begin());
		while (uiTempCount < m_uiTabNumber)
		{
			++it;
			++uiTempCount;
		}

		m_sCurrentMessage = Translate(_T("chat_command_whisper_short")) + _T(" ") + (*it) + _T(" ");
	}

	return m_sCurrentMessage;
}


/*====================
  CChatManager::UpdateClientChannelStatus
  ====================*/
void	CChatManager::UpdateClientChannelStatus(const tstring& sNewChannel, const tstring& sName, uint uiAccountID, byte yStatus, byte yFlags, uint uiChatSymbol, uint uiChatNameColor, uint uiAccountIcon, int iAccountIconSlot, uint uiAscensionLevel)
{
	ChatClientMap_it it(m_mapUserList.find(uiAccountID));

	if (it == m_mapUserList.end())
		return;

	bool bChannelUpdate(false);
	
	if (it->second.sName != sName ||
		it->second.yStatus != yStatus ||
		it->second.uiAccountID != uiAccountID ||
		it->second.uiChatSymbol != uiChatSymbol || 
		it->second.uiChatNameColor != uiChatNameColor ||
		it->second.uiAccountIcon != uiAccountIcon ||
		it->second.iAccountIconSlot != iAccountIconSlot ||
		it->second.yFlags != yFlags ||
		it->second.uiAscensionLevel != uiAscensionLevel)
		bChannelUpdate = true;

	it->second.sName = sName;
	it->second.yStatus = yStatus;
	it->second.uiAccountID = uiAccountID;
	it->second.yFlags = yFlags;
	it->second.uiChatSymbol = uiChatSymbol;
	it->second.uiChatNameColor = uiChatNameColor;
	it->second.uiAccountIcon = uiAccountIcon;
	it->second.iAccountIconSlot = iAccountIconSlot;
	it->second.uiAscensionLevel = uiAscensionLevel;

	uint uiChatNameColor2(uiChatNameColor);

	if (yFlags & CHAT_CLIENT_IS_STAFF && uiChatNameColor2 == INVALID_INDEX)
	{
		uint uiDevChatNameColor(Host.LookupChatNameColor(_CTS("s2logo")));
		if (uiDevChatNameColor != INVALID_INDEX)
			uiChatNameColor2 = uiDevChatNameColor;
	}
	if (yFlags & CHAT_CLIENT_IS_PREMIUM && uiChatNameColor2 == INVALID_INDEX)
	{
		uint uiGoldChatNameColor(Host.LookupChatNameColor(_CTS("goldshield")));
		if (uiGoldChatNameColor != INVALID_INDEX)
			uiChatNameColor2 = uiGoldChatNameColor;
	}

	if (uiChatNameColor2 != INVALID_INDEX)
		it->second.uiSortIndex = Host.GetChatNameColorSortIndex(uiChatNameColor2);
	else
		it->second.uiSortIndex = DEFAULT_CHAT_NAME_COLOR_SORT_INDEX;

	if (bChannelUpdate || !sNewChannel.empty())
	{
		static tsvector vParams(18);
		static tsvector vMiniParams(2);

		for (uiset_it itChan(it->second.setChannels.begin()); itChan != it->second.setChannels.end(); ++itChan)
		{
			bool bNewChannel(!sNewChannel.empty() && GetChannelName(*itChan) == sNewChannel);

			if (!bNewChannel && m_setChannelsIn.find(*itChan) == m_setChannelsIn.end())
				continue;

			// These stay the same throughout the rest of the function
			vParams[0] = vMiniParams[0] = GetChannelName(*itChan);

			if (!bNewChannel)
			{
				vMiniParams[1] = _T("EraseListItemByValue('") + it->second.sName + _T("');");
				ChatUserEvent.Trigger(vMiniParams);

				if (IsCafeChannel(sNewChannel))
				{
					m_setCafeList.erase(it->second.uiAccountID);
					RefreshCafeList();
				}
			}

			if (it->second.yStatus > CHAT_CLIENT_STATUS_DISCONNECTED)
			{
				vParams[1] = it->second.sName;
				vParams[2] = XtoA(GetAdminLevel(*itChan, it->first));
				vParams[3] = XtoA(it->second.yStatus > CHAT_CLIENT_STATUS_CONNECTED, true);
				vParams[4] = XtoA((it->second.yFlags & CHAT_CLIENT_IS_PREMIUM) != 0, true);
				vParams[5] = XtoA(it->second.uiAccountID);
				vParams[6] = Host.GetChatSymbolTexturePath(it->second.uiChatSymbol);
				vParams[7] = Host.GetChatNameColorTexturePath(it->second.uiChatNameColor);
				vParams[8] = Host.GetChatNameColorString(it->second.uiChatNameColor);
				vParams[9] = Host.GetChatNameColorIngameString(it->second.uiChatNameColor);
				vParams[10] = Host.GetAccountIconTexturePath(it->second.uiAccountIcon, it->second.iAccountIconSlot, it->second.uiAccountID);
				vParams[11] = XtoA(it->second.uiSortIndex);
				vParams[12] = XtoA(Host.GetChatNameGlow(it->second.uiChatNameColor));
				vParams[13] = Host.GetChatNameGlowColorString(it->second.uiChatNameColor);
				vParams[14] = Host.GetChatNameGlowColorIngameString(it->second.uiChatNameColor);
				vParams[15] = XtoA(uiAscensionLevel);
				vParams[16] = Host.GetChatNameColorFont(it->second.uiChatNameColor);
				vParams[17] = XtoA(Host.GetChatNameBackgroundGlow(it->second.uiChatNameColor));
				ChatUserNames.Trigger(vParams);

				if (IsCafeChannel(sNewChannel))
				{
					m_setCafeList.insert(it->second.uiAccountID);
					RefreshCafeList();
				}
			}

			vMiniParams[1] = _T("SortListboxSortIndex();");
			ChatUserEvent.Trigger(vMiniParams);
		}
	}
}


/*====================
  CChatManager::CreateTMMGroup
  ====================*/
void	CChatManager::CreateTMMGroup(byte yTMMType, byte yGameType/*ETMMGameTypes*/, const tstring& sMapName, const tstring& sGameModes, const tstring& sRegions, bool bRanked, byte yMatchFidelity, byte yBotDifficulty, bool bRandomizeBots)
{
	Console << _CTS("Creating TMM Group") << newl;
	Console << _CTS("TMMType: ") << yTMMType << newl;
	Console << _CTS("GameType: ") << yGameType << newl;
	Console << _CTS("MapName: ") << sMapName << newl;
	Console << _CTS("GameModes: ") << sGameModes << newl;
	Console << _CTS("Regions: ") << sRegions << newl;
	Console << _CTS("Ranked: ") << bRanked << newl;
	Console << _CTS("MatchFidelity: ") << yMatchFidelity << newl;
	Console << _CTS("BotDifficulty: ") << yBotDifficulty << newl;
	Console << _CTS("RandomizeBots: ") << bRandomizeBots << newl;

	tstring sData = LogCollector.GetJsonKeyValueString(_T("GamePhase"), _T("PLAY_GAME"));
	LogCollector.HttpSend(ELCCmd_GamePhase, ELCLevel_Info, sData);

	m_bInGroup = false;
	m_uiTMMGroupLeaderID = INVALID_INDEX;
	m_bTMMOtherPlayersReady = false;
	m_bTMMAllPlayersReady = false;
	m_uiPendingMatchTimeEnd = 0;
	m_sTMMMapName = TSNULL;
	m_yGroupSize = 0;	
	
	UnFollow();
		
	for (uint ui(0); ui < MAX_GROUP_SIZE; ++ui)
		m_aGroupInfo[ui].Clear();

	m_uiTMMSelfGroupIndex = 0;

	m_uiTMMStartTime = INVALID_TIME;
	m_uiTMMAverageQueueTime = INVALID_TIME;

	UpdateReadyStatus();

	CPacket pktSend;	
	pktSend << NET_CHAT_CL_TMM_GROUP_CREATE << K2_Version3(K2System.GetVersionString()) << yTMMType << yGameType << sMapName << sGameModes << sRegions << BYTE_BOOL(bRanked) << yMatchFidelity << yBotDifficulty << BYTE_BOOL(bRandomizeBots);

	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::IsVIP
  ====================*/
bool	CChatManager::IsVIP() const
{
	if (!IsConnected())
		return false;

	ChatClientMap_cit itFind(m_mapUserList.find(m_uiAccountID));

	if (itFind != m_mapUserList.end())
		return itFind->second.yFlags & CHAT_CLIENT_IS_STAFF || itFind->second.yFlags & CHAT_CLIENT_IS_PREMIUM;
	else
		return false;
}

/*====================
  CChatManager::IsStaff
  ====================*/
bool	CChatManager::IsStaff() const
{
	const CHostClient* pClient(Host.GetActiveClient()); 

	if (pClient == NULL) 
		return false; 
	
	const CClientAccount& account(pClient->GetAccount()); 
	
	return account.IsStaff();
}


/*====================
  CChatManager::JoinTMMGroup
  ====================*/
bool CChatManager::JoinTMMGroup(const wstring& sNickname)
{
	if (Host.IsInGame())
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_unable_to_join_group"));
		return false;
	}
	
	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_GROUP_JOIN << K2_Version3(K2System.GetVersionString()) << sNickname;
	m_sockChat.SendPacket(pktSend);
	
	return true;
}


/*====================
  CChatManager::LeaveTMMGroup
  ====================*/
void	CChatManager::LeaveTMMGroup(bool bLocalOnly, const tstring& sReason, uint uiValue)
{
	bool bWasInGroup(m_bInGroup);

	if (cc_printTMMUpdates)
	{
		Console << L"Leaving current group, was in group: " << XtoA(bWasInGroup) << newl;
	}

	m_bInGroup = false;
	m_uiTMMGroupLeaderID = INVALID_INDEX;
	m_bTMMOtherPlayersReady = false;
	m_bTMMAllPlayersReady = false;
	m_uiPendingMatchTimeEnd = 0;
	m_sTMMMapName = TSNULL;
	m_yGroupSize = 0;	
		
	for (uint ui(0); ui < MAX_GROUP_SIZE; ++ui)
		m_aGroupInfo[ui].Clear();

	m_uiTMMSelfGroupIndex = 0;

	if (sReason == _CTS("disconnected"))
		m_uiTMMBanTime = INVALID_TIME;

	m_uiTMMStartTime = INVALID_TIME;
	m_uiTMMAverageQueueTime = INVALID_TIME;

	UpdateReadyStatus();

	tsvector vParams(3);
	vParams[0] = sReason;
	vParams[1] = XtoA(bWasInGroup);
	vParams[2] = XtoA(uiValue);

	TMMLeaveGroup.Trigger(vParams, cc_forceTMMInterfaceUpdate);

	Console << _T("Left TMM Group - ") << (!sReason.empty() ? sReason : _T("NULL")) << newl;

	if (!bLocalOnly)
	{
		CPacket pktSend;
		pktSend << NET_CHAT_CL_TMM_GROUP_LEAVE;
		m_sockChat.SendPacket(pktSend);
	}
}


/*====================
  CChatManager::InviteToTMMGroup
  ====================*/
void CChatManager::InviteToTMMGroup(const wstring& sNickname)
{
	if (!IsInGroup())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_GROUP_INVITE << sNickname;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::JoinTMMQueue
  ====================*/
void CChatManager::JoinTMMQueue()
{
	if (!IsInGroup())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::LeaveTMMQueue
  ====================*/
void CChatManager::LeaveTMMQueue()
{
	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::RejectTMMInvite
  ====================*/
void CChatManager::RejectTMMInvite(const wstring& sNickname)
{
	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_GROUP_REJECT_INVITE << sNickname;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::KickFromTMMGroup
  ====================*/
void CChatManager::KickFromTMMGroup(byte ySlotNumber)
{
	if (!IsInGroup())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_GROUP_KICK << ySlotNumber;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::SendTMMGroupOptionsUpdate
  ====================*/
void	CChatManager::SendTMMGroupOptionsUpdate(byte yGameType, const tstring& sMapName, const tstring& sGameModes, const tstring& sRegions, bool bRanked, byte yMatchFidelity, byte yBotDifficulty, bool bRandomizeBots)
{
	Console << _CTS("Updating group options") << newl;

	if (yGameType == 99)
	{
		Console << _CTS("MatchFidelity: ") << yMatchFidelity << newl;
	}
	else
	{
		Console << _CTS("GameType: ") << yGameType << newl;
		Console << _CTS("MapName: ") << sMapName << newl;
		Console << _CTS("GameModes: ") << sGameModes << newl;
		Console << _CTS("Regions: ") << sRegions << newl;
		Console << _CTS("Ranked: ") << bRanked << newl;
		Console << _CTS("MatchFidelity: ") << yMatchFidelity << newl;
		Console << _CTS("BotDifficulty: ") << yBotDifficulty << newl;
		Console << _CTS("RandomizeBots: ") << bRandomizeBots << newl;

		if (m_uiTMMStartTime != INVALID_TIME)
			Console.Warn << _CTS("TMM options reset while in queue") << newl;
	}

	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_GAME_OPTION_UPDATE << yGameType << sMapName << sGameModes << sRegions << BYTE_BOOL(bRanked) << yMatchFidelity << yBotDifficulty << BYTE_BOOL(bRandomizeBots);

	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::SendTMMGroupEnemyBotUpdate
  ====================*/
void	CChatManager::SendTMMGroupEnemyBotUpdate(byte yBotSlot, const tstring& sBot)
{
	if (yBotSlot > 4)
		return;

	byte yTeamType(2);

	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_BOT_GROUP_UPDATE 
		<< yTeamType
		<< yBotSlot
		<< sBot;
	
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::SendTMMGroupAllyBotUpdate
  ====================*/
void	CChatManager::SendTMMGroupTeamBotUpdate(byte yBotSlot, const tstring& sBot)
{
	if (yBotSlot > 4)
		return;

	byte yTeamType(1);

	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_BOT_GROUP_UPDATE 
		<< yTeamType
		<< yBotSlot
		<< sBot;
	
	m_sockChat.SendPacket(pktSend);
}

/*====================
  CChatManager::SendTMMChangeGroupType
  ====================*/
void	CChatManager::SendTMMChangeGroupType(byte yType)
{
	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_CHANGE_GROUP_TYPE << yType;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::SendTMMSwapGroupType
  ====================*/
void	CChatManager::SendTMMSwapGroupType()
{
	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_SWAP_GROUP_TYPE;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::AcceptPendingMatch
  ====================*/
void	CChatManager::AcceptPendingMatch()
{
	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_ACCEPT_PENDING_MATCH;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::RequestTMMPopularityUpdate
  ====================*/
void CChatManager::RequestTMMPopularityUpdate()
{
	if (!IsConnected())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_POPULARITY_UPDATE;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::SendTMMPlayerLoadingUpdate
  ====================*/
void	CChatManager::SendTMMPlayerLoadingUpdate(byte yPercent)
{
	if (!IsInGroup() && !IsInScheduledMatch())
		return;

	CPacket pktSend;	
	pktSend << NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS << yPercent;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::SendTMMPlayerReadyStatus
  ====================*/
void CChatManager::SendTMMPlayerReadyStatus(byte yReadyStatus, byte yGameType)
{
	if (!IsInGroup())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS << yReadyStatus << yGameType;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::IsInGroup
  ====================*/
bool	CChatManager::IsInGroup()
{
	if (!IsConnected())
		return false;

	return m_bInGroup;
}


/*====================
  CChatManager::IsTMMEnabled
  ====================*/
bool	CChatManager::IsTMMEnabled()
{
	if (!IsConnected())
		return false;

	return m_bTMMEnabled;
}


/*====================
  CChatManager::IsInQueue
  ====================*/
bool CChatManager::IsInQueue()
{
	if (m_uiTMMStartTime == INVALID_TIME || !IsConnected())
		return false;
	else
		return true;
}


/*====================
  CChatManager::GetGroupLeaderID
  ====================*/
uint	CChatManager::GetGroupLeaderID()
{
	if (!IsConnected())
		return INVALID_INDEX;

	return m_uiTMMGroupLeaderID;
}


/*====================
  CChatManager::GetOtherPlayersReady
  ====================*/
bool	CChatManager::GetOtherPlayersReady()
{
	if (!IsConnected())
		return false;

	return m_bTMMOtherPlayersReady;
}


/*====================
  CChatManager::GetAllPlayersReady
  ====================*/
bool	CChatManager::GetAllPlayersReady()
{
	if (!IsConnected())
		return false;

	return m_bTMMAllPlayersReady;
}


/*====================
  CChatManager::CanGroupPlayerAccessRegion
  ====================*/
bool	CChatManager::CanPlayerAccessRegion(const wstring& sRegion)
{
	// Read the regions from the chat server CVAR... "US->USW|TH->TH"
	const wsvector vCountryRestrictions(TokenizeString(m_sRestrictedRegions, L'|'));

	if (vCountryRestrictions.empty())
		return true;

	for (wsvector_cit citCountryRestrictions(vCountryRestrictions.begin()), citEnd(vCountryRestrictions.end()); citCountryRestrictions != citEnd; ++citCountryRestrictions)
	{
		// Be safe and make sure we don't substr() on an empty string
		const wstring sCountryRestrictions(*citCountryRestrictions);
		
		if (sCountryRestrictions.empty())
			continue;
	
		const size_t zPos(sCountryRestrictions.find(L"->"));
		
		if (zPos == wstring::npos)
			continue;
	
		// Strip off the country code
		const wstring sCountry(sCountryRestrictions.substr(0, 2));
		
		// Strip off the server region corresponding to the country code
		const wstring sServerRegion(sCountryRestrictions.substr(zPos + 2));
		
		// Check if the local player can access the server region, given the restricted regions and the country they are from
		if (sServerRegion == sRegion && sCountry != GetCountry())
			return false;
	}
	
	return true;
}


/*====================
  CChatManager::CanGroupPlayerAccessRegion
  ====================*/
bool	CChatManager::CanGroupPlayerAccessRegion(const wstring& sNickName, const wstring& sRegion)
{
	// All these must be true in order to actually perform the logic this function needs
	if (!IsInGroup() || sNickName.empty() || sRegion.empty())
		return true;

	// Read the regions from the chat server CVAR... "US->USW|TH->TH"
	const wsvector vCountryRestrictions(TokenizeString(m_sRestrictedRegions, L'|'));

	// Read all the available regions from the chat server CVAR
	const wsvector vAvailableRegions(TokenizeString(m_sAvailableRegions, L'|'));
	
	// If either of these vectors is empty then then just default to displaying that the player can access the region
	if (vCountryRestrictions.empty() || vAvailableRegions.empty())
		return true;

	for (wsvector_cit citCountryRestrictions(vCountryRestrictions.begin()), citEnd(vCountryRestrictions.end()); citCountryRestrictions != citEnd; ++citCountryRestrictions)
	{
		// Be safe and make sure we don't substr() on an empty string
		const wstring sCountryRestrictions(*citCountryRestrictions);
		
		if (sCountryRestrictions.empty())
			continue;
	
		// Strip off the country code
		const wstring sCountry(sCountryRestrictions.substr(0, 2));
		
		const size_t zPos(sCountryRestrictions.find(L"->"));
		
		if (zPos == wstring::npos)
			continue;
		
		// Strip off the server region corresponding to the country code
		const wstring sServerRegion(sCountryRestrictions.substr(zPos + 2));
		
		// Loop over each region this group has selected and determine if the regions they chose are restricted
		for (wsvector_cit citRegion(vAvailableRegions.begin()), citEndRegion(vAvailableRegions.end()); citRegion != citEndRegion; ++citRegion)
		{
			// If they chose a restricted region, check to make sure each group member can access the region
			if (sRegion == sServerRegion && (*citRegion) == sServerRegion)
			{
				for (uint i(0); i < m_yGroupSize; ++i)
				{
					// If the region this player is from is not equal to the country the server is restricted to then this player can not access this region, 
					if (RemoveClanTag(LowerString(m_aGroupInfo[i].sName)) == RemoveClanTag(LowerString(sNickName)) && m_aGroupInfo[i].sCountry != sCountry)
					{
						return false;
					}
				}
			}			
		}
	}
	
	return true;
}


/*====================
  CChatManager::CanGroupAccessRegion
  ====================*/
bool	CChatManager::CanGroupAccessRegion(const wstring& sRegion)
{
	if (sRegion.empty())
		return true;

	// Read the regions from the chat server CVAR into a wsvector... "US->USW|TH->TH"
	const wsvector vCountryRestrictions(TokenizeString(m_sRestrictedRegions, L'|'));

	if (vCountryRestrictions.empty())
		return true;

	for (wsvector_cit citCountryRestrictions(vCountryRestrictions.begin()), citEnd(vCountryRestrictions.end()); citCountryRestrictions != citEnd; ++citCountryRestrictions)
	{
		// Be safe and make sure we don't substr() on an empty string
		const wstring sCountryRestrictions(*citCountryRestrictions);
		
		if (sCountryRestrictions.empty())
			continue;
	
		// Strip off the country code
		const wstring sCountry(sCountryRestrictions.substr(0, 2));
		
		const size_t zPos(sCountryRestrictions.find(L"->"));
		
		if (zPos == wstring::npos)
			continue;
		
		// Strip off the server region corresponding to the country code
		const wstring sServerRegion(sCountryRestrictions.substr(zPos + 2));
		
		// If they chose a restricted region, check to make sure each group member can access the region
		if (sRegion == sServerRegion)
		{
			for (uint i(0); i < m_yGroupSize; ++i)
			{
				// If the country this player is from is not equal to the country the server is restricted to then this group can not access this region, 
				if (m_aGroupInfo[i].sCountry != sCountry)
				{
					return false;
				}
			}
		}
	}
	
	return true;
}


/*====================
  CChatManager::IsEnabledGameMode
  ====================*/
bool	CChatManager::IsEnabledGameMode(const wstring& sGameType, const wstring& sMapName, const wstring& sGameMode, const wstring& sRanked) const
{
	if (sGameMode.empty())
		return false;

	// Check for disabled game modes based on game type
	if (!sGameType.empty())
	{
		for (wsvector_cit cit(m_vDisabledGameModesByGameType.begin()), citEnd(m_vDisabledGameModesByGameType.end()); cit != citEnd; ++cit)
		{
			wstring sDisabledGameMode((*cit));
			if (sDisabledGameMode.empty())
				continue;

			size_t zPos(sDisabledGameMode.find(L"->"));
			if (zPos == wstring::npos)
				continue;

			wstring sGameTypeCheck(sDisabledGameMode.substr(0, zPos));

			if (sGameType == sGameTypeCheck)
			{
				wstring sGameModeCheck(sDisabledGameMode.substr(zPos + 2));

				if (sGameMode == sGameModeCheck)
				{
					return false;
				}
			}
		}
	}

	// Check for disabled game modes based on rank type
	if (!sRanked.empty())
	{
		for (wsvector_cit cit(m_vDisabledGameModesByRankType.begin()), citEnd(m_vDisabledGameModesByRankType.end()); cit != citEnd; ++cit)
		{
			wstring sDisabledGameMode((*cit));
			if (sDisabledGameMode.empty())
				continue;

			size_t zPos(sDisabledGameMode.find(L"->"));
			if (zPos == wstring::npos)
				continue;

			bool bRankTypeCheck(AtoB(sDisabledGameMode.substr(0, zPos)));	
			bool bRanked(AtoB(sRanked));

			if (bRanked == bRankTypeCheck)
			{
				wstring sGameModeCheck(sDisabledGameMode.substr(zPos + 2));

				if (sGameMode == sGameModeCheck)
				{
					return false;
				}
			}
		}
	}

	// Check for disabled game modes based on map
	if (!sMapName.empty())
	{
		for (wsvector_cit cit(m_vDisabledGameModesByMap.begin()), citEnd(m_vDisabledGameModesByMap.end()); cit != citEnd; ++cit)
		{
			wstring sDisabledGameMode((*cit));
			if (sDisabledGameMode.empty())
				continue;

			size_t zPos(sDisabledGameMode.find(L"->"));
			if (zPos == wstring::npos)
				continue;

			wstring sMapCheck(sDisabledGameMode.substr(0, zPos));

			if (sMapName == sMapCheck)
			{
				wstring sGameModeCheck(sDisabledGameMode.substr(zPos + 2));

				if (sGameMode == sGameModeCheck)
				{
					return false;
				}
			}
		}
	}

	return true;
}


/*====================
  CChatManager::RequestGameInfo
  ====================*/
void CChatManager::RequestGameInfo(const wstring sNickname)
{
	if (!IsConnected())
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_REQUEST_GAME_INFO << sNickname;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::HandleRequestGameInfo
  ====================*/
void CChatManager::HandleRequestGameInfo(CPacket& pkt)
{
	wstring sNickName(pkt.ReadWString());
	wstring sGameName(pkt.ReadWString());
	wstring sMapName(pkt.ReadWString());
	bool bIsCasual(pkt.ReadByte() != 0);
	byte yHardcore(pkt.ReadByte());
	byte yGated(pkt.ReadByte());
	wstring sGameModeName(pkt.ReadWString());
	wstring sCGT(pkt.ReadWString());
	wstring sTeamInfo1(pkt.ReadWString());
	wstring sTeamInfo2(pkt.ReadWString());
	wstring sPlayerInfo0(pkt.ReadWString());
	wstring sPlayerInfo1(pkt.ReadWString());
	wstring sPlayerInfo2(pkt.ReadWString());
	wstring sPlayerInfo3(pkt.ReadWString());
	wstring sPlayerInfo4(pkt.ReadWString());
	wstring sPlayerInfo5(pkt.ReadWString());
	wstring sPlayerInfo6(pkt.ReadWString());
	wstring sPlayerInfo7(pkt.ReadWString());
	wstring sPlayerInfo8(pkt.ReadWString());
	wstring sPlayerInfo9(pkt.ReadWString());
	
	if (pkt.HasFaults())
		return;
	
	static wsvector vParams(20);
	
	vParams[0] = sNickName;
	vParams[1] = sTeamInfo1;
	vParams[2] = sTeamInfo2;
	vParams[3] = sPlayerInfo0;
	vParams[4] = sPlayerInfo1;
	vParams[5] = sPlayerInfo2;
	vParams[6] = sPlayerInfo3;
	vParams[7] = sPlayerInfo4;
	vParams[8] = sPlayerInfo5;
	vParams[9] = sPlayerInfo6;
	vParams[10] = sPlayerInfo7;
	vParams[11] = sPlayerInfo8;
	vParams[12] = sPlayerInfo9;
	vParams[13] = StripColorCodes(sGameName);
	vParams[14] = sMapName;
	vParams[15] = sGameModeName;
	vParams[16] = sCGT;
	vParams[17] = XtoA(bIsCasual);
	vParams[18] = XtoA(yHardcore);
	vParams[19] = XtoA(yGated);

	ChatRequestGameInfo.Trigger(vParams);
}


/*====================
  CChannelManager::GetAccountIconInfo
  ====================*/
void	CChatManager::GetAccountIconInfo(const tstring& sAccountIcon, uint& uiOutAccountIcon, int& iOutAccountIconSlot)
{
	tsvector vsIcon(TokenizeString(sAccountIcon, _T(':')));

	uiOutAccountIcon = Host.LookupAccountIcon(vsIcon.size() > 0 ? vsIcon[0] : TSNULL);
	iOutAccountIconSlot = vsIcon.size() > 1 ? AtoI(vsIcon[1]) : 0;
}


/*====================
  CChatManager::HandleTMMRegionUnavailable
  ====================*/
void CChatManager::HandleTMMRegionUnavailable(CPacket& pkt)
{
	wstring sRegionsUnavailable(pkt.ReadWString());
	
	if (pkt.HasFaults() || sRegionsUnavailable.empty())
		return;
	
	// Strip off the last "," from the region list
	sRegionsUnavailable = sRegionsUnavailable.substr(0, sRegionsUnavailable.length() - 1);

	RestrictedRegions.Trigger(sRegionsUnavailable);
}


/*====================
  CChatManager::HandleListData
  ====================*/
void CChatManager::HandleListData(CPacket& pkt)
{
	// This is just a testing function and the sending of the list data is initiated on the chat server
	uint uiIndex(0);

	while (1)
	{
		wstring sName(pkt.ReadWString());
		
		if (sName == L"END_OF_THE_LIST_DATA")
			break;

		uint uiAccountID(pkt.ReadInt());
		EChatClientStatus eStatus(static_cast<EChatClientStatus>(pkt.ReadByte()));
		byte yFlags(pkt.ReadByte());
		uint uiClanID(pkt.ReadInt());
		wstring sClan(pkt.ReadWString());
		uint uiChatSymbol(Host.LookupChatSymbol(pkt.ReadWString()));
		uint uiChatNameColor(Host.LookupChatNameColor(pkt.ReadWString()));	
		wstring sAccountIcon(pkt.ReadWString());

		wstring sServerAddresssPort;
		wstring sGameName;
		uint uiMatchID(0);

		if (eStatus > CHAT_CLIENT_STATUS_CONNECTED)
		{
			sServerAddresssPort = pkt.ReadWString();
		}

		if (eStatus == CHAT_CLIENT_STATUS_IN_GAME)
		{
			sGameName = pkt.ReadWString();
			uiMatchID = pkt.ReadInt();
		}

		uint uiAscensionLevel(pkt.ReadInt());

		if (pkt.HasFaults())
			return;


		uint uiAccountIcon(0);
		int iAccountIconSlot(0);

		GetAccountIconInfo(sAccountIcon, uiAccountIcon, iAccountIconSlot);
		
		ChatClientMap_it itAssociate(m_mapUserList.find(uiAccountID));

		if (itAssociate == m_mapUserList.end())
		{
			SChatClient cNewClient;
			itAssociate = m_mapUserList.insert(ChatClientPair(uiAccountID, cNewClient)).first;
			m_mapNameToAccountID.insert(pair<wstring, uint>(LowerString(StripClanTag(sName)), uiAccountID));
		}
		
		itAssociate->second.sName = sName;
		itAssociate->second.uiAccountID = uiAccountID;
		itAssociate->second.yStatus = static_cast<byte>(eStatus);
		itAssociate->second.yFlags = yFlags;
		itAssociate->second.iClanID = uiClanID;
		itAssociate->second.sClan = sClan;
		itAssociate->second.uiChatSymbol = uiChatSymbol;
		itAssociate->second.uiChatNameColor = uiChatNameColor;
		itAssociate->second.uiAccountIcon = uiAccountIcon;
		itAssociate->second.iAccountIconSlot = iAccountIconSlot;
		itAssociate->second.sServerAddressPort = sServerAddresssPort;
		itAssociate->second.sGameName = sGameName;
		itAssociate->second.uiMatchID = uiMatchID;
		itAssociate->second.uiAscensionLevel = uiAscensionLevel;
		
		uiIndex++;
		
		if (uiIndex > 1000)
			break;
	}
	
	if (uiIndex)
	{
		RefreshBuddyList();
		RefreshClanList();
	}
}


/*====================
  CChatManager::HandleActiveStreams
  ====================*/
void	CChatManager::HandleActiveStreams(CPacket& pkt)
{
	tstring sStreams(pkt.ReadTString());
	
	if (pkt.HasFaults())
		return;

#if defined(K2_AWESOMIUM)
	WebBrowserManager.TriggerSubscriptions(sStreams);
#endif // defined(K2_AWESOMIUM)
}


/*====================
  CChatManager::HandlePlayerSpectateRequest
  ====================*/
void CChatManager::HandlePlayerSpectateRequest(CPacket& pkt)
{
	EPlayerSpectateRequest eType(EPlayerSpectateRequest(pkt.ReadByte()));

	if (pkt.HasFaults())
		return;

	switch (eType)
	{
	case PLAYER_SPECTATE_REQUEST_RESPONSE:
	{
		tstring sTargetName(pkt.ReadTString());
		EPlayerSpectateRequestResponses eResponse(EPlayerSpectateRequestResponses(pkt.ReadByte()));

		if (pkt.HasFaults())
			return;

		if (Host.IsConnected() || sTargetName.empty() || sTargetName != StripClanTag(LowerString(Host.GetPendingPlayerSpectateName())))
			break;

		if (eResponse == PSRR_ALLOW)
			JoinGame(sTargetName, true);
		else
		{
			switch (eResponse)
			{
			case PSRR_DENY:
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_player_spectate_denied"), _T("name"), Host.GetPendingPlayerSpectateName()));
				break;
			case PSRR_FULL_SERVER:
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_player_spectate_denied_full_server"), _T("name"), Host.GetPendingPlayerSpectateName()));
				break;
			case PSRR_FULL_PLAYER:
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_player_spectate_denied_full_player"), _T("name"), Host.GetPendingPlayerSpectateName()));
				break;
			case PSSR_TOO_DEEP:
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_player_spectate_denied_too_deep"), _T("name"), Host.GetPendingPlayerSpectateName()));
				break;
			default:
				break;
			}
		}

		Host.SetPendingPlayerSpectate(TSNULL, false);
	}	break;

	default:
		break;
	}
}


/*====================
  CChatManager::RequestChangeBuddyGroup
  ====================*/
void	CChatManager::RequestChangeBuddyGroup(const wstring& sBuddyName, const wstring& sBuddyGroup)
{
	if (sBuddyName.empty())
		return;

	// The UI likes to work with nicknames instead of accountIDs so handle the translation for the UI
	uint uiBuddyID(0);
	const wstring sCleanBuddyNick(RemoveClanTag(sBuddyName));
	for (ChatClientMap_it it(m_mapUserList.begin()), itEnd(m_mapUserList.end()); it != itEnd; ++it)
	{
		if (RemoveClanTag(it->second.sName) == sCleanBuddyNick)
		{
			uiBuddyID = it->second.uiAccountID;
			break;
		}
	}
	
	// At this point they probably aren't really on their buddy list if we can't find them
	if (uiBuddyID <= 0)
		return;
		
	// Don't save off buddy groups with wierd % and ^ in them
	if (!sBuddyGroup.empty() && (sBuddyGroup.find(_CTS("%")) != tstring::npos || sBuddyGroup.find(_CTS("^")) != tstring::npos))
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_invalid_buddy_group"), _CTS("buddy"), sCleanBuddyNick, _CTS("group"), sBuddyGroup));
		return;
	}
		
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;

	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester);
	pHTTPRequest->AddVariable(_CTS("f"), _CTS("change_buddy_group"));
	pHTTPRequest->AddVariable(_CTS("account_id"), m_uiAccountID);
	pHTTPRequest->AddVariable(_CTS("buddy_id"), uiBuddyID);
	pHTTPRequest->AddVariable(_CTS("buddy_nick"), sCleanBuddyNick);	
	pHTTPRequest->AddVariable(_CTS("group"), sBuddyGroup);
	pHTTPRequest->AddVariable(_CTS("cookie"), m_sCookie);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pNewRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_CHANGE_BUDDY_GROUP, sBuddyGroup));
	m_lHTTPRequests.push_back(pNewRequest);

	if (sBuddyGroup.empty())
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_removing_buddy_group"), _CTS("buddy"), sCleanBuddyNick));
	else
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_saving_buddy_group"), _CTS("buddy"), sCleanBuddyNick, _CTS("group"), sBuddyGroup));
}


/*====================
  CChatManager::ProcessChangeBuddyGroupResponse
  ====================*/
void	CChatManager::ProcessChangeBuddyGroupResponse(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());
	
	if (phpResponse.GetTString(_U8("change_buddy_group")) == _CTS("OK"))
	{
		ChatClientMap_it it(m_mapUserList.find(phpResponse.GetInteger(_U8("buddy_id"))));
		
		if (it != m_mapUserList.end())
		{
			if (phpResponse.GetTString(_U8("group")).empty())
			{
				it->second.sBuddyGroup = WSNULL;

				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_removed_buddy_group"), _CTS("buddy"), phpResponse.GetTString(_U8("buddy_nick"))));
			}
			else
			{
				// Remove any slashes that were added by the PHP script mysql escape routine
				it->second.sBuddyGroup = StringReplace(phpResponse.GetTString(_U8("group")), _T("\\'"), _T("'"));

				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_saved_buddy_group"), _CTS("buddy"), phpResponse.GetTString(_U8("buddy_nick")), _CTS("group"), it->second.sBuddyGroup));
			}
		}
		else
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_failed_buddy_group"), _CTS("buddy"), phpResponse.GetTString(_U8("buddy_nick"))));
	}
	else
	{
		AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_CTS("chat_failed_buddy_group"), _CTS("buddy"), phpResponse.GetTString(_U8("buddy_nick"))));
	}
}


/*====================
  CChatManager::GetBuddyGroup
  ====================*/
const wstring&	CChatManager::GetBuddyGroup(uint uiAccountID)
{
	if (uiAccountID <= 0)
		return WSNULL;

	ChatClientMap_it it(m_mapUserList.find(uiAccountID));
	
	if (it != m_mapUserList.end())
	{
		return it->second.sBuddyGroup;
	}
	
	return WSNULL;
}


/*====================
  CChatManager::GetBuddyGroup
  ====================*/
const wstring&	CChatManager::GetBuddyGroup(const wstring& sBuddyName)
{
	if (sBuddyName.empty())
		return WSNULL;

	// The UI likes to work with nicknames instead of accountIDs so handle the translation for the UI
	const wstring sCleanBuddyNick(RemoveClanTag(sBuddyName));
	for (ChatClientMap_it it(m_mapUserList.begin()), itEnd(m_mapUserList.end()); it != itEnd; ++it)
	{
		if (RemoveClanTag(it->second.sName) == sCleanBuddyNick)
		{
			return it->second.sBuddyGroup;
		}
	}
	
	return WSNULL;
}


/*====================
  CChatManager::SendAction
  ====================*/
void	CChatManager::SendAction(EActionCampaigns eType)
{
	if (!IsConnected())
		return;

	CPacket pktSend;
	pktSend << CHAT_CMD_TRACK_PLAYER_ACTION << byte(eType);
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::HandleEventInfo
  ====================*/
void CChatManager::HandleEventInfo(CPacket& pkt)
{
	// Anytime events are sent scheduled matches are sent also so clear both here so that we can notify the UI
	m_vEventInfo.clear();

	wstring sEventListing;
	uint uiNumEvents(pkt.ReadInt());

	for (uint i(0); i < uiNumEvents; i++)
	{
		uint uiEventID(pkt.ReadInt());
		tstring sEventName(StringReplace(StringReplace(pkt.ReadWString(), L"|", L"&#124;"), L"~", L"&#126;"));
		time_t tStartDate(pkt.ReadInt());
		time_t tEndDate(pkt.ReadInt());

		m_vEventInfo.push_back(SEventInfo(uiEventID, sEventName, tStartDate, tEndDate));
		sEventListing += XtoA(uiEventID) + L"|" + sEventName + L"|" + CDate(tStartDate).GetString(true) + L"|" + CDate(tEndDate).GetString(true) + L"~";	
	}
	
	if (pkt.HasFaults())
		return;

	if (!sEventListing.empty())
		EventListing.Trigger(sEventListing.substr(0, sEventListing.length() - 1));
}


/*====================
  CChatManager::HandleScheduledMatchInfo
  ====================*/
void CChatManager::HandleScheduledMatchInfo(CPacket& pkt)
{
	wstring sPlayerRequestedFor(pkt.ReadWString());

	// If we received a name here, then this scheduled match info update was requested by this client
	bool bRequested(!sPlayerRequestedFor.empty());

	// If a player requested this information to be sent to them, then we only use the information for specific display purposes, we want to keep 
	// them requesting this information as strictly an informational query so we don't risk messing with their scheduled match interface at all
	if (!bRequested)
	{
		// Since we store the currently selected match info inside of the scheduled match info map structure we need to make sure 
		// we don't overwrite the data we need access to in the map so we make a copy then reconstruct the map
		map<uint, SScheduledMatchInfo>::iterator itScheduledMatch(m_mapScheduledMatchInfo.find(m_uiScheduledMatchID));
		if (itScheduledMatch != m_mapScheduledMatchInfo.end())
		{
			SScheduledMatchInfo structScheduledMatch(itScheduledMatch->second);
			m_mapScheduledMatchInfo.clear();
			m_mapScheduledMatchInfo.insert(pair<uint, SScheduledMatchInfo>(m_uiScheduledMatchID, structScheduledMatch));
		}
		else
		{
			m_mapScheduledMatchInfo.clear();
		}
	}

	wstring sScheduledMatchListing;
	uint uiNumScheduledMatches(pkt.ReadInt());

	if (bRequested)
	{
		if (uiNumScheduledMatches)
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, L"Found " + XtoA(uiNumScheduledMatches) + L" scheduled matches for " + sPlayerRequestedFor + L":");
		else
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, L"No scheduled matches found for " + sPlayerRequestedFor);
	}
	
	for (uint i(0); i < uiNumScheduledMatches; i++)
	{
		uint uiEventID(pkt.ReadInt());
		uint uiScheduledMatchID(pkt.ReadInt());
		tstring sMatchTitle(StringReplace(StringReplace(pkt.ReadWString(), L"|", L"&#124;"), L"~", L"&#126;"));
		time_t tStartDate(pkt.ReadInt());
		time_t tExpirationDate(pkt.ReadInt());
		uint uiSecondsTillStart(pkt.ReadInt());
		uint uiSecondsTillExpiration(pkt.ReadInt());
		pkt.ReadByte();  // Unused placeholder
		byte yGameType(pkt.ReadByte());
		byte yTeamSize(pkt.ReadByte());
		tstring sMapName(pkt.ReadWString());
		tstring sGameMode(pkt.ReadWString());
		tstring sRegion(pkt.ReadWString());
		tstring sTeamName1(StringReplace(StringReplace(pkt.ReadWString(), L"|", L"&#124;"), L"~", L"&#126;"));
		tstring sTeamName2(StringReplace(StringReplace(pkt.ReadWString(), L"|", L"&#124;"), L"~", L"&#126;"));

		SScheduledMatchInfo structScheduledMatch(uiEventID, uiScheduledMatchID, sMatchTitle, tStartDate, tExpirationDate, yGameType, yTeamSize, sMapName, sGameMode, sRegion, sTeamName1, sTeamName2);

		if (!bRequested)
			Console << L"Found EventID:" << uiEventID << " ScheduledMatchID:" << uiScheduledMatchID << newl;

		// Now that we've populated basic match information loop over and read each of the team rosters
		for (uint j(0); j < 2; ++j)
		{
			uint uiRosterSize(pkt.ReadByte());

			for (uint k(0); k < uiRosterSize; ++k)
			{
				uint uiAccountID(pkt.ReadInt());
				byte yTeamSlot(pkt.ReadByte());
				structScheduledMatch.vTeam[j].push_back(SRosterInfo(uiAccountID, yTeamSlot));
			}
		}

		if (!bRequested)
		{
			// If the scheduled match we are populating isn't the currently selected scheduled match, or if for some reason the scheduled match wasn't 
			// inserted at the beginning of the function then insert it here
			map<uint, SScheduledMatchInfo>::iterator itScheduledMatch(m_mapScheduledMatchInfo.find(m_uiScheduledMatchID));
			if (m_uiScheduledMatchID != uiScheduledMatchID || itScheduledMatch != m_mapScheduledMatchInfo.end())
			{
				m_mapScheduledMatchInfo.insert(pair<uint, SScheduledMatchInfo>(uiScheduledMatchID, structScheduledMatch));
			}

			sScheduledMatchListing += XtoA(uiEventID) + L"|" + XtoA(uiScheduledMatchID) + L"|" + sMatchTitle + L"|" + CDate(tStartDate).GetString(true) + L"|" + 
										  CDate(tExpirationDate).GetString(true) + L"|" + XtoA(false /* Unused placeholder */) + L"|" + XtoA(yGameType) + L"|" + XtoA(yTeamSize) + L"|" +
										  sMapName + L"|" + sGameMode + L"|" + sRegion + L"|" + sTeamName1 + L"|" + sTeamName2 + L"~";
		}
		else
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, XtoA(i + 1) + L") Match Title:" + sMatchTitle);
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, L"ScheduledMatchID:" + XtoA(uiScheduledMatchID) + L" EventID:" + XtoA(uiEventID));
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, L"TimeTillStart:" + FormatTime(SecToMs(uiSecondsTillStart), 1, 0, FMT_NONE) + L" TimeTillExpiration:" + FormatTime(SecToMs(uiSecondsTillExpiration), 1, 0, FMT_NONE));
		}
	}
	
	if (pkt.HasFaults())
		return;

	if (!bRequested && !sScheduledMatchListing.empty())
		ScheduledMatchListing.Trigger(sScheduledMatchListing.substr(0, sScheduledMatchListing.length() - 1));
}


/*====================
  CChatManager::HandleScheduledMatchUpdates
  ====================*/
void	CChatManager::HandleScheduledMatchUpdates(CPacket& pkt)
{
	// This handles sending any updates pertaining to the scheduled match to all players in the pre-game lobby of the scheduled match.
	// It is designed to be stateless so any update will always provide all the information required so we can avoid synchronization complications
	byte yUpdateType(pkt.ReadByte());
	uint uiAccountID(pkt.ReadInt());
	m_uiScheduledMatchID = pkt.ReadInt();
	wstring sSpectatorList(pkt.ReadWString());

	// Only fire this trigger on the staff spectator's client when they join or leave a scheduled match
	if (!m_bSpectatingScheduledMatch)
	{
		CHostClient* pClient(Host.GetActiveClient());
		if (pClient != NULL && sSpectatorList.find(pClient->GetAccount().GetNickname()) != wstring::npos)
		{
			m_bSpectatingScheduledMatch = true;
			ScheduledMatchSpectatorInfo.Trigger(XtoA(true));
		}
	}

	map<uint, SScheduledMatchInfo>::iterator itScheduledMatch(m_mapScheduledMatchInfo.find(m_uiScheduledMatchID));

	if (itScheduledMatch != m_mapScheduledMatchInfo.end())
	{
		bool bHandlePartialUpdate(yUpdateType == SM_LOADING_UPDATE || yUpdateType == SM_PLAYER_READY || yUpdateType == SM_PLAYER_UNREADY);

		uint uiSecondsTillStart(pkt.ReadInt());
		uint uiSecondsTillExpiration(pkt.ReadInt());

		itScheduledMatch->second.uiSecondsTillStart = K2System.GetUnixTimestamp() + uiSecondsTillStart;
		itScheduledMatch->second.uiSecondsTillExpiration = K2System.GetUnixTimestamp() + uiSecondsTillExpiration;

		if (!bHandlePartialUpdate)
		{
			for (uint i(0); i < 2; ++i)
			{
				for (uint j(0); j < MAX_GROUP_SIZE; ++j)
				{
					itScheduledMatch->second.aGroupInfo[i][j].Clear();
				}
			}
		}

		for (uint i(0); i < 2; ++i)
		{
			uint uiGroupSize(pkt.ReadByte());

			for (uint j(0); j < uiGroupSize; ++j)
			{
				if (bHandlePartialUpdate)
				{
					itScheduledMatch->second.aGroupInfo[i][j].yLoadingPercent = pkt.ReadByte();
					itScheduledMatch->second.aGroupInfo[i][j].yReadyStatus = pkt.ReadByte();
				}
				else
				{
					itScheduledMatch->second.aGroupInfo[i][j].uiAccountID = pkt.ReadInt();
					itScheduledMatch->second.aGroupInfo[i][j].sName = pkt.ReadTString();
					itScheduledMatch->second.aGroupInfo[i][j].unRating = pkt.ReadShort();
					itScheduledMatch->second.aGroupInfo[i][j].yLoadingPercent = pkt.ReadByte();
					itScheduledMatch->second.aGroupInfo[i][j].yReadyStatus = pkt.ReadByte();
					pkt.ReadByte();		// Unused verified Placeholder
					itScheduledMatch->second.aGroupInfo[i][j].uiChatNameColor = (Host.LookupChatNameColor(pkt.ReadTString()));
					itScheduledMatch->second.aGroupInfo[i][j].sCountry = pkt.ReadTString();
				}
			}
		}

		if (pkt.HasFaults())
			return;
	}
	else
	{
		// We weren't able to find a stored scheduled match to match with the information we received, something is wrong
		pkt.Clear();
		return;
	}

	// Send all the scheduled match information to the UI
	static tsvector vScheduledMatchParams(14);

	if (yUpdateType == SM_UPDATE)
	{
		Console << L"Received SM update..." << newl;
	}
	else if (yUpdateType == SM_PLAYER_JOIN)
	{
		// If it was us that joined the scheduled match, make sure we don't follow anybody out of the match
		if (uiAccountID == GetAccountID())
		{
			Console << L"You (" << uiAccountID << L") joined the SM..." << newl;

			if (FileManager.GetUsingCustomFiles())
			{
				AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(_T("chat_mods_detected")));
			}

			UnFollow();
		}
		else
		{
			Console << L"Player (" << uiAccountID << L") joined SM..." << newl;
		}
	}
	else if (yUpdateType == SM_PLAYER_LEAVE)
	{
		// If it was us that left the scheduled match, then update the UI accordingly
		if (uiAccountID == GetAccountID())
		{
			Console << L"You (" << uiAccountID << L") left the SM..." << newl;
			LeaveScheduledMatch(true, _T("left"));
		}
		else
		{
			// Make sure we remove the entry
			bool bFound(false);

			for (uint i(0); i < 2; ++i)
			{
				for (uint j(0); j < MAX_GROUP_SIZE; ++j)
				{
					if (uiAccountID == itScheduledMatch->second.aGroupInfo[i][j].uiAccountID)
					{
						itScheduledMatch->second.aGroupInfo[i][j].Clear();
						Console << L"Player (" << uiAccountID << L") left the SM..." << newl;
						bFound = true;
						break;
					}
				}

				if (bFound)
					break;
			}
		}
	}
	else if (yUpdateType == SM_PLAYER_READY)
	{
		Console << L"Received SM player ready update..." << newl;
	}
	else if (yUpdateType == SM_PLAYER_UNREADY)
	{
		Console << L"Received SM player unready update..." << newl;
	}	
	else if (yUpdateType == SM_LOADING_UPDATE)
	{
		Console << L"Received SM loading update..." << newl;
	}
	else if (yUpdateType == SM_REMOVED)
	{
		Console << L"Received SM removed update..." << newl;
		LeaveScheduledMatch(true, _T("removed"));
		return;
	}
	else if (yUpdateType == SM_FORFEIT)
	{
		uint uiTeam(-1);

		// Figure out which team this local client is on
		for (uint i(0); i < 2; ++i)
		{
			for (uint j(0); j < MAX_GROUP_SIZE; ++j)
			{
				if (GetAccountID() == itScheduledMatch->second.aGroupInfo[i][j].uiAccountID)
				{
					uiTeam = i;
					break;
				}
			}

			if (uiTeam != -1)
				break;
		}

		Console << L"Received SM forfeit update..." << newl;

		// Figure out which team won the forfeit and fire the appropriate trigger (uiAccount value of 0 means team0 won, -1 means team1 won
		if (uiTeam == 0)
		{
			if (uiAccountID == 0)
				LeaveScheduledMatch(true, _T("forfeit_won"));
			else
				LeaveScheduledMatch(true, _T("forfeit_loss"));

			return;
		}
		else if (uiTeam == 1)
		{
			if (uiAccountID == -1)
				LeaveScheduledMatch(true, _T("forfeit_won"));
			else
				LeaveScheduledMatch(true, _T("forfeit_loss"));

			return;
		}		
	}

	vScheduledMatchParams[0] = XtoA(yUpdateType);

	vScheduledMatchParams[1] = XtoA(itScheduledMatch->second.uiEventID) + L"|" +
								XtoA(itScheduledMatch->second.uiScheduledMatchID) + L"|" +
								itScheduledMatch->second.sMatchTitle + L"|" +
								XtoA(itScheduledMatch->second.dateStartDate.GetString(true)) + L"|" +
								XtoA(itScheduledMatch->second.dateExpirationDate.GetString(true)) + L"|" +
								XtoA(false) + L"|" +	// Unused verified only Placeholder
								XtoA(itScheduledMatch->second.yGameType) + L"|" +
								itScheduledMatch->second.sMapName + L"|" +
								itScheduledMatch->second.sGameMode + L"|" +
								itScheduledMatch->second.sRegion + L"|" +
								itScheduledMatch->second.sTeamName[0] + L"|" +
								itScheduledMatch->second.sTeamName[1];

	uint uiIndex(1);

	for (uint i(0); i < 2; ++i)
	{
		for (uint j(0); j < MAX_GROUP_SIZE; ++j)
		{
			tstring sPlayerInfo;

			if (itScheduledMatch->second.aGroupInfo[i][j].uiAccountID != INVALID_INDEX)
			{			
				sPlayerInfo += XtoA(itScheduledMatch->second.aGroupInfo[i][j].uiAccountID) + L"|";
				sPlayerInfo += itScheduledMatch->second.aGroupInfo[i][j].sName + L"|";
				sPlayerInfo += XtoA(itScheduledMatch->second.aGroupInfo[i][j].unRating) + L"|";
				sPlayerInfo += XtoA(itScheduledMatch->second.aGroupInfo[i][j].yLoadingPercent) + L"|";
				sPlayerInfo += XtoA(itScheduledMatch->second.aGroupInfo[i][j].yReadyStatus) + L"|";
				sPlayerInfo += XtoA(false) + L"|";		// Unused verified Placeholder
				sPlayerInfo += XtoA(itScheduledMatch->second.aGroupInfo[i][j].uiChatNameColor) + L"|";
				sPlayerInfo += itScheduledMatch->second.aGroupInfo[i][j].sCountry;
			}

			vScheduledMatchParams[++uiIndex] = sPlayerInfo;
		}
	}

	// This indicates which player (if any) the SM update applied to
	vScheduledMatchParams[12] = XtoA(uiAccountID);
	vScheduledMatchParams[13] = sSpectatorList;

	ScheduledMatchInfo.Trigger(vScheduledMatchParams);

	// Check to see if all players are ready, if they are load their assets to prepare for the match to begin
	if ((_testHonTour && AreAllPlayersReady(m_uiScheduledMatchID, 0)) || (AreAllPlayersReady(m_uiScheduledMatchID, 0) && AreAllPlayersReady(m_uiScheduledMatchID, 1)))
	{
		if (!m_bSMMapLoaded)
		{
			Console << L"All players ready and m_bSMMapLoaded = false, pre-loading world " << itScheduledMatch->second.sMapName << L" ..." << newl;

			m_bSMMapLoaded = true;

			Host.PreloadWorld(itScheduledMatch->second.sMapName);
		}
	}
	else
	{
		m_bSMMapLoaded = false;
	}
}


/*====================
  CChatManager::HandleScheduledMatchFound
  ====================*/
void	CChatManager::HandleScheduledMatchFound(CPacket& pkt)
{
	byte yUpdateType(pkt.ReadByte());
	
	if (pkt.HasFaults())
		return;

	if (yUpdateType == SM_SERVER_LOCATING)
	{
		Console << L"Received SM server locating update..." << newl;
	}
	else if (yUpdateType == SM_SERVER_LOADING)
	{
		Console << L"Received SM server loading update..." << newl;
	}

	ScheduledMatchServerInfo.Trigger(XtoA(yUpdateType));
}


/*====================
  CChatManager::ScheduledMatchCommand
  ====================*/
void	CChatManager::ScheduledMatchCommand(const wstring& sCommand, uint uiValue)
{
	if (!IsConnected())
		return;

	if (sCommand == L"join")
	{
		if (Host.IsInGame())
		{
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, Translate(L"chat_unable_to_join_scheduled_match"));
			return;
		}

		if (IsInGroup())
			LeaveTMMGroup();

		if (IsInScheduledMatch())
			LeaveScheduledMatch(false, _T("left"));

		m_bSMMapLoaded = false;

		CPacket pktSend;
		pktSend << NET_CHAT_CL_TMM_SCHEDULED_MATCH_COMMAND << byte(SM_PLAYER_JOIN) << uiValue;
		m_sockChat.SendPacket(pktSend);
	}
	else if (sCommand == L"leave")
	{
		CPacket pktSend;
		pktSend << NET_CHAT_CL_TMM_SCHEDULED_MATCH_COMMAND << byte(SM_PLAYER_LEAVE);
		m_sockChat.SendPacket(pktSend);
	}
	else if (sCommand == L"ready")
	{
		CPacket pktSend;
		pktSend << NET_CHAT_CL_TMM_SCHEDULED_MATCH_COMMAND << byte(SM_PLAYER_READY);
		m_sockChat.SendPacket(pktSend);
	}
	else if (sCommand == L"unready")
	{
		CPacket pktSend;
		pktSend << NET_CHAT_CL_TMM_SCHEDULED_MATCH_COMMAND << byte(SM_PLAYER_UNREADY);
		m_sockChat.SendPacket(pktSend);
	}
}


/*====================
  CChatManager::AreAllPlayersReady
  ====================*/
bool	CChatManager::AreAllPlayersReady(uint uiScheduledMatchID, int iTeam, bool bIncludeTeamCaptain) const
{
	bool bAllPlayersReady(true);

	iTeam = CLAMP(iTeam, 0, 1);

	map<uint, SScheduledMatchInfo>::const_iterator itScheduledMatch(m_mapScheduledMatchInfo.find(uiScheduledMatchID));

	if (itScheduledMatch != m_mapScheduledMatchInfo.end())
	{
		uint uiTeamSize(itScheduledMatch->second.yTeamSize);

		for (uint j(0); j < uiTeamSize; ++j)
		{
			// We only want to check if all the players on the team besides the team captain are ready
			if (!bIncludeTeamCaptain && (j == 0))
				continue;

			if (itScheduledMatch->second.aGroupInfo[iTeam][j].yReadyStatus != 1)
			{
				bAllPlayersReady = false;
				break;
			}
		}
	}
	else
	{
		bAllPlayersReady = false;
	}

	return bAllPlayersReady;
}


/*====================
  CChatManager::IsInScheduledMatch
  ====================*/
bool	CChatManager::IsInScheduledMatch() const
{
	map<uint, SScheduledMatchInfo>::const_iterator itScheduledMatch(m_mapScheduledMatchInfo.find(m_uiScheduledMatchID));

	return itScheduledMatch != m_mapScheduledMatchInfo.end();
}


/*====================
  CChatManager::LeaveScheduledMatch
  ====================*/
void	CChatManager::LeaveScheduledMatch(bool bLocalOnly, const tstring& sReason)
{
	// We only enter into this if we have a scheduled match or if the team was full.  We do this in case we never got a group 
	// update so the scheduled match info was not initialized properly, so do this so the UI can handle/cleanup properly.
	if (m_uiScheduledMatchID != 0 || sReason == _T("teamfull"))
	{
		Console << _T("Calling LeaveScheduledMatch, smid: ") << m_uiScheduledMatchID << _T(" local: ") << XtoA(bLocalOnly) << _T(" reason: ") << sReason << newl;

		map<uint, SScheduledMatchInfo>::iterator itScheduledMatch(m_mapScheduledMatchInfo.find(m_uiScheduledMatchID));

		if (itScheduledMatch != m_mapScheduledMatchInfo.end())
		{
			itScheduledMatch->second.uiSecondsTillStart = INVALID_TIME;
			itScheduledMatch->second.uiSecondsTillExpiration = INVALID_TIME;

			for (uint i(0); i < 2; ++i)
			{
				for (uint j(0); j < MAX_GROUP_SIZE; ++j)
				{
					itScheduledMatch->second.aGroupInfo[i][j].Clear();
				}
			}
		}

		m_uiScheduledMatchID = 0;
		m_bSMMapLoaded = false;

		Console << _T("Left SM - ") << (!sReason.empty() ? sReason : _T("NULL")) << newl;

		if (!bLocalOnly)
			ScheduledMatchCommand(_T("leave"));

		ScheduledMatchLeave.Trigger(sReason);

		// Only fire this trigger on the staff spectator's client when they join or leave a scheduled match
		if (m_bSpectatingScheduledMatch)
		{
			m_bSpectatingScheduledMatch = false;
			ScheduledMatchSpectatorInfo.Trigger(XtoA(false));
		}
	}
}


/*====================
  CChatManager::RequestScheduledMatchInfo
  ====================*/
void	CChatManager::RequestScheduledMatchInfo(const wstring& sNickname)
{
	if (!IsConnected())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_SCHEDULED_MATCH_INFO << sNickname;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::RequestScheduledMatchLobbyInfo
  ====================*/
void	CChatManager::RequestScheduledMatchLobbyInfo(uint uiScheduledMatchID)
{
	if (!IsConnected())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_SCHEDULED_MATCH_LOBBY_INFO << uiScheduledMatchID;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::HandleScheduledMatchLobbyInfo
  ====================*/
void	CChatManager::HandleScheduledMatchLobbyInfo(CPacket& pkt)
{
	uint uiScheduledMatchID(pkt.ReadInt());
	wstring sMatchTitle(pkt.ReadWString());

	AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, L"Scheduled Match Lobby Info for \"" + sMatchTitle + L"\" - (smid#" + XtoA(uiScheduledMatchID) + L") :");

	for (uint i(0); i < 2; ++i)
	{
		wstring sMessage;
		uint uiTimeFullAndInLobby(pkt.ReadInt());
		uint uiTimeReadied(pkt.ReadInt());

		if (pkt.HasFaults())
			return;

		if (i == 0)
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, L"Legion: TimeFullAndInLobby = " + XtoA(uiTimeFullAndInLobby) + L", TimeFullAndReadied = " + XtoA(uiTimeReadied));
		else 
			AddIRCChatMessage(CHAT_MESSAGE_SYSTEM, L"Hellbourne: TimeFullAndInLobby = " + XtoA(uiTimeFullAndInLobby) + L", TimeFullAndReadied = " + XtoA(uiTimeReadied));
	}
}


/*====================
  CChatManager::ShouldHideTMMStats
  ====================*/
bool	CChatManager::ShouldHideTMMStats() const
{
	// Try to keep this in kind of in sync with CGameClient::ShouldHideStats()
	if (m_yGameType == TMM_GAME_TYPE_CASUAL || 
		m_yGameType == TMM_GAME_TYPE_MIDWARS || 
		m_yGameType == TMM_GAME_TYPE_RIFTWARS || 
		m_yGameType == TMM_GAME_TYPE_CUSTOM ||
		m_yArrangedMatchType == AM_UNRANKED_MATCHMAKING ||
		m_yArrangedMatchType == AM_MATCHMAKING)
		return true;

	return false;
}


/*====================
  CChatManager::GetChatClientInfo
  ====================*/
wstring	CChatManager::GetChatClientInfo(const wstring& sNickname, const wstring& sParams) const
{
	// This method allows us to query any client info that we have stored locally
	if (sNickname.empty() || sParams.empty())
		return WSNULL;

	bool bFound(false);
	ChatClientMap_cit citChatClient;

	wstring sCleanNickname(LowerString(StripClanTag(sNickname)));

	// Try to lookup their accountID based on their nickname
	map<wstring, uint>::const_iterator citAccountID(m_mapNameToAccountID.find(sCleanNickname));
	if (citAccountID != m_mapNameToAccountID.end())
	{
		citChatClient = m_mapUserList.find(citAccountID->second);
		if (citChatClient != m_mapUserList.end())
		{
			bFound = true;
		}
	}

	// We couldn't find them using the lookup, so iterate over every user we have to see if we can find a match of their name
	if (!bFound)
	{
		for (ChatClientMap_cit cit(m_mapUserList.begin()), citEnd(m_mapUserList.end()); cit != citEnd; ++cit)
		{
			if (LowerString(RemoveClanTag(cit->second.sName)) == sCleanNickname)
			{
				citChatClient = cit;
				bFound = true;
				break;
			}
		}
	}

	if (bFound)
	{
		// We found the client we were looking for, now construct a pipe (|) delimited output string returning the 
		// data and order specified in the params argument
		wstring sOutput;
		const wsvector vParams(TokenizeString(sParams, L'|'));

		for (wsvector_cit cit(vParams.begin()), citEnd(vParams.end()); cit != citEnd; ++cit)
		{
			wstring sParam(Trim(*cit));
			
			if (sParam.empty())
				continue;

			if (sParam == L"name")
				sOutput += citChatClient->second.sName + L'|';
			else if (sParam == L"accountid")
				sOutput += XtoA(GetAccountID()) + L'|';
			else if (sParam == L"matchid")
				sOutput += XtoA(citChatClient->second.uiMatchID) + L'|';
			else if (sParam == L"serveraddressport")
				sOutput += citChatClient->second.sServerAddressPort + L'|';
			else if (sParam == L"gamename")
				sOutput += citChatClient->second.sGameName + L'|';
			else if (sParam == L"clan")
				sOutput += citChatClient->second.sClan + L'|';
			else if (sParam == L"clantag")
				sOutput += citChatClient->second.sClanTag + L'|';
			else if (sParam == L"chatsymbol")
				sOutput += Host.GetChatSymbolTexturePath(citChatClient->second.uiChatSymbol) + L'|';
			else if (sParam == L"chatnamecolortexturepath")
				sOutput += Host.GetChatNameColorTexturePath(citChatClient->second.uiChatNameColor) + L'|';
			else if (sParam == L"chatnamecolorstring")
				sOutput += Host.GetChatNameColorString(citChatClient->second.uiChatNameColor) + L'|';
			else if (sParam == L"chatnamecoloringamestring")
				sOutput += Host.GetChatNameColorIngameString(citChatClient->second.uiChatNameColor) + L'|';
			else if (sParam == L"chatnamecolorsortindex")
				sOutput += Host.GetChatNameColorSortIndex(citChatClient->second.uiChatNameColor) + L'|';
			else if (sParam == L"getaccounticontexturepath")
				sOutput += Host.GetAccountIconTexturePath(citChatClient->second.uiAccountIcon, citChatClient->second.iAccountIconSlot, citChatClient->second.uiAccountID) + L'|';
			else if (sParam == L"chatnameglow")
				sOutput += XtoA(Host.GetChatNameGlow(citChatClient->second.uiChatNameColor)) + L'|';
			else if (sParam == L"chatnameglowcolorstring")
				sOutput += Host.GetChatNameGlowColorString(citChatClient->second.uiChatNameColor) + L'|';
			else if (sParam == L"chatnameglowcoloringamestring")
				sOutput += Host.GetChatNameGlowColorIngameString(citChatClient->second.uiChatNameColor) + L'|';
			else if (sParam == L"chatnamefont")
				sOutput += Host.GetChatNameColorFont(citChatClient->second.uiChatNameColor) + L'|';
			else if (sParam == L"chatnamebackgroundglow")
				sOutput += XtoA(Host.GetChatNameBackgroundGlow(citChatClient->second.uiChatNameColor)) + L'|';
		}

		return sOutput;
	}

	return WSNULL;
}



/*====================
  CChatManager::GetPopularity
  ====================*/
uint CChatManager::GetPopularity(const tstring& sPopularityType, const tstring& sGameType, const tstring& sMapName, const tstring& sGameMode, const tstring& sRegion, bool bRanked) const
{
	// We can lookup the string -> enum conversions through the maps populated in HandleTMMPopularityUpdates()
	ETMMGameTypes eGameType(static_cast<ETMMGameTypes>(AtoI(sGameType)));
	ETMMGameMaps eGameMap(TMM_GAME_MAP_NONE);
	ETMMGameModes eGameMode(TMM_GAME_MODE_NONE);
	ETMMGameRegions eRegion(TMM_GAME_REGION_NONE);

	map<tstring, byte>::const_iterator itMap(m_mapMapLookup.find(sMapName));
	if (itMap != m_mapMapLookup.end())
		eGameMap = static_cast<ETMMGameMaps>(itMap->second);

	map<tstring, byte>::const_iterator itMode(m_mapGameModeLookup.find(sGameMode));
	if (itMode != m_mapGameModeLookup.end())
		eGameMode = static_cast<ETMMGameModes>(itMode->second);

	map<tstring, byte>::const_iterator itRegion(m_mapRegionLookup.find(sRegion));
	if (itRegion != m_mapRegionLookup.end())
		eRegion = static_cast<ETMMGameRegions>(itRegion->second);

	if (CompareNoCase(sPopularityType, L"gametype") == 0 && eGameType != TMM_GAME_TYPE_NONE && eGameMap != TMM_GAME_MAP_NONE)
	{
		return m_TMMPopularities.ayGameType[eGameType][eGameMap][BYTE_BOOL(bRanked)];
	}
	else if (CompareNoCase(sPopularityType, L"gamemap") == 0 && eGameMap != TMM_GAME_MAP_NONE && eGameType != TMM_GAME_TYPE_NONE)
	{
		return m_TMMPopularities.ayGameMap[eGameMap][eGameType][BYTE_BOOL(bRanked)];
	}
	else if (CompareNoCase(sPopularityType, L"gamemode") == 0 && eGameMode != TMM_GAME_MODE_NONE && eGameMap != TMM_GAME_MAP_NONE && eGameType != TMM_GAME_TYPE_NONE)
	{
		return m_TMMPopularities.ayGameMode[eGameMode][eGameMap][eGameType][BYTE_BOOL(bRanked)];
	}
	else if (CompareNoCase(sPopularityType, L"region") == 0 && eRegion != TMM_GAME_REGION_NONE && eGameMap != TMM_GAME_MAP_NONE && eGameType != TMM_GAME_TYPE_NONE)
	{
		return m_TMMPopularities.ayRegion[eRegion][eGameMap][eGameType][BYTE_BOOL(bRanked)];
	}

	return 0;
}


/*====================
  CChatManager::HandleLeaverInfo
  ====================*/
void	CChatManager::HandleLeaverInfo(CPacket& pkt)
{
	int iGames(pkt.ReadInt());
	int iDisconnects(pkt.ReadInt());

	CHostClient* pClient(Host.GetActiveClient());
	if (pClient != NULL)
	{
		pClient->GetAccount().SetGames(iGames);
		pClient->GetAccount().SetDisconnects(iDisconnects);
	}
}


/*====================
  CChatManager::HandleRequestReadyUp
  ====================*/
void	CChatManager::HandleRequestReadyUp(CPacket& pkt)
{
	K2_UNREFERENCED_PARAMETER(pkt);

	TMMRequestReadyUp.Trigger(TSNULL);
}


/*====================
  CChatManager::HandleTMMStartLoading
  ====================*/
void	CChatManager::HandleTMMStartLoading(CPacket& pkt)
{
	K2_UNREFERENCED_PARAMETER(pkt);

	Host.PreloadWorld(m_sTMMMapName);
}


/*====================
  CChatManager::HandleTMMPendingMatch
  ====================*/
bool	CChatManager::HandleTMMPendingMatch(CPacket& pkt)
{
	uint uiAcceptTimeMs(pkt.ReadInt());
	uint uiAcceptTotalTimeMs(pkt.ReadInt());
	uint uiNumPlayers(pkt.ReadInt());
	uint uiNumPlayersAccepted(pkt.ReadInt());
	bool bAcceptedPendingMatch(pkt.ReadByte() != 0);

	if (cc_printPendingMatch)
	{
		Console << L"uiAcceptTimeMs = " << uiAcceptTimeMs << newl
				<< L"uiAcceptTotalTimeMs = " << uiAcceptTotalTimeMs << newl
				<< L"uiNumPlayers = " << uiNumPlayers << newl
				<< L"uiNumPlayersAccepted = " << uiNumPlayersAccepted << newl
				<< L"bAcceptedPendingMatch = " << XtoA(bAcceptedPendingMatch) << newl;
	}

	if (pkt.HasFaults())
		return false;

	m_uiPendingMatchTimeEnd = Host.GetSystemTime() + uiAcceptTimeMs;
	m_uiPendingMatchLength = uiAcceptTotalTimeMs;
	m_uiPendingMatchNumPlayers = uiNumPlayers;
	m_uiPendingMatchNumPlayersAccepted = uiNumPlayersAccepted;
	m_bAcceptedPendingMatch = bAcceptedPendingMatch;

	return true;
}


/*====================
  CChatManager::HandleTMMFailedToAcceptPendingMatch
  ====================*/
void	CChatManager::HandleTMMFailedToAcceptPendingMatch(CPacket& pkt)
{
	tsvector vAccountNames;
	while (true)
	{
		uint uiAccountID(pkt.ReadInt());

		if (pkt.HasFaults())
			return;

		if (uiAccountID == ~0)
			break;

		vAccountNames.push_back(GetAccountNameFromID(uiAccountID));
	}

	tstring sAccountNames(Implode(_T(","), vAccountNames));
	TMMMatchFailedToAccept.Trigger(sAccountNames);
}


/*====================
  CChatManager::RequestSMUpload
  ====================*/
bool	CChatManager::RequestSMUpload(uint uiMatchID, const wstring& sFileExtension)
{
	if (!IsConnected() || !OnDemandReplaysEnabled())
		return false;

	if (man_debugOnDemandUploads)
		Console << L"Requesting upload of M" << uiMatchID << L"." << sFileExtension << L"..." << newl;

	CPacket pktSend;
	pktSend << CHAT_CMD_UPLOAD_REQUEST << uiMatchID << sFileExtension;
	m_sockChat.SendPacket(pktSend);

	return true;
}


/*====================
  CChatManager::HandleUploadStatus
  ====================*/
void	CChatManager::HandleUploadStatus(CPacket& pkt)
{
	uint uiMatchID(pkt.ReadInt());
	EUploadUpdateType eType(static_cast<EUploadUpdateType>(pkt.ReadByte()));

	wstring sDownloadLink;
	if (eType == EUUT_FILE_ALREADY_UPLOADED || eType == EUUT_FILE_UPLOAD_COMPLETE)
		sDownloadLink = pkt.ReadWString();

	if (pkt.HasFaults())
		return;

	static wsvector vParams(3);
	vParams[0] = XtoA(uiMatchID);
	vParams[1] = XtoA(eType);
	vParams[2] = sDownloadLink;

	if (man_debugOnDemandUploads)
		Console << L"vParams[0] = " << vParams[0] << L" vParams[1] = " << vParams[1] << L" vParams[2] = " << vParams[2] << newl;

	UploadReplayStatus.Trigger(vParams);

	wstring sConsoleMessage;

	switch (eType)
	{
		case EUUT_GENERAL_FAILURE:
			sConsoleMessage = L"Failed to download replay....";
			break;
		case EUUT_FILE_DOES_NOT_EXIST:
			sConsoleMessage = L"Unable to download replay, file does not exist on server manager...";
			break;
		case EUUT_FILE_INVALID_HOST:
			sConsoleMessage = L"Invalid host information specified...";
			break;
		case EUUT_FILE_ALREADY_UPLOADED:
			sConsoleMessage = L"Should download existing replay (" + sDownloadLink + L")...";
			break;
		case EUUT_FILE_ALREADY_QUEUED:
			sConsoleMessage = L"Replay is already queued...";
			break;
		case EUUT_FILE_QUEUED:
			sConsoleMessage = L"Replay is queued...";
			break;
		case EUUT_FILE_UPLOADING:
			sConsoleMessage = L"Replay is uploading...";
			break;
		case EUUT_FILE_UPLOAD_COMPLETE:
			sConsoleMessage = L"Replay upload is complete (" + sDownloadLink + L")...";
			PushNotification(NOTIFY_TYPE_REPLAY_AVAILABLE, XtoA(uiMatchID));
			break;
		default:
		case EUUT_NONE:
			sConsoleMessage = L"No upload update type specified...";
	}

	if (man_debugOnDemandUploads)
		Console << sConsoleMessage << newl;
}

/*====================
  CChatManager::HandleOptions
  ====================*/
void	CChatManager::HandleOptions(CPacket& pkt)
{
	bool bOnDemandFTPAvailable(pkt.ReadByte() != 0);
	bool bOnDemandS3Available(pkt.ReadByte() != 0);

	EQuestsAvailabilityType eQuestsAvailability(static_cast<EQuestsAvailabilityType>(pkt.ReadByte()));
	EQuestsAvailabilityType eQuestsLadderAvailability(static_cast<EQuestsAvailabilityType>(pkt.ReadByte()));
	const byte yConnectResendTime(pkt.ReadByte());

	bool bMessagesEnabled(pkt.ReadByte() != 0);

	wstring sDynamicCommands;
	if (CHAT_PROTOCOL_VERSION >= 67)
	{
		sDynamicCommands = pkt.ReadWString();
	}

	if (pkt.HasFaults())
		return;

	CHostClient* pHostClient(Host.GetActiveClient());
	if (pHostClient != NULL)
		pHostClient->SetQuestsDisabled(eQuestsAvailability, eQuestsLadderAvailability);

	m_bOnDemandFTPAvailable = bOnDemandFTPAvailable;
	m_bOnDemandS3Available = bOnDemandS3Available;
	if (yConnectResendTime > 0)
		cl_packetResendTime = yConnectResendTime * 1000;

	SetMessagesEnabled(bMessagesEnabled);

	if (CHAT_PROTOCOL_VERSION >= 67)
	{
		if (!sDynamicCommands.empty())
		{
			wsvector vDynamicCommands(TokenizeString(sDynamicCommands, L'|'));

			for (wsvector_cit cit(vDynamicCommands.begin()), citEnd(vDynamicCommands.end()); cit != citEnd; ++cit)
			{
				wstring sCommand(*cit);
				Console.Execute(sCommand);
			}

			DynamicCommandsExecuted.Trigger(TSNULL);
		}
	}
}

/*====================
  CChatManager::HandleLogout
  ====================*/
void	CChatManager::HandleLogout(CPacket& pkt)
{
	bool bLogOutStaff(pkt.ReadByte() != 0);

	if (pkt.HasFaults())
		return;

	CHostClient* pHostClient(Host.GetActiveClient());
	if (pHostClient == NULL)
		return;

	if(!bLogOutStaff && IsStaff())
		return;

	pHostClient->Logout();
}


/*====================
  CChatManager::HandleNewMessages
  ====================*/
void	CChatManager::HandleNewMessages(CPacket& pkt)
{
	if (pkt.HasFaults())
		return;

	uint uiNumMessages(pkt.ReadInt());
	if (uiNumMessages > 0)
		UpdateMessages();
}

/*====================
  CChatManager::HandleRankedPlayInfo
  ====================*/
void CChatManager::HandleRankedPlayInfo(CPacket& pkt)
{
	if (pkt.HasFaults())
		return;

	m_sNormalRankedPlayInfo.fMMR = pkt.ReadFloat();
	m_sNormalRankedPlayInfo.uiRankLevel = pkt.ReadInt();
	m_sNormalRankedPlayInfo.uiWins = pkt.ReadInt();
	m_sNormalRankedPlayInfo.uiLosses = pkt.ReadInt();
	m_sNormalRankedPlayInfo.uiWinStreaks = pkt.ReadInt();
	m_sNormalRankedPlayInfo.iRanking = pkt.ReadInt();
	m_sNormalRankedPlayInfo.uiPlacementMatches = pkt.ReadInt();
	m_sNormalRankedPlayInfo.sPlacementDetail = pkt.ReadTString();
	m_sCasualRankedPlayInfo.fMMR = pkt.ReadFloat();
	m_sCasualRankedPlayInfo.uiRankLevel = pkt.ReadInt();
	m_sCasualRankedPlayInfo.uiWins = pkt.ReadInt();
	m_sCasualRankedPlayInfo.uiLosses = pkt.ReadInt();
	m_sCasualRankedPlayInfo.uiWinStreaks = pkt.ReadInt();
	m_sCasualRankedPlayInfo.iRanking = pkt.ReadInt();
	m_sCasualRankedPlayInfo.uiPlacementMatches = pkt.ReadInt();
	m_sCasualRankedPlayInfo.sPlacementDetail = pkt.ReadTString();

	m_sNormalRankedPlayInfo.bEligible = (pkt.ReadByte() != 0);
	m_sCasualRankedPlayInfo.bEligible = m_sNormalRankedPlayInfo.bEligible;

	m_sNormalRankedPlayInfo.bSeasonEnd = (pkt.ReadByte() == 0);
	m_sCasualRankedPlayInfo.bSeasonEnd = m_sNormalRankedPlayInfo.bSeasonEnd;

	m_sNormalRankedPlayInfo.uiRankLevel = CLAMP<uint>(m_sNormalRankedPlayInfo.uiRankLevel, RANK_LEVEL_S7_UNKNOWN, RANK_LEVEL_S7_IMMORTAL);
	m_sCasualRankedPlayInfo.uiRankLevel = CLAMP<uint>(m_sCasualRankedPlayInfo.uiRankLevel, RANK_LEVEL_S7_UNKNOWN, RANK_LEVEL_S7_IMMORTAL);

	// pick mode is fixed
	m_sNormalRankedPlayInfo.sPickMode = _T("cp|bp");
	m_sCasualRankedPlayInfo.sPickMode = _T("cp|bp");

#if !defined(K2_STABLE)
	Console << _T("Get NET_CHAT_CL_TMM_CAMPAIGN_STATS Message: ") << newl;
	Console << _T("Normal MMR:") << m_sNormalRankedPlayInfo.fMMR << newl;
	Console << _T("Normal Level:") << m_sNormalRankedPlayInfo.uiRankLevel << newl;
	Console << _T("Normal Placement:") << m_sNormalRankedPlayInfo.uiPlacementMatches << newl;
	Console << _T("Casual MMR:") << m_sCasualRankedPlayInfo.fMMR << newl;
	Console << _T("Casual Level:") << m_sCasualRankedPlayInfo.uiRankLevel << newl;
	Console << _T("Casual Placement:") << m_sCasualRankedPlayInfo.uiPlacementMatches << newl;
	Console << _T("Eligible:") << m_sNormalRankedPlayInfo.bEligible << newl;
	Console << _T("Season End:") << m_sNormalRankedPlayInfo.bSeasonEnd << newl;
#endif

	RankInfoUpdated.Trigger(TSNULL);
}


/*====================
  CChatManager::HandleLeaveStrikeWarning
  ====================*/
void CChatManager::HandleLeaveStrikeWarning(CPacket& pkt)
{
	if (pkt.HasFaults())
		return;
	pkt.ReadInt();
	LeaveStrikeWarning.Trigger(TSNULL);
}
/*====================
  CChatManager::ClearMessages
  ====================*/
void	CChatManager::ClearMessages()
{
	m_listMessages.clear();
	// MessagesUpdated.Trigger(_T("true"));
}


/*====================
  CChatManager::SetMessagesEnabled
  ====================*/
void	CChatManager::SetMessagesEnabled(bool bEnabled)
{
	bool bWasEnabled(m_bMessagesEnabled);
	m_bMessagesEnabled = bEnabled;

	// If we are going from disabled to enabled pull the messages list since the system is now up
	if (m_bMessagesEnabled && !bWasEnabled)
		UpdateMessages();

	MessagesEnabled.Trigger(XtoA(m_bMessagesEnabled));
}


/*====================
  CChatManager::UpdateMessages
  ====================*/
void	CChatManager::UpdateMessages()
{
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;

	pHTTPRequest->SetTargetURL(Host.GetMessageServerAddress() + _T("/message/list/") + XtoA(GetAccountID()));
	pHTTPRequest->AddVariable(L"cookie", m_sCookie);
	pHTTPRequest->AddVariable(L"response-type", L"serialized");
	pHTTPRequest->SendPostRequest();

	SChatRequest* pRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_GET_MESSAGES, TSNULL));
	m_lHTTPRequests.push_back(pRequest);
}


/*====================
  CChatManager::ProcessGetMessagesResponse
  ====================*/
void	CChatManager::ProcessGetMessagesResponse(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	if (!phpResponse.IsValid())
		return;

	if (!phpResponse.GetBool(_U8("success")))
		return;

	if (!phpResponse.HasVar(_U8("data")))
		return;

	m_listMessages.clear();

	const CPHPData* phpMessageList(phpResponse.GetVar(_U8("data")));
	for (size_t sz(0); sz < phpMessageList->GetSize(); sz++)
	{
		const CPHPData* pMessage(phpMessageList->GetVar(sz));
		if (pMessage == NULL)
			continue;

		SMessage message;
		message.sSubject = pMessage->GetTString(_U8("subject"));
		message.sIcon = pMessage->GetTString(_U8("image"));
		message.bRead = pMessage->GetBool(_U8("read"));
		message.uiExpiration = pMessage->GetInteger(_U8("expiration"));
		message.uiSent = pMessage->GetInteger(_U8("sent"));
		message.sCRC = pMessage->GetTString(_U8("crc"));
		message.bDeletable = pMessage->GetBool(_U8("deletable"));

		m_listMessages.push_back(message);
	}

	MessagesUpdated.Trigger(_T("true"));
}


/*====================
  CChatManager::GetMessage
  ====================*/
bool	CChatManager::GetMessage(const tstring& sCRC, uint uiLuaCallback)
{
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return false;

	pHTTPRequest->SetTargetURL(Host.GetMessageServerAddress() + _T("/message/get/") + XtoA(GetAccountID()) + _T("/") + sCRC);
	pHTTPRequest->AddVariable(L"cookie", m_sCookie);
	pHTTPRequest->AddVariable(L"language", host_language);
	pHTTPRequest->AddVariable(L"response-type", L"serialized");
	pHTTPRequest->SendPostRequest();

	SChatRequest* pRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_GET_MESSAGE, uiLuaCallback));
	m_lHTTPRequests.push_back(pRequest);

	return true;
}


/*====================
  CChatManager::ProcessGetMessageResponse
  ====================*/
void	CChatManager::ProcessGetMessageResponse(SChatRequest* pRequest, bool bRequestFailed)
{
	lua_State* L(LuaScriptManager.GetMasterState());
	LuaScriptManager.PushCallback(pRequest->uiTarget);

	if (!bRequestFailed)
	{
		bool bFailed(false);
		const CPHPData phpResponse(pRequest->pRequest->GetResponse());

		if (!phpResponse.IsValid())
			bFailed = true;
		if (!phpResponse.GetBool(_U8("success")))
			bFailed = true;
		if (!phpResponse.HasVar(_U8("data")))
			bFailed = true;

		if (bFailed)
		{
			Lua::PushNil(L);
		}
		else
		{
			const CPHPData* pMessage(phpResponse.GetVar(_U8("data")));
			const tstring& sCRC(pMessage->GetTString(_U8("crc")));

			// Update the message in our list with the given info
			for (MessageList_it it(m_listMessages.begin()); it != m_listMessages.end(); it++)
			{
				if (it->sCRC == sCRC)
				{
					it->bRead = true;
				}
			}

			// Pass information to the UI
			CLuaTable tblMessage(L);
			tblMessage.AddString(pMessage->GetU8String(_U8("body")), "body");
			tblMessage.AddString(pMessage->GetU8String(_U8("subject")), "subject");
			tblMessage.AddString(pMessage->GetU8String(_U8("image")), "icon");
			tblMessage.AddNumber(pMessage->GetInteger(_U8("expiration")), "expiration");
			tblMessage.AddNumber(pMessage->GetInteger(_U8("sent")), "sent");
			tblMessage.AddBoolean(pMessage->GetBool(_U8("read")), "read");
			tblMessage.AddString(u8string(sCRC), "crc");
			tblMessage.AddBoolean(pMessage->GetBool(_U8("deletable")), "deletable");

			CLuaTable tblMetadata(L);
			const CPHPData* pMeta(pMessage->GetVar(_U8("meta")));
			if (pMeta != NULL)
			{
				for (size_t sz(0); sz < pMeta->GetSize(); sz++)
				{
					const CPHPData* pMetaData(pMeta->GetVar(sz));
					if (pMetaData == NULL)
						continue;

					tblMetadata.AddString(pMetaData->GetU8String(), pMetaData->GetKey().c_str());
				}
			}

			tblMessage.AddTable(tblMetadata, "meta");
		}
	}
	else
	{
		Lua::PushNil(L);
	}

	LuaScriptManager.PushGlobalEnvironment();
	Lua::ExecuteStack(L, 1);
	LuaScriptManager.PopGlobalEnvironment();

	LuaScriptManager.DeleteCallback(pRequest->uiTarget);
}


/*====================
  CChatManager::DeleteMessage
  ====================*/
void	CChatManager::DeleteMessage(const tstring& sCRC)
{
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;

	pHTTPRequest->SetTargetURL(Host.GetMessageServerAddress() + _T("/message/delete/") + XtoA(GetAccountID()) + _T("/") + sCRC);
	pHTTPRequest->AddVariable(L"cookie", m_sCookie);
	pHTTPRequest->AddVariable(L"response-type", L"serialized");
	pHTTPRequest->SendPostRequest();

	SChatRequest* pRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_DELETE_MESSAGE, sCRC));
	m_lHTTPRequests.push_back(pRequest);
}


/*====================
  CChatManager::ProcessDeleteMessageResponse
  ====================*/
void	CChatManager::ProcessDeleteMessageResponse(SChatRequest* pRequest)
{
	const tstring& sCRC(pRequest->sTarget);

	tsvector vParams(2);
	vParams[0] = sCRC;			// Message being deleted
	vParams[1] = _T("false");	// Success?

	const CPHPData phpResponse(pRequest->pRequest->GetResponse());
	if (phpResponse.IsValid() && phpResponse.GetBool(_U8("success")))
	{
		vParams[1] = _T("true");

		// Remove the message with the given CRC from the message list
		for (MessageList_it it(m_listMessages.begin()); it != m_listMessages.end(); it++)
		{
			if (sCRC == it->sCRC)
			{
				m_listMessages.erase(it);
				break;
			}
		}
	}

	MessageDeleted.Trigger(vParams);
}


/*====================
  CChatManager::EndMatch
  ====================*/
void	CChatManager::EndMatch(uint uiMatchID, uint uiLosingTeam)
{
	if (!IsConnected() || !IsStaff())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_END_MATCH << uiMatchID << uiLosingTeam;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::ForceGroupMatchup
  ====================*/
void	CChatManager::ForceGroupMatchup(const wstring& sGroup1Nickname, const wstring& sGroup2Nickname)
{
	if (!IsConnected() || !IsStaff())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_FORCE_GROUP_MATCHUP << sGroup1Nickname << sGroup2Nickname;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::SetMatchmakingVersion
  ====================*/
void	CChatManager::SetMatchmakingVersion(const wstring& sMatchmakingVersion)
{
	if (!IsConnected() || !IsStaff())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_SET_MATCHMAKING_VERSION << sMatchmakingVersion;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::BlockPhrase
  ====================*/
void	CChatManager::BlockPhrase(const wstring& sPhrase)
{
	if (!IsConnected() || !IsStaff())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_BLOCK_PHRASE << sPhrase;
	m_sockChat.SendPacket(pktSend);
}


/*====================
  CChatManager::UnblockPhrase
  ====================*/
void	CChatManager::UnblockPhrase(const wstring& sPhrase)
{
	if (!IsConnected() || !IsStaff())
		return;

	CPacket pktSend;
	pktSend << NET_CHAT_CL_UNBLOCK_PHRASE << sPhrase;
	m_sockChat.SendPacket(pktSend);
}

/*====================
  CChatManager::IsValidCafeChannelName
  ====================*/
bool CChatManager::IsValidCafeChannelName(const tstring& sName)
{
	const tstring sPrefix = _T("Cafe");
	const tstring sCompare = sName.substr(0, sPrefix.length());
	return CompareNoCase(sPrefix, sCompare) == 0;
}

/*====================
  CChatManager::IsCafeChannel
  ====================*/
bool CChatManager::IsCafeChannel(const tstring& sName)
{
	if (!IsValidCafeChannelName(sName))
		return false;

	return (m_mapChannelNameToFlag[sName] & CHAT_CHANNEL_FLAG_RESERVED) != 0;
}

/*====================
CChatManager::UpdateSpecialMessages
====================*/
void CChatManager::UpdateSpecialMessages(bool bForcePopup)
{
	CHTTPRequest* pHTTPRequest(m_pHTTPManager->SpawnRequest());
	if (pHTTPRequest == NULL)
		return;
	pHTTPRequest->SetHost(Host.GetMasterServerAddress());
	pHTTPRequest->SetTargetURL(Host.GetMasterServerIP() + m_sRequester + "?f=get_special_messages");
	pHTTPRequest->AddVariable(L"cookie", m_sCookie);
	pHTTPRequest->SendPostRequest();

	SChatRequest* pRequest(K2_NEW(ctx_Net, SChatRequest)(pHTTPRequest, REQUEST_GET_SPECIAL_MESSAGES, TSNULL));
	m_lHTTPRequests.push_back(pRequest);

	m_bSpecialMessagesPopup = bForcePopup;
}

/*====================
CChatManager::ProcessGetSpecialMessagesResponse
====================*/
void CChatManager::ProcessGetSpecialMessagesResponse(SChatRequest* pRequest)
{
	const CPHPData phpResponse(pRequest->pRequest->GetResponse());

	m_listSpecialMessages.clear();

	if (!phpResponse.IsValid() || !phpResponse.HasVar(_U8("messages")))
	{
		SpecialMessagesUpdated.Trigger(XtoA(false));
		return;
	}

	tstring sDate = phpResponse.GetTString(_U8("date"));

	const CPHPData* phpMessageList(phpResponse.GetVar(_U8("messages")));
	for (size_t sz(0); sz < phpMessageList->GetSize(); sz++)
	{
		const CPHPData* pMessage(phpMessageList->GetVar(sz));
		if (pMessage == NULL)
			continue;

		SSpecialMessage message;
		message.uiId = pMessage->GetInteger(_U8("message_id"));
		message.sTitle = pMessage->GetTString(_U8("title"));
		message.sURL = pMessage->GetTString(_U8("url"));
		message.sStartTime = pMessage->GetTString(_U8("start_time"));
		message.sEndTime = pMessage->GetTString(_U8("end_time"));
		message.uiLeftSeconds = pMessage->GetInteger(_U8("left_secs"));
		message.sDate = sDate;
		string sKey = TStringToUTF8(pMessage->GetTString(_U8("title")) + pMessage->GetTString(_U8("url")) + sDate);
		message.sMD5 = MD5Buffer(sKey.c_str(), (uint)sKey.length());

		m_listSpecialMessages.push_back(message);
	}

	SpecialMessagesUpdated.Trigger(XtoA(m_bSpecialMessagesPopup));
}

void CChatManager::RequestRankedPlayInfo()
{
	CPacket pktSend;
	pktSend << NET_CHAT_CL_TMM_CAMPAIGN_STATS;
	m_sockChat.SendPacket(pktSend);	
}


