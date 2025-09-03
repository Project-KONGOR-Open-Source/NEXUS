// (C)2007 S2 Games
// c_chatmanager.h
//
//=============================================================================
#ifndef __C_CHATMANAGER_H__
#define __C_CHATMANAGER_H__

//=============================================================================
// Headers
//=============================================================================
#include "chatserver_protocol.h"

#include "c_widgetreference.h"
#include "c_textbuffer.h"
#include "c_hostclient.h"
#include "c_clientlogin.h"
#include "c_messagesocket.h"
#include "c_date.h"
//=============================================================================

//=============================================================================
// Declarations
//=============================================================================
class CHTTPManager;
class CHTTPRequest;

K2_API EXTERN_CVAR_BOOL(cg_censorChat);
EXTERN_CVAR_BOOL(login_invisible);
EXTERN_ARRAY_CVAR_UINT(cc_mmrConfig);
EXTERN_ARRAY_CVAR_UINT(cc_mmrConfigAfterS6);
//=============================================================================

//=============================================================================
// Definitions
//=============================================================================
static const uint MAX_GROUP_SIZE(5);
static const uint DEFAULT_CHAT_NAME_COLOR_SORT_INDEX(150);

// referal system flags
const byte CHAT_CLIENT_REFERRAL_NEW			(BIT(0));
const byte CHAT_CLIENT_REFERRAL_INACTIVE	(BIT(1));

enum EChatMessageType
{ 
	CHAT_MESSAGE_NONE = -1,

	CHAT_MESSAGE_CLEAR = 0,
	CHAT_MESSAGE_IRC,
	CHAT_MESSAGE_SYSTEM,
	CHAT_MESSAGE_GAME,
	CHAT_MESSAGE_GAME_IMPORTANT,
	CHAT_MESSAGE_TEAM,
	CHAT_MESSAGE_ALL,
	CHAT_MESSAGE_MENTOR,
	CHAT_MESSAGE_ROLL,
	CHAT_MESSAGE_EMOTE,
	CHAT_MESSAGE_GROUP,
	CHAT_MESSAGE_WHISPER,
	CHAT_MESSAGE_WHISPER_BUDDIES,
	CHAT_MESSAGE_CLAN,
	CHAT_MESSAGE_GLOBAL,
	CHAT_MESSAGE_IM,
	CHAT_MESSAGE_SERVER,
	CHAT_MESSAGE_LOCAL,

};

enum EChatIgnoreType
{
	CHAT_IGNORE_NONE,
	CHAT_IGNORE_ENEMY_ALL,
	CHAT_IGNORE_ALL,
	CHAT_IGNORE_TEAM,
	CHAT_IGNORE_EVERYONE,
};

enum ERequestType
{
	REQUEST_DELETE_BUDDY,
	REQUEST_CLAN_PROMOTE,
	REQUEST_CLAN_DEMOTE,
	REQUEST_CLAN_REMOVE,
	REQUEST_ADD_IGNORED_NICK2ID,
	REQUEST_ADD_IGNORED,
	REQUEST_REMOVE_IGNORED,
	REQUEST_ADD_BANNED_NICK2ID,
	REQUEST_ADD_BANNED,
	REQUEST_REMOVE_BANNED,
	REQUEST_GET_BANNED,
	REQUEST_UPDATE_CLAN,
	REQUEST_CHECK_CLAN_NAME,
	REQUEST_COMPLETE_NICK,
	REQUEST_SAVE_CHANNEL,
	REQUEST_REMOVE_CHANNEL,
	REQUEST_SAVE_NOTIFICATION,
	REQUEST_REMOVE_NOTIFICATION,
	REQUEST_REMOVE_ALL_NOTIFICATIONS,
	REQUEST_CHANGE_BUDDY_GROUP,
	REQUEST_GET_MESSAGES,
	REQUEST_GET_MESSAGE,
	REQUEST_DELETE_MESSAGE,
	REQUEST_GET_SPECIAL_MESSAGES,
	
	NUM_REQUEST_TYPES
};

enum ENotifyType	// values hard coded in lua
{
	NOTIFY_TYPE_UNKNOWN,
	NOTIFY_TYPE_BUDDY_ADDER,
	NOTIFY_TYPE_BUDDY_ADDED,
	NOTIFY_TYPE_BUDDY_REMOVER,	// DISABLED
	NOTIFY_TYPE_BUDDY_REMOVED,	// DISABLED
	NOTIFY_TYPE_CLAN_RANK,
	NOTIFY_TYPE_CLAN_ADD,
	NOTIFY_TYPE_CLAN_REMOVE,
	NOTIFY_TYPE_BUDDY_ONLINE,
	NOTIFY_TYPE_BUDDY_LEFT_GAME,
	NOTIFY_TYPE_BUDDY_OFFLINE,
	NOTIFY_TYPE_BUDDY_JOIN_GAME,
	NOTIFY_TYPE_CLAN_ONLINE,
	NOTIFY_TYPE_CLAN_LEFT_GAME,
	NOTIFY_TYPE_CLAN_OFFLINE,
	NOTIFY_TYPE_CLAN_JOIN_GAME,
	NOTIFY_TYPE_CLAN_WHISPER,	// UNUSED
	NOTIFY_TYPE_UPDATE,
	NOTIFY_TYPE_GENERIC,
	NOTIFY_TYPE_IM,
	NOTIFY_TYPE_GAME_INVITE,
	NOTIFY_TYPE_SELF_JOIN_GAME,
	NOTIFY_TYPE_BUDDY_REQUESTED_ADDER,
	NOTIFY_TYPE_BUDDY_REQUESTED_ADDED,
	NOTIFY_TYPE_TMM_GROUP_INVITE,
	NOTIFY_TYPE_ACTIVE_STREAM,
	NOTIFY_TYPE_REPLAY_AVAILABLE,
	NOTIFY_TYPE_CLOUDSAVE_UPLOAD_SUCCESS,
	NOTIFY_TYPE_CLOUDSAVE_UPLOAD_FAIL,
	NOTIFY_TYPE_CLOUDSAVE_DOWNLOAD_SUCCESS,
	NOTIFY_TYPE_CLOUDSAVE_DOWNLOAD_FAIL,

	NUM_NOTIFICATIONS
};

enum
{
	RANK_LEVEL_UNKNOWN,
	RANK_LEVEL_BRONZE5,
	RANK_LEVEL_BRONZE4,
	RANK_LEVEL_BRONZE3,
	RANK_LEVEL_BRONZE2,
	RANK_LEVEL_BRONZE1,
	RANK_LEVEL_SILVER5,
	RANK_LEVEL_SILVER4,
	RANK_LEVEL_SILVER3,
	RANK_LEVEL_SILVER2,
	RANK_LEVEL_SILVER1,
	RANK_LEVEL_GOLD3,
	RANK_LEVEL_GOLD2,
	RANK_LEVEL_GOLD1,
	RANK_LEVEL_DIAMOND2,
	RANK_LEVEL_DIAMOND1,
	RANK_LEVEL_LEGENDARY,
	RANK_LEVEL_IMMORTAL
};

enum
{
	RANK_LEVEL_S7_UNKNOWN,
	RANK_LEVEL_S7_BRONZE5,
	RANK_LEVEL_S7_BRONZE4,
	RANK_LEVEL_S7_BRONZE3,
	RANK_LEVEL_S7_BRONZE2,
	RANK_LEVEL_S7_BRONZE1,
	RANK_LEVEL_S7_SILVER5,
	RANK_LEVEL_S7_SILVER4,
	RANK_LEVEL_S7_SILVER3,
	RANK_LEVEL_S7_SILVER2,
	RANK_LEVEL_S7_SILVER1,
    RANK_LEVEL_S7_GOLD4,
	RANK_LEVEL_S7_GOLD3,
	RANK_LEVEL_S7_GOLD2,
	RANK_LEVEL_S7_GOLD1,
    RANK_LEVEL_S7_DIAMOND3,
	RANK_LEVEL_S7_DIAMOND2,
	RANK_LEVEL_S7_DIAMOND1,
    RANK_LEVEL_S7_LEGENDARY2,
	RANK_LEVEL_S7_LEGENDARY1,
	RANK_LEVEL_S7_IMMORTAL
};

struct SRankedPlayInfo
{
	uint uiRankLevel;
	uint uiWins;
	uint uiLosses;
	uint uiWinStreaks;
	uint uiPlacementMatches;
	tstring sPlacementDetail;
	int iRanking;
	float fMMR;
	tstring sPickMode;
	bool bEligible;
	bool bSeasonEnd;

#if defined(__APPLE__) && defined(__clang__)
    SRankedPlayInfo():uiRankLevel(0), uiWins(0), uiLosses(0), uiWinStreaks(0), uiPlacementMatches(0), iRanking(-1), fMMR(0.0f), sPickMode(_T("")), sPlacementDetail(_T("")), bEligible(true), bSeasonEnd(false){}
#else
    SRankedPlayInfo():uiRankLevel(0), uiWins(0), uiLosses(0), uiWinStreaks(0), uiPlacementMatches(0), iRanking(-1), fMMR(0.0f), sPickMode(TSNULL), sPlacementDetail(TSNULL), bEligible(true), bSeasonEnd(false){}
#endif
};

struct SNotificationInfo
{
	tstring sText;
	tstring sType;
	tstring sAction;
};

static const SNotificationInfo g_NotificationInfo[] =
{
	{ _CTS("notify_unknown"),				_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_buddy_adder"),			_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_buddy_added"),			_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_buddy_remover"),			_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_buddy_removed"),			_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_clan_rank"),				_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_clan_add"),				_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_clan_remove"),			_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_buddy_online"),			_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_buddy_left_game"),		_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_buddy_offline"),			_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_buddy_join_game"),		_CTS("notfication_generic_info_join"),	_CTS("action_joingame"), },
	{ _CTS("notify_clan_online"),			_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_clan_left_game"),		_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_clan_offline"),			_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_clan_join_game"),		_CTS("notfication_generic_info_join"),	_CTS("action_joingame"), },
	{ _CTS("notify_clan_whisper"),			_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_update"),				_CTS("notfication_generic_info"),		_CTS("action_update"), },
	{ _CTS("notify_generic"),				_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_im"),					_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_game_invite"),			_CTS("notfication_generic_info_join"),	_CTS("action_joininvite"), },
	{ _CTS("notify_self_join_game"),		_CTS("notfication_generic_info_join"),	_CTS("action_joingame"), },
	{ _CTS("notify_buddy_requested_adder"),	_CTS("notfication_generic_info"),		_CTS(""), },
	{ _CTS("notify_buddy_requested_added"), _CTS("notfication_generic_action"),		_CTS("action_friend_request"), },
	{ _CTS("notify_group_invite"),			_CTS("notfication_generic_action"),		_CTS("action_groupinvite"), }, 
	{ _CTS("notify_active_stream"),			_CTS("notfication_generic_action"),		_CTS("action_openstream"), }, 
	{ _CTS("notify_replay_available"),		_CTS("notfication_generic_action"),		_CTS("action_openstats"), },
	{ _CTS("notify_cloudsave_upload_success"),	_CTS("notfication_generic_info"),	_CTS(""), },
	{ _CTS("notify_cloudsave_upload_fail"),		_CTS("notfication_generic_info"),	_CTS(""), },
	{ _CTS("notify_cloudsave_download_success"),_CTS("notfication_generic_info"),	_CTS(""), },
	{ _CTS("notify_cloudsave_download_fail"),	_CTS("notfication_generic_info"),	_CTS(""), },
};
assert_compile_time(STATIC_ARRAY_SIZE(g_NotificationInfo) == NUM_NOTIFICATIONS);

static const wstring g_sAdminNames[CHAT_NUM_ADMIN_LEVELS] =
{
	L"chat_admin_level_none",
	L"chat_admin_level_officer",
	L"chat_admin_level_leader",
	L"chat_admin_level_administrator",
	L"chat_admin_level_staff",
};

struct SChatBanned
{
	uint		uiAccountID;
	tstring		sName;
	tstring		sReason;
};

struct SChatClient
{
	uint		uiAccountID;
	uint		uiMatchID;
	tstring		sServerAddressPort;
	tstring		sGameName;
	byte		yFlags;
	byte		yReferralFlags;
	byte		yStatus;
	tstring		sName;
	tstring		sClan;
	tstring		sClanTag;
	int			iClanID;
	uiset		setChannels;
	uint		uiChatSymbol;
	uint		uiChatNameColor;
	uint		uiAccountIcon;
	int			iAccountIconSlot;
	tstring		sBuddyGroup;
	uint		uiSortIndex;
	uint		uiAscensionLevel;

	SChatClient()
	{
		Clear();
	}

	void	Clear()
	{
		uiAccountID = INVALID_ACCOUNT;
		uiMatchID = -1;
		sServerAddressPort.clear();
		sGameName.clear();
		yFlags = 0;
		yReferralFlags = 0;
		yStatus = CHAT_CLIENT_STATUS_DISCONNECTED;
		sName.clear();
		sClan.clear();
		sClanTag.clear();
		iClanID = INVALID_INDEX;
		setChannels.clear();
		uiChatSymbol = INVALID_INDEX;
		uiChatNameColor = INVALID_INDEX;
		uiAccountIcon = INVALID_INDEX;
		iAccountIconSlot = 0;
		sBuddyGroup.clear();
		uiSortIndex = DEFAULT_CHAT_NAME_COLOR_SORT_INDEX;
		uiAscensionLevel = 0;
	}
};

struct SChatRequest
{
	CHTTPRequest*		pRequest;
	ERequestType		eType;
	uint				uiTarget;
	tstring				sTarget;
	tstring				sText;

	SChatRequest(CHTTPRequest* _pRequest, ERequestType _eType, const tstring& _sTarget, const tstring& _sText = TSNULL) :
	pRequest(_pRequest),
	eType(_eType),
	uiTarget(INVALID_ACCOUNT),
	sTarget(_sTarget),
	sText(_sText)
	{
	}

	SChatRequest(CHTTPRequest* _pRequest, ERequestType _eType, uint _uiTarget, const tstring& _sText = TSNULL) :
	pRequest(_pRequest),
	eType(_eType),
	uiTarget(_uiTarget),
	sText(_sText)
	{
	}
};

typedef	map<uint, byte>			ChatAdminMap;
typedef pair<uint, byte>		ChatAdminPair;
typedef	ChatAdminMap::iterator	ChatAdminMap_it;

struct SChatChannel
{
	tstring				sChannelName;
	uint				uiUserCount;
	uint				uiFlags;
	ChatAdminMap		mapAdmins;
	tstring				sTopic;
	bool				bUnread;	// Locally managed boolean
	uint				uiFocusPriority;

	SChatChannel() :
	uiUserCount(0),
	uiFlags(0),
	bUnread(false),
	uiFocusPriority(0)
	{
	}
};

struct SChatChannelInfo
{
	tstring				sName;
	tstring				sLowerName;
	uint				uiUserCount;

	SChatChannelInfo() :
	uiUserCount(0)
	{
	}
};

struct SChatWidget
{
	CWidgetReference	refWidget;
	bool				bGameChat;
	tstring				sChannel;

	SChatWidget(CTextBuffer* pBuffer, bool bIsGameChat, tstring sChannelName) :
	bGameChat(bIsGameChat),
	sChannel(sChannelName)
	{
		refWidget = pBuffer;
	}
};

struct SGroupMemberInfo
{
	uint		uiAccountID;
	tstring		sName;
	uint		uiChatNameColor;
	uint		uiAccountIcon;
	int			iAccountIconSlot;
	tstring		sCountry;
	byte		ySlot;
	ushort		unRating;
	byte		yLoadingPercent;
	byte		yReadyStatus;
	bool		bInGame;
	bool		bEligibleForRanked;
	bool		bFriend;
	bool		bGameModeAccess;
	wstring		sGameModeAccess;
	uint		uiCampaignMedalNormal;
	uint		uiCampaignMedalCasual;
	int			iCampaignRankingNormal;
	int			iCampaignrankingCasual;
	bool		bEligibleForCampaign;

	SGroupMemberInfo()
	{
		Clear();
	}

	void	Clear()
	{
		uiAccountID = INVALID_INDEX;
		sName.clear();
		uiChatNameColor = INVALID_INDEX;
		uiAccountIcon = INVALID_INDEX;
		iAccountIconSlot = 0;
		sCountry.clear();
		ySlot = 0xff;
		unRating = 0;
		yLoadingPercent = 0;
		yReadyStatus = 0;
		bInGame = false;
		bEligibleForRanked = false;
		bFriend = false;
		bGameModeAccess = false;
		uiCampaignMedalNormal = RANK_LEVEL_UNKNOWN;
		uiCampaignMedalCasual = RANK_LEVEL_UNKNOWN;
		iCampaignRankingNormal = -1;
		iCampaignrankingCasual = -1;
		bEligibleForCampaign = false;
	}
};

struct SEventInfo
{
	uint		uiEventID;
	tstring		sEventName;
	CDate		dateStartDate;
	CDate		dateEndDate;

	SEventInfo(uint _uiEventID, const tstring &_sEventName, time_t _tStartDate, time_t _tEndDate) :
	uiEventID(_uiEventID),
	sEventName(_sEventName),
	dateStartDate(CDate(_tStartDate)),
	dateEndDate(CDate(_tEndDate))
	{
	}
};

struct SScheduledMatchInfo
{
	uint		uiEventID;
	uint		uiScheduledMatchID;
	tstring		sMatchTitle;
	CDate		dateStartDate;
	CDate		dateExpirationDate;
	byte		yGameType;
	byte		yTeamSize;
	tstring		sMapName;
	tstring		sGameMode;
	tstring		sRegion;
	byte		yShouldStart;
	uint		uiSecondsTillStart;
	uint		uiSecondsTillExpiration;

	tstring				sTeamName[2];
	vector<SRosterInfo>	vTeam[2];
	SGroupMemberInfo	aGroupInfo[2][MAX_GROUP_SIZE];	

	SScheduledMatchInfo(uint _uiEventID, uint _uiScheduledMatchID, const tstring& sMatchTitle, time_t _tStartDate, time_t _tExpirationDate, 
						byte _yGameType, byte _yTeamSize, const tstring& _sMapName, const tstring& _sGameMode, const tstring& _sRegion,
						const tstring& _sTeamName1, const tstring& _sTeamName2) :
	uiEventID(_uiEventID),
	uiScheduledMatchID(_uiScheduledMatchID),
	sMatchTitle(sMatchTitle),
	dateStartDate(CDate(_tStartDate)),
	dateExpirationDate(CDate(_tExpirationDate)),
	yGameType(_yGameType),
	yTeamSize(_yTeamSize),
	sMapName(_sMapName),
	sGameMode(_sGameMode),
	sRegion(_sRegion),
	yShouldStart(0),
	uiSecondsTillStart(INVALID_TIME),
	uiSecondsTillExpiration(INVALID_TIME)
	{
		sTeamName[0] = _sTeamName1;
		sTeamName[1] = _sTeamName2;
	}
};

struct SMessage {
	tstring		sSubject;
	tstring		sIcon;
	bool		bRead;
	bool		bDeletable;
	uint		uiExpiration;
	uint		uiSent;
	tstring		sCRC;
};

struct SSpecialMessage
{
	uint		uiId;
	tstring		sTitle;
	tstring		sURL;
	tstring		sStartTime;
	tstring		sEndTime;
	tstring		sDate;
	uint		uiLeftSeconds;
	tstring		sMD5;
};

typedef	map<uint, SChatClient> ChatClientMap;
typedef pair<uint, SChatClient> ChatClientPair;
typedef	ChatClientMap::iterator ChatClientMap_it;
typedef	ChatClientMap::const_iterator ChatClientMap_cit;

typedef map<tstring, tsvector>		IMMap;
typedef IMMap::iterator				IMMap_it;

typedef map<tstring, uint>			IMCountMap;
typedef IMCountMap::iterator		IMCountMap_it;

typedef map<uint, tsvector>			NotificationMap;
typedef pair<uint, tsvector>		NotificationPair;
typedef NotificationMap::iterator	NotificationMap_it;

typedef	map<uint, SChatChannel>		ChatChannelMap;
typedef pair<uint, SChatChannel>	ChatChannelPair;
typedef	ChatChannelMap::iterator	ChatChannelMap_it;

typedef	map<uint, SChatBanned>		ChatBanMap;
typedef pair<uint, SChatBanned>		ChatBanPair;
typedef	ChatBanMap::iterator		ChatBanMap_it;

typedef	map<uint, tstring>			ChatIgnoreMap;
typedef pair<uint, tstring>			ChatIgnorePair;
typedef	ChatIgnoreMap::iterator		ChatIgnoreMap_it;

typedef vector<SChatWidget>				ChatWidgetReference;
typedef ChatWidgetReference::iterator	ChatWidgetReference_it;

typedef	map<uint, SChatChannelInfo>		ChatChannelInfoMap;
typedef pair<uint, SChatChannelInfo>	ChatChannelInfoPair;
typedef	ChatChannelInfoMap::iterator	ChatChannelInfoMap_it;

typedef list<SMessage>					MessageList;
typedef list<SMessage>::iterator		MessageList_it;
typedef list<SMessage>::const_iterator	MessageList_cit;

typedef list<SSpecialMessage>					SpecialMessageList;
typedef	list<SSpecialMessage>::iterator			SpecialMessage_it;
typedef list<SSpecialMessage>::const_iterator	SpecialMessage_cit;

//=============================================================================

//=============================================================================
// CChatManager
//=============================================================================
class CChatManager
{
	SINGLETON_DEF(CChatManager)

private:
	typedef list<SChatRequest*>			ChatRequestList;
	typedef ChatRequestList::iterator	ChatRequestList_it;

	CHTTPManager*	m_pHTTPManager;
	string			m_sRequester;

	// Old chat manager functionality
	tstring			m_sCurrentMessage;
	tstring			m_sChatType;


	// New chat manager functionality
	CMessageSocket	m_sockChat;

	CDate			m_cDate;

	uint			m_uiConnectTimeout;
	uint			m_uiNextReconnectTime;
	
	ChatClientMap		m_mapUserList;
	map<wstring, uint>	m_mapNameToAccountID;
	uiset				m_setBuddyList;
	uiset				m_setClanList;
	uiset				m_setCafeList;
	ChatBanMap			m_mapBanList;
	ChatIgnoreMap		m_mapIgnoreList;
	uint				m_uiIgnoreChat;
	bool				m_bFriendlyChat;
	byte				m_yChatModeType;
	map<tstring, byte>  m_mapChannelNameToFlag;

	bool			m_bBuddyUpdateRequired;
	bool			m_bClanUpdateRequired;
	bool			m_bCafeUpdateRequired;

	ChatRequestList	m_lHTTPRequests;

	bool			m_bOnDemandS3Available;
	bool			m_bOnDemandFTPAvailable;
	uint			m_uiConnectRetries;

	tsvector		m_vClanWhispers;

	uint			m_uiAccountID;
	tstring			m_sCookie;
	wstring			m_sRemoteIP;
	wstring			m_sAuthHash;

	sset			m_setAutoJoinChannels;

	ChatChannelMap	m_mapChannels;
	uiset			m_setChannelsIn;

	tsmapts			m_mapCensor;

	ResHandle		m_hStringTable;

	bool			m_bPrivateGame;
	bool			m_bHost;

	bool			m_bWhisperMode;
	list<tstring>	m_lLastWhispers;
	uint			m_uiTabNumber;

	bool			m_bRetrievingStats;

	CXMLDoc*		m_pRecentlyPlayed;

	tsvector		m_vNoteTimes;
	tsvector		m_vNotes;

	uint			m_uiMatchID;
	tstring			m_sGameName;
	byte			m_yArrangedType;

	sset			m_setRecentlyPlayed;

	ChatWidgetReference	m_vWidgets;

	deque<tstring>	m_vChatHistory;
	uint			m_uiHistoryPos;

	uint			m_uiCreateTimeSent;

	tstring			m_sFollowName;
	bool			m_bFollow;

	tstring			m_sChatAddress;

	bool			m_bMatchStarted;
	bool			m_bWaitingToShowStats;
	uint			m_uiShowStatsMatchID;

	uint			m_uiFocusedChannel;
	uint			m_uiFocusCount;
	tstring			m_sFocusedIM;

	IMMap			m_mapIMs;						// Map of all IMs
	IMCountMap		m_mapIMUnreadCount;				// Map of all IM counts
	IMCountMap		m_mapIMFocusPriority;			// IM focus priority

	uint			m_uiLastIMNotificationTime;		// Timestamp of last IM received, to throttle IM notifications
	uint			m_uiReceivedIMCount;			// Total received IMs for current session
	uint			m_uiReadIMCount;				// Total read IMs for current session
	uint			m_uiSentIMCount;				// Total sent IMs for current session

	NotificationMap		m_mapNotifications;			// All the notifications for the client
	uint				m_uiNotificationIndex;		// The client keeps track of all notifications now so this is just a sequence number incremented

	byte			m_unChatSymbol;					// Contains the id of the chat symbol this user has that should override the default symbol
	byte			m_unChatNameColor;				// Contains the id of the chat name color of the players name in chat channels

	friend class CWidgetReference;


	// Local client info
	EChatClientStatus	m_eStatus;
	bool				m_bCanBeReferred;
	
	// TMM info
	uint				m_uiTMMBanTime;
	uint				m_uiTMMStartTime;
	uint				m_uiTMMAverageQueueTime;
	bool				m_bInGroup;						// Is the player in a group
	bool				m_bTMMEnabled;					// Whether or not TMM is enabled
	uint				m_uiTMMGroupLeaderID;
	bool				m_bTMMOtherPlayersReady;
	bool				m_bTMMAllPlayersReady;
	tstring				m_sTMMMapName;
	byte				m_yGameType;					// 1 - Normal, 2 - Casual, 3 - Midwars as seen in HandleTMMPlayerUpdates
	EArrangedMatchType	m_yArrangedMatchType;
	bool				m_bTMMMapLoaded;
	SGroupMemberInfo	m_aGroupInfo[MAX_GROUP_SIZE];
	uint				m_uiTMMSelfGroupIndex;

	wstring				m_sAvailableRegions;			// The regions available
	wstring				m_sRestrictedRegions;			// A list of any regions that are considered restricted by the chat server

	wsvector			m_vDisabledGameModesByGameType;
	wsvector			m_vDisabledGameModesByRankType;
	wsvector			m_vDisabledGameModesByMap;

	byte				m_yGroupSize;					// For looping over aGroupInfo
	wstring				m_sCountry;						// The country the local client is from

	STMMPopularities	m_TMMPopularities;
	map<tstring, byte>	m_mapMapLookup;
	map<tstring, byte>	m_mapGameModeLookup;
	map<tstring, byte>	m_mapRegionLookup;


	uint				m_uiLastPendingMatchUpdate;
	uint				m_uiPendingMatchTimeEnd;
	uint				m_uiPendingMatchLength;
	uint				m_uiPendingMatchNumPlayers;
	uint				m_uiPendingMatchNumPlayersAccepted;
	bool				m_bAcceptedPendingMatch;
	bool				m_bHadPendingMatchLastUpdate;

	ChatChannelInfoMap	m_mapChannelList;
	bool				m_bFinishedList;
	byte				m_yListStartSequence;
	byte				m_yProcessingListSequence;
	byte				m_yFinishedListSequence;

	tstring				m_sProcessingListHead;
	tstring				m_sFinishedListHead;

	bool				m_bInGameLobby;

	uint				m_uiNextTimerUpdateTime;
	byte				m_yAntiAddictionBenefit;

	bool							m_bSMMapLoaded;
	bool							m_bSpectatingScheduledMatch;
	uint							m_uiScheduledMatchID;
	vector<SEventInfo>				m_vEventInfo;
	map<uint, SScheduledMatchInfo>	m_mapScheduledMatchInfo;	

	uint				m_uiChatMuteExpiration;

	MessageList			m_listMessages;
	bool				m_bMessagesEnabled;

	SpecialMessageList m_listSpecialMessages;
	bool			   m_bSpecialMessagesPopup;

	SRankedPlayInfo		m_sNormalRankedPlayInfo;
	SRankedPlayInfo		m_sCasualRankedPlayInfo;

	uint				m_uiMapSettingPhaseEndTime;
	bool				m_bMapSettingChangeTriggered;

	uint				GetAccountIDFromName(const wstring& sName);
	const tstring&		GetAccountNameFromID(uint uiAccountID);

	void				UpdateClientChannelStatus(const tstring& sNewChannel, const tstring& sName, uint uiAccountID, byte yStatus, byte yFlags, uint uiChatSymbol, uint uiChatNameColor, uint uiAccountIcon, int iAccountIconSlot, uint uiAscensionLevel);
	void				UpdateReadyStatus();
	void				PendingMatchFrame();

	void				GetAccountIconInfo(const tstring& sAccountIcon, uint& uiOutAccountIcon, int& iOutAccountIconSlot);

	// Functions to handle data recieved from server
	void				HandlePing();
	void				HandleChannelInfo(CPacket& pkt);
	void				HandleChannelChange(CPacket& pkt);
	void				HandleChannelJoin(CPacket& pkt);
	void				HandleChannelLeave(CPacket& pkt);
	void				HandleChannelMessage(CPacket& pkt);
	void				HandleWhisper(CPacket& pkt);
	void				HandleWhisperBuddies(CPacket& pkt);
	void				HandleWhisperFailed(CPacket& pkt);
	void				HandleIM(CPacket& pkt);
	void				HandleIMFailed(CPacket& pkt);
	void				HandleDisconnect();
	void				HandleInitialStatusUpdate(CPacket& pkt);
	void				HandleStatusUpdate(CPacket& pkt);
	void				HandleClanWhisper(CPacket& pkt);
	void				HandleClanWhisperFailed();
	void				HandleFlooding();
	void				HandleMaxChannels();
	void				HandleServerInvite(CPacket& pkt);
	void				HandleInviteFailedUserNotFound();
	void				HandleInviteFailedNotInGame();
	void				HandleInviteRejected(CPacket& pkt);
	void				HandleUserInfoNoExist(CPacket& pkt);
	void				HandleUserInfoOffline(CPacket& pkt);
	void				HandleUserInfoOnline(CPacket& pkt);
	void				HandleUserInfoInGame(CPacket& pkt);
	void				HandleChannelUpdate(CPacket& pkt);
	void				HandleChannelTopic(CPacket& pkt);
	void				HandleChannelKick(CPacket& pkt);
	void				HandleChannelBan(CPacket& pkt);
	void				HandleChannelUnban(CPacket& pkt);
	void				HandleBannedFromChannel(CPacket& pkt);
	void				HandleChannelSilenced(CPacket& pkt);
	void				HandleChannelSilenceLifted(CPacket& pkt);
	void				HandleSilencePlaced(CPacket& pkt);
	void				HandleMessageAll(CPacket& pkt);
	void				HandleExcessiveGameplayMessage(CPacket& pkt);
	void				HandleMaintenanceMessage(CPacket& pkt);
	void				HandleChannelPromote(CPacket& pkt);
	void				HandleChannelDemote(CPacket& pkt);
	void				HandleAuthAccepted();
	void				HandleRejected(CPacket& pkt);
	void				HandleChannelAuthEnabled(CPacket& pkt);
	void				HandleChannelAuthDisabled(CPacket& pkt);
	void				HandleChannelAddAuthUser(CPacket& pkt);
	void				HandleChannelRemoveAuthUser(CPacket& pkt);
	void				HandleChannelAddAuthUserFailed(CPacket& pkt);
	void				HandleChannelRemoveAuthUserFailed(CPacket& pkt);
	void				HandleChannelListAuth(CPacket& pkt);
	void				HandleChannelSetPassword(CPacket& pkt);
	void				HandleChannelJoinPassword(CPacket& pkt);
	void				HandleClanInvite(CPacket& pkt);
	void				HandleClanInviteRejected(CPacket& pkt);
	void				HandleClanInviteFailedOnline(CPacket& pkt);
	void				HandleClanInviteFailedClan(CPacket& pkt);
	void				HandleClanInviteFailedInvite(CPacket& pkt);
	void				HandleClanInviteFailedPermissions(CPacket& pkt);
	void				HandleClanInviteFailedUnknown(CPacket& pkt);
	void				HandleNewClanMember(CPacket& pkt);
	void				HandleClanRankChanged(CPacket& pkt);
	void				HandleClanCreateFailedClan(CPacket& pkt);
	void				HandleClanCreateFailedInvite(CPacket& pkt);
	void				HandleClanCreateFailedNotFound(CPacket& pkt);
	void				HandleClanCreateFailedDuplicate(CPacket& pkt);
	void				HandleClanCreateFailedParam(CPacket& pkt);
	void				HandleClanCreateFailedClanName(CPacket& pkt);
	void				HandleClanCreateFailedTag(CPacket& pkt);
	void				HandleClanCreateFailedUnknown(CPacket& pkt);
	void				HandleClanCreateAccept(CPacket& pkt);
	void				HandleClanCreateRejected(CPacket& pkt);
	void				HandleClanCreateComplete(CPacket& pkt);
	void				HandleNameChange(CPacket& pkt);
	void				HandleAutoMatchConnect(CPacket& pkt);
	void				HandleServerNotIdle(CPacket& pkt);
	void				HandleAutoMatchStatus(CPacket& pkt);
	void				HandleChatRoll(CPacket& pkt);
	void				HandleChatEmote(CPacket& pkt);
	void				HandleSetChatModeType(CPacket& pkt);
	void				HandleChatModeAutoResponse(CPacket& pkt);
	void				HandleUserCount(CPacket& pkt);
	void				HandleTMMPlayerUpdates(CPacket& pkt);
	void				HandleTMMPopularityUpdates(CPacket& pkt);
	void				HandleTMMQueueUpdates(CPacket& pkt);
	void				HandleTMMJoinQueue(CPacket& pkt);
	void				HandleTMMReJoinQueue(CPacket& pkt);
	void				HandleTMMGenericResponse(CPacket& pkt);
	void				HandleTMMLeaveQueue(CPacket& pkt);
	void				HandleTMMInviteToGroup(CPacket& pkt);
	void				HandleTMMInviteToGroupBroadcast(CPacket& pkt);
	void				HandleTMMRejectInvite(CPacket& pkt);
	void				HandleTMMMatchFound(CPacket& pkt);
	void				HandleTMMJoinFailed(CPacket& pkt);
	void				HandleRequestBuddyAddResponse(CPacket& pkt);
	void				HandleRequestBuddyApproveResponse(CPacket& pkt);
	void				HandleStaffJoinMatchResponse(CPacket& pkt);
	void				HandleChannelInfoSub(CPacket& pkt);
	void				HandleUserStatus(CPacket& pkt);
	void				HandleRequestGameInfo(CPacket& pkt);
	void				HandleTMMRegionUnavailable(CPacket& pkt);
	void				HandleListData(CPacket& pkt);
	void				HandleActiveStreams(CPacket& pkt);
	void				HandlePlayerSpectateRequest(CPacket& pkt);
	void				HandleEventInfo(CPacket& pkt);
	void				HandleScheduledMatchInfo(CPacket& pkt);
	void				HandleScheduledMatchUpdates(CPacket& pkt);
	void				HandleScheduledMatchFound(CPacket& pkt);
 	void				HandleTMMPlayerBotUpdates(CPacket& pkt);
	void				HandleTMMPlayerBots(CPacket& pkt);
	void				HandleTMMStartLocalBotMatch();
	void				HandleTMMTeamNoBotsSelected();
	void				HandleScheduledMatchLobbyInfo(CPacket& pkt);
	void				HandleLeaverInfo(CPacket& pkt);
	void				HandleRequestReadyUp(CPacket& pkt);
	void				HandleTMMStartLoading(CPacket& pkt);
	bool				HandleTMMPendingMatch(CPacket& pkt);
	void				HandleTMMFailedToAcceptPendingMatch(CPacket& pkt);
	void				HandleUploadStatus(CPacket& pkt);
	void				HandleOptions(CPacket& pkt);
	void				HandleLogout(CPacket& pkt);
	void				HandleNewMessages(CPacket& pkt);
	void				HandleRankedPlayInfo(CPacket& pkt);
	void				HandleLeaveStrikeWarning(CPacket& pkt);

	void				ProcessFailedRequest(SChatRequest* pRequest);
	void				ProcessRemoveBuddySuccess(SChatRequest* pRequest);
	void				ProcessClanPromoteSuccess(SChatRequest* pRequest);
	void				ProcessClanDemoteSuccess(SChatRequest* pRequest);
	void				ProcessClanRemoveSuccess(SChatRequest* pRequest);
	void				ProcessClanUpdateSuccess(SChatRequest* pRequest);
	void				ProcessClanNameCheckSuccess(SChatRequest* pRequest);
	void				ProcessBanLookupIDSuccess(SChatRequest* pRequest);
	void				ProcessAddBanSuccess(SChatRequest* pRequest);
	void				ProcessRemoveBanSuccess(SChatRequest* pRequest);
	void				ProcessIgnoreLookupIDSuccess(SChatRequest* pRequest);
	void				ProcessIgnoreAddSuccess(SChatRequest* pRequest);
	void				ProcessIgnoreRemoveSuccess(SChatRequest* pRequest);
	void				ProcessCompleteNickSuccess(SChatRequest* pRequest);
	void				ProcessSaveChannelSuccess(SChatRequest* pRequest);
	void				ProcessRemoveChannelSuccess(SChatRequest* pRequest);
	void				ProcessSaveNotificationResponse(SChatRequest* pRequest);
	void				ProcessRemoveNotificationResponse(SChatRequest* pRequest);
	void				ProcessRemoveAllNotificationsResponse(SChatRequest* pRequest);
	void				ProcessGetMessagesResponse(SChatRequest* pRequest);
	void				ProcessGetMessageResponse(SChatRequest* pRequest, bool bRequestFailed = false);
	void				ProcessDeleteMessageResponse(SChatRequest* pRequest);
	void				ProcessGetSpecialMessagesResponse(SChatRequest* pRequest);

public:
	~CChatManager();

	// Old chat manager functionality
	K2_API void			AddIRCChatMessage(EChatMessageType eType, const tstring& sMessage = TSNULL, const tstring& sChannel = TSNULL, bool bTimeStamp = false, bool bSelfMessage = false);
	K2_API void			AddGameChatMessage(EChatMessageType eType, const tstring& sMessage = TSNULL, const tstring& sHeroEntityName = TSNULL,  bool bSelfMessage = false, const tstring& sSound = TSNULL);

	K2_API tstring		SetCurrentChatMessage(const tstring& sMessage);
	K2_API tstring		GetCurrentChatMessage()								{ return m_sCurrentMessage; }

	K2_API tstring		TabChatMessage(const tstring& sMessage);

	K2_API void			SetCurrentChatType(const tstring& sType)			{ m_sChatType = sType; }
	K2_API tstring		GetCurrentChatType()								{ return m_sChatType; }

	// New chat manager functionality, to handle custom chat server
	void				Init(CHTTPManager* pHTTPManager);

	void				SetAddress(const tstring& sAddress)		{ m_sChatAddress = sAddress; }

	void				Frame();
	void				UpdateTimers();
	void				Connect(bool bInvisible = false);
	void				Disconnect();

	uint				GetAccountID() const								{ return m_uiAccountID; }
	uint				GetConnectRetries() const							{ return m_uiConnectRetries; }

	void				SetInfo(int iAccountID, const tstring& sCookie, const tstring& sNickname, const tstring& sClan, const tstring& sClanTag, int iClanID, EClanRank eClanRank, byte yFlags, uint uiChatSymbol, uint uiChatNameColor, uint uiAccountIcon, int iAccountIconSlot, const wstring& sRemoteIP, const wstring& sAuthHash);

	bool				GetLoginInvisible() const							{ return login_invisible; }
	bool				IsChatMuted() const									{ return K2System.GetUnixTimestamp() < m_uiChatMuteExpiration; }
	void				SetChatMuteExpiration(uint uiChatMuteExpiration)	{ m_uiChatMuteExpiration = K2System.GetUnixTimestamp() + uiChatMuteExpiration; }
	uint				GetChatMuteExpiration() const						{ return (IsChatMuted() ? m_uiChatMuteExpiration - K2System.GetUnixTimestamp() : 0); }

	void				SetPrivateGame(bool bPrivateGame)					{ m_bPrivateGame = bPrivateGame; }
	bool				GetPrivateGame() const								{ return m_bPrivateGame; }

	void				SetHost(bool bHost)									{ m_bHost = bHost; }
	bool				GetHost() const										{ return m_bHost; }
	
	tstring				GetCountry() const									{ return m_sCountry; }

	void				ConnectingFrame();
	void				AuthFrame();
	void				ConnectedFrame();

	void				RequestFrame();

	bool				ProcessData(CPacket& pkt);

	void				AddBuddy(uint uiAccountID, const tstring& sName, byte yFlags = 0, byte yReferralFlags = 0, const tstring& sBuddyGroup = TSNULL);
	void				AddClanMember(uint uiAccountID, const tstring& sName, byte yFlags = 0, byte yReferralFlags = 0);
	void				AddBan(uint uiAccountID, const tstring& sName, const tstring& sReason);
	void				AddIgnore(uint uiAccountID, const tstring& sName);

	void				RemoveBuddy(uint uiAccountID);
	inline void			RemoveBuddy(const tstring& sName)					{ RemoveBuddy(GetAccountIDFromName(sName)); }

	void				RemoveClanMember(uint uiAccountID);
	void				RemoveClanMember(const tstring& sName);
	void				RemoveBan(uint uiAccountID);
	void				RemoveBan(const tstring& sName);
	void				RemoveIgnore(uint uiAccountID);
	void				RemoveIgnore(const tstring& sName);

	void				ClearBuddyList()									{ m_setBuddyList.clear(); }
	void				ClearClanList()										{ m_setClanList.clear(); }
	void				ClearCafeList()										{ m_setCafeList.clear(); }
	void				ClearBanList()										{ m_mapBanList.clear(); }
	void				ClearIgnoreList()									{ m_mapIgnoreList.clear(); }

	void				GetBanList();

	void				RequestBuddyAdd(const tstring& sName);
	void				RequestBuddyApprove(const tstring& sName);

	void				RequestBuddyRemove(uint uiAccountID);
	inline void			RequestBuddyRemove(const tstring& sName)			{ RequestBuddyRemove(GetAccountIDFromName(sName)); }

	void				RequestBanlistAdd(const tstring& sName, const tstring& sReason);
	void				RequestBanlistRemove(uint uiAccountID);
	inline void			RequestBanlistRemove(const tstring& sName)			{ RequestBanlistRemove(GetAccountIDFromName(sName)); }

	void				RequestIgnoreAdd(const tstring& sName);
	void				RequestIgnoreAdd(uint uiAccountID)					{ RequestIgnoreAdd(GetAccountNameFromID(uiAccountID)); }

	void				RequestIgnoreRemove(uint uiAccountID);
	void				RequestIgnoreRemove(const tstring& sName)			{ RequestIgnoreRemove(GetAccountIDFromName(sName)); }

	void				UpdateUserList(uint uiChannelID);
	void				UpdateBuddyList();
	void				UpdateClanList();
	void				UpdateCompanionList();

	void				UpdateRecentlyPlayed();

	inline byte			GetStatus() const								{ return byte(m_eStatus); }

	bool				IsUserInGame(const tstring& sUser);
	bool				IsUserInCurrentGame(const tstring& sUser);
	bool				IsUserOnline(const tstring& sUser);

	bool				SendChannelMessage(const tstring& sMessage, uint uiChannelID, uint eChatMessageType = CHAT_MESSAGE_IRC);
	bool				SendWhisper(const tstring& sName, const tstring& sMessage);
	bool				SendIM(const tstring& sName, const tstring& sMessage);
	bool				SendClanWhisper(const tstring& sMessage);

	K2_API bool			SubmitChatMessage(const tstring& sMessage, uint uiChannelID);

	void				UpdateWhispers(const tstring& sName);
	void				UpdateClanWhispers();
	void				UpdateLookingForClan();

	void				RequestPromoteClanMember(const tstring& sName);
	void				RequestDemoteClanMember(const tstring& sName);
	void				RequestRemoveClanMember(const tstring& sName);

	void				InviteToClan(const tstring& sName);

	void				AddToRecentlyPlayed(const tstring& sName);

	bool				UpdateHoverInfo(const tstring& sName);

	inline bool			IsConnected() const									{ return m_eStatus >= CHAT_CLIENT_STATUS_CONNECTED; }
	inline bool			IsConnecting() const								{ return m_eStatus > CHAT_CLIENT_STATUS_DISCONNECTED && m_eStatus < CHAT_CLIENT_STATUS_CONNECTED; }

	inline void			RefreshBuddyList()
	{
		m_bBuddyUpdateRequired = true;
	}
	inline void			RefreshClanList()
	{
		m_bClanUpdateRequired = true;
	}
	inline void			RefreshCafeList()
	{
		m_bCafeUpdateRequired = true;
	}

	void				SetCanBeReferred (bool bCanBeReferred ) { m_bCanBeReferred =  bCanBeReferred ; }
	bool				GetCanBeReferred () { return m_bCanBeReferred ; }

	const tstring&		GetChannelName(uint uiChannelID);
	uint				GetChannelID(const tstring& sName);

	void				ClearAutoJoinChannels()								{ m_setAutoJoinChannels.clear(); }
	bool				IsSavedChannel(const tstring& sChannel);
	void				SaveChannelLocal(const tstring& sChannel);
	void				RemoveChannelLocal(const tstring& sChannel);
	void				RemoveChannelsLocal();
	void				SaveChannel(const tstring& sChannel);
	void				RemoveChannel(const tstring& sChannel, bool bRemoveAll = false);
	void				JoinChannel(const tstring& sChannel);
	void				JoinChannel(const tstring& sChannel, const tstring& sPassword);
	void				JoinStreamChannel(const tstring& sChannel);
	void				LeaveChannel(const tstring& sChannel);
	void				RequestChannelList();
	void				RequestChannelSublist(const tstring& sHead);
	void				ChannelSublistCancel();

	bool				IsFollowing(const tstring& sName);
	K2_API tstring		GetFollowing();
	K2_API void			UnFollow();
	void				UpdateFollow(const wstring& sServer);
	bool				SetFollowing(const tstring& sName);

	void				InviteUser(const tstring& sName);

	K2_API bool			IsBuddy(const tstring& sName);
	K2_API bool			IsBuddy(uint uiAccountID);
	K2_API bool			IsClanMember(const tstring& sName);
	K2_API bool			IsClanMember(uint uiAccountID);
	K2_API bool         IsCafeMember(const tstring& sName);
	K2_API bool			IsCafeMember(uint uiAccountID);
	K2_API bool			IsCompanion(uint uiAccountID)						{ return IsBuddy(uiAccountID) || IsClanMember(uiAccountID) || IsCafeMember(uiAccountID); }
	K2_API bool			IsCompanion(const tstring& sName)					{ return IsBuddy(sName) || IsClanMember(sName) || IsCafeMember(sName); }
	K2_API bool			IsBanned(const tstring& sName);
	K2_API bool			IsBanned(uint uiAccountID);
	K2_API bool			IsIgnored(const tstring& sName);
	K2_API bool			IsIgnored(uint uiAccountID);
	K2_API uint			GetIgnoreChat() const								{ return m_uiIgnoreChat; }
	K2_API void			SetIgnoreChat(uint uiIgnoreChat)					{ m_uiIgnoreChat = uiIgnoreChat; }
	K2_API bool			GetFriendlyChat() const								{ return m_bFriendlyChat; }
	K2_API void			SetFriendlyChat(bool bFriendlyChat)					{ m_bFriendlyChat = bFriendlyChat; }

	K2_API bool			OnDemandFTPAvailable() const						{ return m_bOnDemandFTPAvailable; }
	K2_API bool			OnDemandS3Available() const							{ return m_bOnDemandS3Available; }
	K2_API bool			OnDemandReplaysEnabled() const						{ return m_bOnDemandFTPAvailable || m_bOnDemandS3Available; }

	K2_API bool			IsAccountInactive(const tstring& sName);
	K2_API bool			IsAccountInactive(uint uiAccountID);

	bool				IsValidCafeChannelName(const tstring& sName);
	bool				IsCafeChannel(const tstring& sName);
	byte				GetChatModeType() const								{ return m_yChatModeType; }
	void				SetChatModeType(byte yChatModeType, const tstring& sReason, bool bSetDefaultMode = false);

	bool				IsInAClan(const tstring& sName);
	bool				IsInAClan(uint uiAccountID);

	bool				HasFlags(const tstring& sName, byte yFlags);
	bool				HasFlags(uint uiAccountID, byte yFlags);

	K2_API tstring		GetBanReason(const tstring& sName);
	K2_API tstring		GetBanReason(uint uiAccountID);

	K2_API void			PlaySound(const tstring& sSoundName);

	K2_API void			SendServerInvite(int iAccountID);
	K2_API void			SendServerInvite(const tstring& sName);
	K2_API void			RejectServerInvite(int iAccountID);

	void				GetUserInfo(const tstring& sName);
	void				RequestUserStatus(const tstring& sName);

	K2_API tstring		Translate(const tstring& sKey, const tstring& sParamName1 = TSNULL, const tstring& sParamValue1 = TSNULL, const tstring& sParamName2 = TSNULL, const tstring& sParamValue2 = TSNULL, const tstring& sParamName3 = TSNULL, const tstring& sParamValue3 = TSNULL, const tstring& sParamName4 = TSNULL, const tstring& sParamValue4 = TSNULL);
	K2_API tstring		Translate(const tstring& sKey, const tsmapts& mapParams);

	void				UpdateChannels();
	void				UpdateChannel(uint uiChannelID);
	void				RebuildChannels();

	void				AutoCompleteNick(const tstring& sName);
	void				AutoCompleteClear();

	uint				GetLocalAccountID()								{ return m_uiAccountID; }

	void				AddWidgetReference(CTextBuffer* pBuffer, bool bIsGameChat, tstring sChannelName);

	void				SetRetrievingStats(bool bValue)					{ m_bRetrievingStats = bValue; }
	bool				IsRetrievingStats()								{ return m_bRetrievingStats; }

	bool				IsAdmin(uint uiChannelID, uint uiAccountID, EAdminLevel eMinLevel = CHAT_CLIENT_ADMIN_OFFICER);
	bool				IsAdmin(uint uiChannelID, const tstring& sName, EAdminLevel eMinLevel = CHAT_CLIENT_ADMIN_OFFICER);
	EAdminLevel			GetAdminLevel(uint uiChannelID, uint uiAccountID);
	EAdminLevel			GetAdminLevel(uint uiChannelID, const tstring& sName);

	void				SetChannelTopic(uint uiChannel, const tstring& sTopic);
	void				KickUserFromChannel(uint uiChannel, const tstring& sUser);
	void				BanUserFromChannel(uint uiChannel, const tstring& sUser);
	void				UnbanUserFromChannel(uint uiChannel, const tstring& sUser);
	void				SilenceChannelUser(uint uiChannel, const tstring& sUser, uint uiDuration);
	void				PromoteUserInChannel(uint uiChannelID, uint uiAccountID);
	void				PromoteUserInChannel(uint uiChannelID, const tstring& sName);
	void				DemoteUserInChannel(uint uiChannelID, uint uiAccountID);
	void				DemoteUserInChannel(uint uiChannelID, const tstring& sName);

	void				RequestAuthEnable(uint uiChannelID);
	void				RequestAuthDisable(uint uiChannelID);
	void				RequestAuthAdd(uint uiChannelID, const tstring& sName);
	void				RequestAuthRemove(uint uiChannelID, const tstring& sName);
	void				RequestAuthList(uint uiChannelID);

	void				SetChannelPassword(uint uiChannelID, const tstring& sPassword);

	void				SendGlobalMessage(byte yType, uint uiValue, const tstring& sMessage);

	void				SaveNotes();

	void				AdminKick(const tstring& sName, uint uiSecondsToBan = 0);
	void				EndMatch(uint uiMatchID, uint uiLosingTeam = 0);
	void				ForceGroupMatchup(const wstring& uiGroup1ID, const wstring& uiGroup2ID);
	void				SetMatchmakingVersion(const wstring& sMatchmakingVersion);
	void				BlockPhrase(const wstring& sPhrase);
	void				UnblockPhrase(const wstring& sPhrase);

	void				PreviousHistory()					{ if (m_uiHistoryPos < m_vChatHistory.size()) m_uiHistoryPos++; }
	void				NextHistory()						{ if (m_uiHistoryPos > 0) m_uiHistoryPos--; }
	tstring				GetCurrentChatHistory();
	void				AddChatHistory(const tstring& sChat)	{ m_uiHistoryPos = 0; m_vChatHistory.push_front(sChat); }

	void				AcceptClanInvite();
	void				RejectClanInvite();

	void				CreateClan(const tstring& sName, const tstring& sTag, const tstring& sMember1, const tstring& sMember2, const tstring& sMember3, const tstring& sMember4);

	static bool			CompareNames(const tstring& sOrig, const tstring& sName);
	bool				CompareNames(uint uiAccountID, const tstring& sName);

	void				CheckClanName(const tstring& sName, const tstring& sTag);

	tstring				RemoveClanTag(const tstring& sName) const;
	bool				HasClanTag(const tstring& sName) const;

	K2_API void			ShowPostGameStats(uint uiMatchID);

	void				AddUnreadChannel(uint uiChannelID);
	void				RemoveUnreadChannel(uint uiChannelID);
	K2_API void			SetFocusedChannel(const tstring& sChannel, bool bForceFocus = false);
	K2_API void			SetFocusedChannel(uint uiChannel, bool bForceFocus = false);
	void				SetNextFocusedChannel();
	void				SetFocusedIM(const tstring& sName);
	void				SetNextFocusedIM();
	const tstring&		GetFocusedIM()				{ return m_sFocusedIM; }
	void				CloseIM(const tstring& sName);

	uint				GetReceivedIMCount()		{ return m_uiReceivedIMCount; }
	uint				GetReadIMCount()			{ return m_uiReadIMCount; }
	uint				GetUnreadIMCount()			{ int uiIMCount = 0; for (IMCountMap_it it(m_mapIMUnreadCount.begin()); it != m_mapIMUnreadCount.end(); ++it) { uiIMCount += it->second; } return uiIMCount; }
	uint				GetSentIMCount()			{ return m_uiSentIMCount; }
	uint				GetOpenIMCount()			{ return (uint)m_mapIMs.size(); }

	void				ResetTabCounter()			{ m_uiTabNumber = 0; }

	uint				AddReceivedIM()							{ return ++m_uiReceivedIMCount; }
	uint				AddReadIM()								{ return ++m_uiReadIMCount; }
	uint				AddReadIM(uint uiCount)					{ return m_uiReadIMCount += uiCount; }
	uint				AddUnreadIM(const tstring& sName);
	uint				AddSentIM()								{ return ++m_uiSentIMCount; }
	uint				RemoveUnreadIMs(const tstring& sName);

	void				ClearNotifications();
	void				PushNotification(byte yType, const tstring& sParam1 = TSNULL, const tstring& sParam2 = TSNULL, const tstring& sParam3 = TSNULL, const tsvector& vParam4 = VSNULL, uint uiExternalNotificationID = 0, bool bSilent = false, const tstring& sNotificationTime = TSNULL);	// pushes the notification to the client interface via triggers
	void				AddNotification(uint uiIndex, const tsvector& vParam)
	{
		m_mapNotifications.insert(NotificationPair(uiIndex, vParam));
	}
	void				RemoveNotification(uint uiIndex);	// tries to remove the notification locally, requests a DB removal if it's a stored notification
	void				RemoveExternalNotification(uint uiIndex);	// after getting a response back from DB, this is called to remove the stored notification locally

	uint				GetNotificationIndex()					{ return m_uiNotificationIndex; }
	uint				IncrementNotificationIndex()			{ return ++m_uiNotificationIndex; }
	uint				GetNotificationCount()					{ return (uint)m_mapNotifications.size(); }

	void				RequestSaveNotification(byte yType, const tstring& sParam1 = TSNULL, const tstring& sParam2 = TSNULL, const tstring& sParam3 = TSNULL, const tsvector vParam4 = VSNULL);  // tries to save notification to the DB
	void				RequestRemoveNotification(uint uiInternalNotificationID, uint uiExternalNotificationID);  // tries to remove notification from the DB based on it's external ID
	void				RequestRemoveAllNotifications();  // tries to request that all notifications from the DB and clear out the list when getting a response back
	void				ParseNotification(const tstring& sNotification, uint uiExternalNotificationID = 0, bool bSilent = false);  // parses the delimited notification string

	// Functions to handle TMM stuff
	K2_API void		CreateTMMGroup(byte yTMMType = 1, byte yGameType = 1, const tstring& sMapName = _T("caldavar"), const tstring& sGameModes = _T("ap|sd|bd|bp|ar"), const tstring& sRegions = _T("USE|USW|EU"), bool bRanked = false, byte yMatchFidelity = 0, byte yBotDifficulty = 1, bool bRandomizeBots = false);
	K2_API bool		JoinTMMGroup(const wstring& sNickname);
	K2_API void		LeaveTMMGroup(bool bLocalOnly = false, const tstring& sReason = TSNULL, uint uiValue = 0);
	K2_API void		InviteToTMMGroup(const wstring& sNickname);
	K2_API void		JoinTMMQueue();
	K2_API void		LeaveTMMQueue();
	K2_API void		RejectTMMInvite(const wstring& sNickname);
	K2_API void		KickFromTMMGroup(byte ySlotNumber);
	K2_API void		SendTMMGroupOptionsUpdate(byte yGameType, const tstring& sMapName, const tstring& sGameModes, const tstring& sRegions, bool bRanked, byte yMatchFidelity, byte yBotDifficulty, bool bRandomizeBots);
	K2_API void		RequestTMMPopularityUpdate();
	K2_API void		SendTMMPlayerLoadingUpdate(byte yPercent);
	K2_API void		SendTMMPlayerReadyStatus(byte yReadyStatus, byte yGameType);
	K2_API bool		IsInQueue();
	K2_API bool		IsInGroup();
	K2_API bool		IsTMMEnabled();
	K2_API uint		GetGroupLeaderID();
	K2_API bool		GetOtherPlayersReady();
	K2_API bool		GetAllPlayersReady();
	K2_API bool		CanPlayerAccessRegion(const wstring& sRegion);
	K2_API bool		CanGroupPlayerAccessRegion(const wstring& sNickName, const wstring& sRegion);
	K2_API bool		CanGroupAccessRegion(const wstring& sRegion);
	K2_API bool		IsEnabledGameMode(const wstring& sGameType, const wstring& sMapName, const wstring& sGameMode, const wstring& sRanked) const;
	K2_API void		SendTMMGroupEnemyBotUpdate(byte yBotSlot, const tstring& sBot);
	K2_API void		SendTMMGroupTeamBotUpdate(byte yBotSlot, const tstring& sBot);
	K2_API void		SendTMMChangeGroupType(byte yType);
	K2_API void		SendTMMSwapGroupType();
	K2_API void		AcceptPendingMatch();
	K2_API void		MatchmakingConnect(const wstring& sAddress, ushort unPort, EArrangedMatchType eMatchType);

	// Functions to handle game joining/leaving
	K2_API bool			JoinGame(const tstring& sName, bool bPlayerSpectate = false, bool bStaffSpectate = false);
	K2_API bool			SpectatePlayer(const tstring& sName, bool bMentor);
	K2_API void			JoiningGame(const tstring& sAddr);
	K2_API void			FinishedJoiningGame(const tstring& sName, uint uiMatchID, byte yArrangedType = 0, bool bJoinChatChannel = true);
	K2_API void			LeftGame();
	K2_API void			MatchStarted();
	K2_API void			ResetGame();
	K2_API void			LeaveMatchChannels();
	bool				IsWaitingToShowStats() const			{ return m_bWaitingToShowStats; }
	K2_API bool			HasServerInfo(const tstring& sName) const;
	
	void				ClearWaitingToShowStats()
	{
		m_bWaitingToShowStats = false;
		m_uiShowStatsMatchID = INVALID_INDEX;
	}
	
	uint				GetShowStatsMatchID() const				{ return m_uiShowStatsMatchID; }

	K2_API void			JoinGameLobby(bool bAllConnected);
	void				LeaveGameLobby();
	
	bool				IsInChannel(uint uiChannelID);

	K2_API void			InitCensor();
	K2_API bool			CensorChat(tstring& sMessage, bool bInGameChat = true);

	void				RequestRefreshUpgrades();
	
	void				RequestGameInfo(const wstring sNickname);
	
	void				RequestChangeBuddyGroup(const wstring& sBuddyName, const wstring& sBuddyGroup);
	void				ProcessChangeBuddyGroupResponse(SChatRequest* pRequest);
	const wstring&		GetBuddyGroup(uint uiAccountID);
	const wstring&		GetBuddyGroup(const wstring& sBuddyName);

	void				SendAction(EActionCampaigns eType);
	void				ScheduledMatchCommand(const wstring& sCommand, uint uiValue = 0);
	bool				AreAllPlayersReady(uint uiScheduledMatchID, int iTeam, bool bIncludeTeamCaptain = true) const;
	bool				IsInScheduledMatch() const;
	bool				IsStaffSpecatingScheduledMatch() const		{ return m_bSpectatingScheduledMatch; }
	void				LeaveScheduledMatch(bool bLocalOnly, const tstring& sReason = TSNULL);
	void				RequestScheduledMatchInfo(const wstring& sNickname);
	void				RequestScheduledMatchLobbyInfo(uint uiScheduledMatchID);
	bool				RequestSMUpload(uint uiMatchID, const wstring& sFileExtension);
	bool				ShouldHideTMMStats() const;
	wstring				GetChatClientInfo(const wstring& sNickname, const wstring& sParams) const;
	uint				GetPopularity(const tstring& sPopularityType, const tstring& sGameType, const tstring& sMapName, const tstring& sGameMode, const tstring& sRegion, bool bRanked) const;
	byte				GetAntiAddictionBenefit() const				{ return m_yAntiAddictionBenefit; }

	bool				IsVIP() const;

	K2_API bool			IsStaff() const;

	void				SetMessagesEnabled(bool bEnabled);
	bool				GetMessagesEnabled() const			{ return m_bMessagesEnabled; }
	void				UpdateMessages();
	void				ClearMessages();
	const MessageList&	GetMessages() const					{ return m_listMessages; };
	uint				GetNumMessages() const				{ return static_cast<uint>(m_listMessages.size()); }
	bool				GetMessage(const tstring& sCRC, uint uiLuaCallback);
	void				DeleteMessage(const tstring& sCRC);

	void				UpdateSpecialMessages(bool bForcePopup);
	const SpecialMessageList&	GetSpecialMessages() const { return	m_listSpecialMessages; }


	void				RequestRankedPlayInfo();
	SRankedPlayInfo&    GetNormalRankedPlayInfo() { return m_sNormalRankedPlayInfo; }
	SRankedPlayInfo&    GetCasualRankedPlayInfo() { return m_sCasualRankedPlayInfo; }
};
//=============================================================================

extern K2_API CChatManager* pChatManager;
#define ChatManager (*pChatManager)

#endif //__C_CHATMANAGER_H__
