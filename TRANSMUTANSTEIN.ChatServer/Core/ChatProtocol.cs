namespace TRANSMUTANSTEIN.ChatServer.Core;

public class ChatProtocol
{
    // TODO: Clean Up From https://github.com/shawwn/hon/blob/f1aa2dfb7d07c447e930aa36f571e547714f4a57/lib/k2public/chatserver_protocol.h

    public const ushort NET_CHAT_CL_CONNECT = 0x0C00;
    public const ushort NET_CHAT_CL_TMM_CAMPAIGN_STATS = 0x0F07;
    public const ushort NET_CHAT_CL_ACCEPT = 0x1C00;

    /*
       // (C)2010 S2 Games
       // chatserver_protocol.h
       //
       //=============================================================================
       #ifndef __CHATSERVER_PROTOCOL_H__
       #define __CHATSERVER_PROTOCOL_H__

       //=============================================================================
       // Definitions
       //=============================================================================
       const uint CHAT_PROTOCOL_VERSION(68);

       const ushort CHAT_CMD_CHANNEL_MSG				(0x03);				// Used when a user messages a channel
       const ushort CHAT_CMD_CHANGED_CHANNEL			(0x04);				// Used when we change channels
       const ushort CHAT_CMD_JOINED_CHANNEL			(0x05);				// Used when a new user joins our channel
       const ushort CHAT_CMD_LEFT_CHANNEL				(0x06);				// Used when a user leaves our channel
       const ushort CHAT_CMD_DISCONNECTED				(0x07);				// Used when we get disconnected
       const ushort CHAT_CMD_WHISPER					(0x08);				// Used when one user whispers another
       const ushort CHAT_CMD_WHISPER_FAILED			(0x09);				// Used when the whisper target could not be found
       const ushort CHAT_CMD_LAST_KNOWN_GAME_SERVER	(0x0A);				// Return the last known game server for myself
       const ushort CHAT_CMD_INITIAL_STATUS			(0x0B);				// Sent on connect to update buddy and clan connection status for new client
       const ushort CHAT_CMD_UPDATE_STATUS				(0x0C);				// Sent on connect to update buddy and clan connection status for old clients
       const ushort CHAT_CMD_REQUEST_BUDDY_ADD			(0x0D);				// Sent from client to chat server to request a buddy add
       const ushort CHAT_CMD_NOTIFY_BUDDY_REMOVE		(0x0E);				// Sent from client to chat server to notify a buddy has been removed
       const ushort CHAT_CMD_JOINING_GAME				(0x0F);				// Sent when a user starts joining a game
       const ushort CHAT_CMD_JOINED_GAME				(0x10);				// Sent when a user finishes joining a game
       const ushort CHAT_CMD_LEFT_GAME					(0x11);				// Sent when a user leaves a game
       //const ushort EMPTY (0x12);
       const ushort CHAT_CMD_CLAN_WHISPER				(0x13);				// Sent when whispering an entire clan
       const ushort CHAT_CMD_CLAN_WHISPER_FAILED		(0x14);				// Sent when a whisper to a clan fails
       const ushort CHAT_CMD_CLAN_PROMOTE_NOTIFY		(0x15);				// Sent with notification keys for the server to verify on clan promotion
       const ushort CHAT_CMD_CLAN_DEMOTE_NOTIFY		(0x16);				// Sent with notification keys for the server to verify on clan demotion
       const ushort CHAT_CMD_CLAN_REMOVE_NOTIFY		(0x17);				// Sent with notification keys for the server to verify on clan removal
       //const ushort EMPTY			(0x18);
       //const ushort EMPTY		(0x19);
       //const ushort EMPTY (0x1A);
       const ushort CHAT_CMD_FLOODING					(0x1B);				// Warning to user that their message wasn't sent due to flood control
       const ushort CHAT_CMD_IM						(0x1C);				// Used when a user recieves/sends an IM through the CC panel
       const ushort CHAT_CMD_IM_FAILED					(0x1D);				// Used when a user fails to send an IM
       const ushort CHAT_CMD_JOIN_CHANNEL				(0x1E);				// Sent by user when joining a new channel
       const ushort CHAT_CMD_DYANMIC_PRODUCT_LIST		(0x1F);				// Dynamic products that are updated in the chat server heartbeat then sent to the client when changed.
       const ushort CHAT_CMD_WHISPER_BUDDIES			(0x20);				// Sending whisper to all buddies
       const ushort CHAT_CMD_MAX_CHANNELS				(0x21);				// Error sent when user has joined max. # of channels
       const ushort CHAT_CMD_LEAVE_CHANNEL				(0x22);				// Sent by user when leaving a channel
       const ushort CHAT_CMD_INVITE_USER_ID			(0x23);				// Sent by game server to invite a user to a game by account ID
       const ushort CHAT_CMD_INVITE_USER_NAME			(0x24);				// Sent by game server to invite a user to a game by account name
       const ushort CHAT_CMD_INVITED_TO_SERVER			(0x25);				// Sent by chat server to notify a user of a pending server invite
       const ushort CHAT_CMD_INVITE_FAILED_USER		(0x26);				// Notifies a user that their invite request failed because the target was not found
       const ushort CHAT_CMD_INVITE_FAILED_GAME		(0x27);				// Notifies a user that their invite request failed because they are not in a game
       const ushort CHAT_CMD_INVITE_REJECTED			(0x28);				// Indicates that a recieved invite was rejected
       //const ushort EMPTY (0x29);
       const ushort CHAT_CMD_USER_INFO					(0x2A);				// Returns information on a user
       const ushort CHAT_CMD_USER_INFO_NO_EXIST		(0x2B);				// The requested user does not exist
       const ushort CHAT_CMD_USER_INFO_OFFLINE			(0x2C);				// Returns information on an offline user
       const ushort CHAT_CMD_USER_INFO_ONLINE			(0x2D);				// Returns information on an online user
       const ushort CHAT_CMD_USER_INFO_IN_GAME			(0x2E);				// Returns information on a user in a game
       const ushort CHAT_CMD_CHANNEL_UPDATE			(0x2F);				// Update channel information
       const ushort CHAT_CMD_CHANNEL_TOPIC				(0x30);				// Set/get channel topic
       const ushort CHAT_CMD_CHANNEL_KICK				(0x31);				// Kick user from channel
       const ushort CHAT_CMD_CHANNEL_BAN				(0x32);				// Ban user from channel
       const ushort CHAT_CMD_CHANNEL_UNBAN				(0x33);				// Unban user from channel
       const ushort CHAT_CMD_CHANNEL_IS_BANNED			(0x34);				// User is banned from channel
       const ushort CHAT_CMD_CHANNEL_SILENCED			(0x35);				// User is silenced in this channel
       const ushort CHAT_CMD_CHANNEL_SILENCE_LIFTED	(0x36);				// User is no longer silenced in a channel
       const ushort CHAT_CMD_CHANNEL_SILENCE_PLACED	(0x37);				// User is now silenced in a channel
       const ushort CHAT_CMD_CHANNEL_SILENCE_USER		(0x38);				// Request to silence a user in a channel
       const ushort CHAT_CMD_MESSAGE_ALL				(0x39);				// Administrator message to all users
       const ushort CHAT_CMD_CHANNEL_PROMOTE			(0x3A);				// Request to promote a user in a channel
       const ushort CHAT_CMD_CHANNEL_DEMOTE			(0x3B);				// Request to demote a user in a channel
       //const ushort EMPTY(0x3C);
       //const ushort EMPTY(0x3D);
       const ushort CHAT_CMD_CHANNEL_SET_AUTH			(0x3E);				// User wants to enable authorization on a channel
       const ushort CHAT_CMD_CHANNEL_REMOVE_AUTH		(0x3F);				// User wants to disable authorization on a channel
       const ushort CHAT_CMD_CHANNEL_ADD_AUTH_USER		(0x40);				// User wants to add a user to the authorization list for a channel
       const ushort CHAT_CMD_CHANNEL_REMOVE_AUTH_USER	(0x41);				// User wants to remove a user from the authorization list for a channel
       const ushort CHAT_CMD_CHANNEL_LIST_AUTH			(0x42);				// User wants to get the authorization list for a channel
       const ushort CHAT_CMD_CHANNEL_SET_PASSWORD		(0x43);				// User wants to set the password for a channel
       const ushort CHAT_CMD_CHANNEL_ADD_AUTH_FAIL		(0x44);				// Failed to add the user to the channel authorization list
       const ushort CHAT_CMD_CHANNEL_REMOVE_AUTH_FAIL	(0x45);				// Failed to remove the user from the channel authorization list
       const ushort CHAT_CMD_JOIN_CHANNEL_PASSWORD		(0x46);				// Channel join with password
       const ushort CHAT_CMD_CLAN_ADD_MEMBER			(0x47);				// Request to add a new clan member
       const ushort CHAT_CMD_CLAN_ADD_REJECTED			(0x48);				// Request to add a member was rejected
       const ushort CHAT_CMD_CLAN_ADD_FAIL_ONLINE		(0x49);				// Request to add a member failed, user was not online
       const ushort CHAT_CMD_CLAN_ADD_FAIL_CLAN		(0x4A);				// Request to add a member failed, user is in a clan
       const ushort CHAT_CMD_CLAN_ADD_FAIL_INVITED		(0x4B);				// Request to add a member failed, user has already been invited
       const ushort CHAT_CMD_CLAN_ADD_FAIL_PERMS		(0x4C);				// Request to add a member failed, user does not have proper permissions
       const ushort CHAT_CMD_CLAN_ADD_FAIL_UNKNOWN		(0x4D);				// Request to add a member failed
       const ushort CHAT_CMD_NEW_CLAN_MEMBER			(0x4E);				// New user added to clan
       const ushort CHAT_CMD_CLAN_ADD_ACCEPTED			(0x4F);				// Request to add a member was accepted
       const ushort CHAT_CMD_CLAN_RANK_CHANGE			(0x50);				// Clan member's rank changed
       const ushort CHAT_CMD_CLAN_CREATE_REQUEST		(0x51);				// Create clan request
       const ushort CHAT_CMD_CLAN_CREATE_ACCEPT		(0x52);				// One of the founding members accepted the request
       const ushort CHAT_CMD_CLAN_CREATE_REJECT		(0x53);				// One of the founding members rejected the request
       const ushort CHAT_CMD_CLAN_CREATE_COMPLETE		(0x54);				// Clan creation completed successfully
       const ushort CHAT_CMD_CLAN_CREATE_FAIL_CLAN		(0x55);				// Clan creation failed, one or more users are already in a clan
       const ushort CHAT_CMD_CLAN_CREATE_FAIL_INVITE	(0x56);				// Clan creation failed, one or more users have an outstanding clan invitation
       const ushort CHAT_CMD_CLAN_CREATE_FAIL_FIND		(0x57);				// Clan creation failed, one or more users could not be found
       const ushort CHAT_CMD_CLAN_CREATE_FAIL_DUPE		(0x58);				// Clan creation failed, duplicate founding members
       const ushort CHAT_CMD_CLAN_CREATE_FAIL_PARAM	(0x59);				// Clan creation failed, one or more parameters are invalid
       const ushort CHAT_CMD_NAME_CHANGE				(0x5A);				// A user's name has changed
       const ushort CHAT_CMD_CLAN_CREATE_FAIL_NAME		(0x5B);				// Clan creation failed, name invalid
       const ushort CHAT_CMD_CLAN_CREATE_FAIL_TAG		(0x5C);				// Clan creation failed, tag invalid
       const ushort CHAT_CMD_CLAN_CREATE_FAIL_UNKNOWN	(0x5D);				// Clan creation failed, unknown error

       const ushort CHAT_CMD_AUTO_MATCH_CONNECT		(0x62);				// The match is ready and the client should connect

       const ushort CHAT_CMD_CHAT_ROLL					(0x64);				// The user just rolled
       const ushort CHAT_CMD_CHAT_EMOTE				(0x65);				// The user just emoted
       const ushort CHAT_CMD_SET_CHAT_MODE_TYPE		(0x66);				// Sets the chat mode type
       const ushort CHAT_CMD_CHAT_MODE_AUTO_RESPONSE	(0x67);				// Used for sending an auto response message

       const ushort CHAT_CMD_PLAYER_COUNT				(0x68);				// Reports user counts to players periodically
       const ushort CHAT_CMD_SERVER_NOT_IDLE			(0x69);				// Server was not idle
       const ushort CHAT_CMD_ACTIVE_STREAMS			(0x6a);				// Active stream list

       const ushort CHAT_CMD_REQUEST_BUDDY_ADD_RESPONSE		(0xb2);
       const ushort CHAT_CMD_REQUEST_BUDDY_APPROVE				(0xb3);
       const ushort CHAT_CMD_REQUEST_BUDDY_APPROVE_RESPONSE	(0xb4);

       const ushort CHAT_CMD_REQUEST_GAME_INFO				(0xb5);
       const ushort CHAT_CMD_LIST_DATA						(0xb6);
       const ushort CHAT_CMD_JOIN_STREAM_CHANNEL			(0xb7);
       const ushort CHAT_CMD_PLAYER_SPECTATE_REQUEST		(0xb8);
       const ushort CHAT_CMD_TRACK_PLAYER_ACTION			(0xb9);
       const ushort CHAT_CMD_STAFF_JOIN_MATCH_REQUEST		(0xba);
       const ushort CHAT_CMD_STAFF_JOIN_MATCH_RESPONSE		(0xbb);
       const ushort CHAT_CMD_EXCESSIVE_GAMEPLAY_MESSAGE	(0xbc);
       const ushort CHAT_CMD_MAINTENANCE_MESSAGE			(0xbd);
       const ushort CHAT_CMD_UPLOAD_REQUEST				(0xbe);
       const ushort CHAT_CMD_UPLOAD_STATUS					(0xbf);
       const ushort CHAT_CMD_OPTIONS						(0xc0);			// Send clients options controlled by the chat server
       const ushort CHAT_CMD_LOGOUT						(0xc1);
       const ushort CHAT_CMD_NEW_MESSAGES					(0xc2);			// The user has recieved new messages and should pull their message list

       //
       // General
       //

       // Bi-directional
       const ushort NET_CHAT_PING						(0x2a00);
       const ushort NET_CHAT_PONG						(0x2a01);

       //
       // Client
       //

       // Client -> Chat Server
       const ushort NET_CHAT_CL_CONNECT				(0x0c00);			// Client requesting connection
       const ushort NET_CHAT_CL_GET_CHANNEL_LIST		(0x0c01);			// Client requests a list of channels
       const ushort NET_CHAT_CL_CHANNEL_LIST_ACK		(0x0c02);			// HACK: until TCP connections are handled properly
       const ushort NET_CHAT_CL_GET_CHANNEL_SUBLIST	(0x0c03);			// Client requests a sub-list of channels (for auto-complete)
       const ushort NET_CHAT_CL_CHANNEL_SUBLIST_ACK	(0x0c04);			// HACK: until TCP connections are handled properly
       const ushort NET_CHAT_CL_GET_USER_STATUS		(0x0c05);			// Client requesting status of a specific user
       //const ushort EMPTY(0x0c06);
       //const ushort EMPTY(0x0c07);
       const ushort NET_CHAT_CL_ADMIN_KICK				(0x0c08);			// Admin request to disconnect target client from chat server
       const ushort NET_CHAT_CL_REFRESH_UPGRADES		(0x0c09);			// Client is requesting an upgrade refresh for itself
       const ushort NET_CHAT_CL_END_MATCH				(0x0c1b);			// Client is requesting to end match
       const ushort NET_CHAT_CL_FORCE_GROUP_MATCHUP	(0x0c1c);			// Client is requesting to force a group into a match
       const ushort NET_CHAT_CL_SET_MATCHMAKING_VERSION(0x0c1d);			// Client is requesting to change matchmaking version
       const ushort NET_CHAT_CL_BLOCK_PHRASE			(0x0c1e);			// Client is requesting to block a phrase
       const ushort NET_CHAT_CL_UNBLOCK_PHRASE			(0x0c1f);			// Client is requesting to unblock a phrase


       //client lobby stuffs
       const ushort NET_CHAT_CL_GAME_LOBBY_SELECT_HERO	(0x0c10);
       const ushort NET_CHAT_CL_GAME_LOBBY_READY		(0x0c11);
       const ushort NET_CHAT_CL_GAME_LOBBY_CREATE		(0x0c12);
       const ushort NET_CHAT_CL_GAME_LOBBY_JOIN       	(0x0c13);			//player request to join the lobby
       const ushort NET_CHAT_CL_GAME_LOBBY_INVITE		(0x0c14);
       const ushort NET_CHAT_CL_GAME_LOBBY_REJECT_INVITE	(0x0c15);
       const ushort NET_CHAT_CL_GAME_LOBBY_ACCEPT_INVITE	(0x0c16);
       const ushort NET_CHAT_CL_GAME_LOBBY_KICK		(0x0c17);			//bidirectional, tell a client that he's kcked, or tell a server to kick someone
       const ushort NET_CHAT_CL_GAME_LOBBY_CHANGE_SLOT	(0x0c18);
       const ushort NET_CHAT_CL_GAME_LOBBY_REQUEST_LIST (0x0c19);
       const ushort NET_CHAT_CL_GAME_LOBBY_RETURN		 (0x0c1a);


       // Bi-directional stuff related to TMM
       const ushort NET_CHAT_CL_TMM_GROUP_CREATE					(0x0c0a);		// Client is requesting a new group be created
       const ushort NET_CHAT_CL_TMM_GROUP_JOIN						(0x0c0b);		// Client is joining a group
       const ushort NET_CHAT_CL_TMM_GROUP_LEAVE					(0x0c0c);		// Client is leaving a group
       const ushort NET_CHAT_CL_TMM_GROUP_INVITE					(0x0c0d);		// Client would like to invite someone to the group
       const ushort NET_CHAT_CL_TMM_GROUP_INVITE_BROADCAST			(0x0c0e);		// Broadcast that a client would like to invite someone to the group
       const ushort NET_CHAT_CL_TMM_GROUP_REJECT_INVITE			(0x0c0f);		// Client rejected invite
       const ushort NET_CHAT_CL_CONNECT_TEST						(0x0c10);
       const ushort NET_CHAT_CL_TMM_GROUP_KICK						(0x0d00);		// The leader requested to kick a group member
       const ushort NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE				(0x0d01);		// The group leader wants to join the queue for a match
       const ushort NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE				(0x0d02);		// The group leader wants to leave the join match queue
       const ushort NET_CHAT_CL_TMM_GROUP_UPDATE					(0x0d03);		// Updates that occur whenever something in the group is updated
       const ushort NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS	(0x0d04);		// Send updates on loading status
       const ushort NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS		(0x0d05);		// Send updates on whether or not the player is ready
       const ushort NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE				(0x0d06);		// Send information on the queue times to the group
       const ushort NET_CHAT_CL_TMM_POPULARITY_UPDATE				(0x0d07);		// Send information on the popularities to all the groups
       const ushort NET_CHAT_CL_TMM_GAME_OPTION_UPDATE				(0x0d08);		// Send group option updates to players when the group leader changes them
       const ushort NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE				(0x0d09);		// Send team a match info update when a match is found
       const ushort NET_CHAT_CL_TMM_SCHEDULED_MATCH_INFO			(0x0e01);		// Used to send scheduled match info to the clients
       const ushort NET_CHAT_CL_TMM_SCHEDULED_MATCH_UPDATE			(0x0e02);		// Used to send specific scheduled match info to the clients
       const ushort NET_CHAT_CL_TMM_SCHEDULED_MATCH_COMMAND		(0x0e03);		// Used to send commands from the client to the chat server
       const ushort NET_CHAT_CL_TMM_SCHEDULED_MATCH_SERVER_INFO	(0x0e04);		// Send team a match info update when a match is found
       const ushort NET_CHAT_CL_TMM_BOT_SPAWN_LOCAL_MATCH			(0x0e05);
       const ushort NET_CHAT_CL_TMM_SWAP_GROUP_TYPE				(0x0e06);
       const ushort NET_CHAT_CL_TMM_BOT_GROUP_UPDATE				(0x0e07);
       const ushort NET_CHAT_CL_TMM_BOT_GROUP_BOTS					(0x0e08);
       const ushort NET_CHAT_CL_TMM_BOT_NO_BOTS_SELECTED			(0x0e09);
       const ushort NET_CHAT_CL_TMM_FAILED_TO_JOIN					(0x0e0a);		// Either TMM is disabled or they were not allowed to join due to being a leaver, being banned, or is not verified
       const ushort NET_CHAT_CL_TMM_REGION_UNAVAILABLE				(0x0e0b);		// One of the regions the group leader selected is unavailable to one of the group members
       const ushort NET_CHAT_CL_TMM_GROUP_REJOIN_QUEUE				(0x0e0c);		// Notify the group if they have been re-placed into the queue at their previous wait time
       const ushort NET_CHAT_CL_TMM_GENERIC_RESPONSE				(0x0e0d);		// Used to send back generic responses back to the clients
       const ushort NET_CHAT_CL_TMM_EVENTS_INFO					(0x0e0f);		// Used to send event info to the clients
       const ushort NET_CHAT_CL_TMM_SCHEDULED_MATCH_LOBBY_INFO		(0x0f00);		// For relaying to tournament admins details about the length of time each team was in the lobby and readied up
       const ushort NET_CHAT_CL_TMM_LEAVER_INFO					(0x0f01);		// If a player is unable to join due to being a leaver, send the client this data so the UI can display it to them
       const ushort NET_CHAT_CL_TMM_REQUEST_READY_UP				(0x0f02);		// Group leader requests group members to ready up
       const ushort NET_CHAT_CL_TMM_START_LOADING					(0x0f03);		// All group members are ready - load!
       const ushort NET_CHAT_CL_TMM_PENDING_MATCH					(0x0f04);		// A match is waiting for your group
       const ushort NET_CHAT_CL_TMM_ACCEPT_PENDING_MATCH			(0x0f05);		// A player has accepted the pending match
       const ushort NET_CHAT_CL_TMM_FAILED_TO_ACCEPT_PENDING_MATCH	(0x0f06);		// A player in your group failed to accept the pending match
       const ushort NET_CHAT_CL_TMM_CAMPAIGN_STATS					(0x0f07);		// Player's campaign stats
       const ushort NET_CHAT_CL_TMM_CHANGE_GROUP_TYPE				(0x0f08);
       const ushort NET_CHAT_CL_TMM_LEAVER_STRIKE_WARN				(0x0f09);		// Notify client that this user needs Leaver Strike popup warning

       // Chat Server -> Client
       const ushort NET_CHAT_CL_ACCEPT						(0x1c00);			// Accept connection from client
       const ushort NET_CHAT_CL_REJECT						(0x1c01);			// Refuse connection from client
       const ushort NET_CHAT_CL_CHANNEL_INFO				(0x1c02);			// Basic information about a channel
       const ushort NET_CHAT_CL_CHANNEL_LIST_SYN			(0x1c03);			// HACK: until TCP connections are handled properly
       const ushort NET_CHAT_CL_CHANNEL_SUBLIST_START		(0x1c04);			// Start of a channel sub-list
       const ushort NET_CHAT_CL_CHANNEL_INFO_SUB			(0x1c05);			// Basic information about a channel in a sublist
       const ushort NET_CHAT_CL_CHANNEL_SUBLIST_SYN		(0x1c06);			// HACK: until TCP connections are handled properly
       const ushort NET_CHAT_CL_CHANNEL_SUBLIST_END		(0x1c07);			// End of a channel sub-list
       const ushort NET_CHAT_CL_USER_STATUS				(0x1c08);			// User status request reponse
       const ushort NET_CHAT_CL_GAME_LOBBY_JOINED			(0x1c09);
       const ushort NET_CHAT_CL_GAME_LOBBY_LEFT			(0x1c0a);
       const ushort NET_CHAT_CL_GAME_LOBBY_UPDATE			(0x1c0b);
       const ushort NET_CHAT_CL_GAME_LOBBY_PLAYER_JOINED	(0x1c0c);
       const ushort NET_CHAT_CL_GAME_LOBBY_PLAYER_LEFT		(0x1c0d);
       const ushort NET_CHAT_CL_GAME_LOBBY_PLAYER_UPDATE	(0x1c0e);
       const ushort NET_CHAT_CL_GAME_LOBBY_LAUNCH_GAME		(0x1c0f);
       const ushort NET_CHAT_CL_GAME_LOBBY_LIST			(0x1c10);
       const ushort NET_CHAT_CL_GAME_LOBBY_FULL			(0x1c11);
       const ushort NET_CHAT_CL_GAME_LOBBY_SPEC_READY		(0x1c12);			// Tell the spectator others are in hero picking mode


       // Game Server -> Chat Server
       const ushort NET_CHAT_GS_CONNECT					(0x0500);			// Game server requesting connection
       const ushort NET_CHAT_GS_DISCONNECT					(0x0501);			// Game server disconnecting
       const ushort NET_CHAT_GS_STATUS						(0x0502);			// Game server's current status
       const ushort NET_CHAT_GS_ANNOUNCE_MATCH				(0x0503);			// An arranged match is ready for clients
       const ushort NET_CHAT_GS_ABANDON_MATCH				(0x0504);			// An arranged match failed to start
       const ushort NET_CHAT_GS_MATCH_STARTED				(0x0505);			// An arranged match has started successfully (all clients in match and banning/picking phase starting)
       const ushort NET_CHAT_GS_REMIND_PLAYER				(0x0506);			// An expected player has not yet connected to an arranged match
       // const ushort EMPTY								(0x0507);
       const ushort NET_CHAT_GS_NOT_IDLE					(0x0508);			// Server was not idle
       const ushort NET_CHAT_GS_MATCH_ABORTED				(0x0509);			// An arranged match failed to start
       const ushort NET_CHAT_GS_SAVE_DISCONNECT_REASON		(0x0510);			// For tracking reasons we are aborting matches due to disconnected players
       const ushort NET_CHAT_GS_REPORT_MISSING_PLAYERS		(0x0511);			// For tracking potential problem players abusing the MM system and causing games fail to start
       const ushort NET_CHAT_GS_MATCH_ID_RESULT			(0x0512);			// Game server letting the chat server know if we got a match ID
       const ushort NET_CHAT_GS_CLIENT_AUTH_RESULT			(0x0513);			// Was this client able to auth with the master server?
       const ushort NET_CHAT_GS_STAT_SUBMISSION_RESULT		(0x0514);			// What result did the server receive when attempting to submit stats?
       const ushort NET_CHAT_GS_MATCH_ENDED				(0x0515);			// An arranged match has finished (stats have been submitted - or at least attempted to submit them)
       const ushort NET_CHAT_GS_MATCH_ONGOING				(0x0516);			// A match is a few minutes in and the game server is just letting the chatserver know
       const ushort NET_CHAT_GS_PLAYER_BENEFITS			(0x0517);			// Game server requests the benefits for a set of clients, chat server responds back with the benefits for each client
       const ushort NET_CHAT_GS_REPORT_LEAVER				(0x0518);			// For tracking leaver players

       // Chat Server -> Game Server
       const ushort NET_CHAT_GS_ACCEPT					(0x1500);			// Accept connection from game server
       const ushort NET_CHAT_GS_REJECT					(0x1501);			// Refuse connection from game server
       const ushort NET_CHAT_GS_CREATE_MATCH			(0x1502);			// Game server has been selected to host an arranged match
       const ushort NET_CHAT_GS_END_MATCH				(0x1503);			// Game server has been told to end a match
       const ushort NET_CHAT_GS_REMOTE_COMMAND			(0x1504);			// Execute a console command on this server
       const ushort NET_CHAT_GS_OPTIONS				(0x1505);			// Various options to control cvars
       const ushort NET_CHAT_GS_DYNAMIC_PRODUCTS		(0x1506);			// NO COMMENT FOR YOU.

       // Server Manager -> Chat Server
       const ushort NET_CHAT_SM_CONNECT				(0x1600);			// Server manager requesting connection
       const ushort NET_CHAT_SM_DISCONNECT				(0x1601);			// Server manager disconnecting
       const ushort NET_CHAT_SM_STATUS					(0x1602);			// Server manager's current status
       const ushort NET_CHAT_SM_UPLOAD_UPDATE			(0x1603);			// Server manager's response to a NET_CHAT_SM_UPLOAD_REQUEST

       // Chat Server -> Server Manager
       const ushort NET_CHAT_SM_ACCEPT					(0x1700);			// Accept connection from game server
       const ushort NET_CHAT_SM_REJECT					(0x1701);			// Refuse connection from game server
       const ushort NET_CHAT_SM_REMOTE_COMMAND			(0x1702);			// Execute a command on this server manager
       const ushort NET_CHAT_SM_OPTIONS				(0x1703);			// Various options to control cvars
       const ushort NET_CHAT_SM_UPLOAD_REQUEST			(0x1704);			// What replay or game log should the SM try to upload?

       // Web -> Chat Server
       const ushort NET_CHAT_WEB_UPLOAD_REQUEST		(0x1800);			// A request from a website to upload a replay or game log
       const ushort NET_CHAT_QUERY_LEAVER_STRIKE		(0x1801);			// A request from masterserver to chatserver to query account leaver strike info


       const ushort NET_CHAT_INVALID					(0xffff);


       const byte CHAT_MESSAGE_MAX_LENGTH				(250);				// Max length of any channel chat message
       const byte CHAT_CHANNEL_MAX_LENGTH				(35);				// Max length of channel names
       const byte CHAT_CHANNEL_TOPIC_MAX_LENGTH		(140);				// Max length of channel topics



       enum EChatRejectReason
       {
        ECR_UNKNOWN = 0,
        ECR_BAD_VERSION,
        ECR_AUTH_FAILED,
        ECR_ACCOUNT_SHARING,
        ECR_ACCOUNT_SHARING_WARNING
       };

       enum EGameLobbyState
       {
        GLS_INVALID = -1,

        GLS_ADJUST_LOBBY = 0,
        GLS_HERO_PICK = 1,
        GLS_LAUNCHING,
        GLS_IN_STATS,

        NUM_GAME_LOBBY_STATES,
       };

       // chat server will sent it to game server when match starts
       // game server then send it to master server.
       //
       // new Arranged Match Type need to be added at bottom
       // this will affect code in master server class_type.php
       enum EArrangedMatchType
       {
        AM_PUBLIC = 0,				    // Public games
        AM_MATCHMAKING = 1,				// Ranked Matchmaking games (normal and casual)
        AM_SCHEDULED_MATCH = 2,			// Scheduled matches (tournament)
        AM_UNSCHEDULED_MATCH = 3,		// Unscheduled matches (league)
        AM_MATCHMAKING_MIDWARS = 4,		// Matchmaking games (midwars)
        AM_MATCHMAKING_BOTMATCH = 5,	// Matchmaking games (coop/bot)
        AM_UNRANKED_MATCHMAKING = 6,	// Unranked matchmaking games (normal and casual)
        AM_MATCHMAKING_RIFTWARS = 7,	// Matchmaking games (riftwars)
        AM_PUBLIC_PRELOBBY = 8,
        AM_MATCHMAKING_CUSTOM = 9,		// Matchmaking games (custom maps)
        AM_MATCHMAKING_CAMPAIGN = 10,   // Matchmaking games (campaign, normal and casual)

        NUM_ARRANGED_MATCH_TYPES
       };

       static bool IsMatchmakingType(EArrangedMatchType eType)
       {
        switch (eType)
        {
        case AM_PUBLIC:
        case AM_SCHEDULED_MATCH:
        case AM_UNSCHEDULED_MATCH:
            return false;

        case AM_MATCHMAKING:
        case AM_MATCHMAKING_MIDWARS:
        case AM_MATCHMAKING_RIFTWARS:
        case AM_MATCHMAKING_BOTMATCH:
        case AM_UNRANKED_MATCHMAKING:
        case AM_MATCHMAKING_CUSTOM:
        case AM_MATCHMAKING_CAMPAIGN:
            return true;

        default:
            return false;
        }
       }

       enum ETMMUpdateType
       {
        TMM_CREATE_GROUP = 0,
        TMM_FULL_GROUP_UPDATE,
        TMM_PARTIAL_GROUP_UPDATE,
        TMM_PLAYER_JOINED_GROUP,
        TMM_PLAYER_LEFT_GROUP,
        TMM_PLAYER_KICKED_FROM_GROUP,
        TMM_GROUP_JOINED_QUEUE,
        TMM_GROUP_REJOINED_QUEUE,
        TMM_GROUP_LEFT_QUEUE,
        TMM_INVITED_TO_GROUP,
        TMM_PLAYER_REJECTED_GROUP_INVITE,
        TMM_GROUP_QUEUE_UPDATE,
        TMM_GROUP_NO_MATCHES_FOUND,
        TMM_GROUP_NO_SERVERS_FOUND,
        TMM_POPULARITY_UPDATE,
        TMM_FOUND_MATCH_UPDATE,
        TMM_GROUP_FOUND_SERVER,
        TMM_MATCHMAKING_DISABLED,

        NUM_TMM_UPDATE_TYPES
       };

       enum ELOBBYUpdateType
       {
        LOBBY_CLIENT_ENTER = 0,	//tell the client who entered, if not bot
        LOBBY_CLIENT_ON_ENTER,	//tell other client that one client/bot entered
        LOBBY_CLIENT_ON_LEAVE,	//tell other client that one client/bot left
        LOBBY_CLIENT_ON_CHANGE,	//tell other client that one client change slot
        LOBBY_STATE_UPDATE,		//tell all client for state update
        LOBBY_INFO_UPDATE,		//tell all client for info upate

        LOBBY_UPDATE_TYPE_NUM
       };

       const wchar_t* const g_sTMMUpdateTypes[] =
       {
        L"TMM_CREATE_GROUP",				// TMM_CREATE_GROUP
        L"TMM_FULL_GROUP_UPDATE",			// TMM_FULL_GROUP_UPDATE
        L"TMM_PARTIAL_GROUP_UPDATE",		// TMM_PARTIAL_GROUP_UPDATE
        L"TMM_PLAYER_JOINED_GROUP",			// TMM_PLAYER_JOINED_GROUP
        L"TMM_PLAYER_LEFT_GROUP",			// TMM_PLAYER_LEFT_GROUP
        L"TMM_PLAYER_KICKED_FROM_GROUP",	// TMM_PLAYER_KICKED_FROM_GROUP
        L"TMM_GROUP_JOINED_QUEUE",			// TMM_GROUP_JOINED_QUEUE
        L"TMM_GROUP_REJOINED_QUEUE",		// TMM_GROUP_REJOINED_QUEUE
        L"TMM_GROUP_LEFT_QUEUE",			// TMM_GROUP_LEFT_QUEUE
        L"TMM_INVITED_TO_GROUP",			// TMM_INVITED_TO_GROUP
        L"TMM_PLAYER_REJECTED_GROUP_INVITE",	// TMM_PLAYER_REJECTED_GROUP_INVITE
        L"TMM_GROUP_QUEUE_UPDATE",			// TMM_GROUP_QUEUE_UPDATE
        L"TMM_GROUP_NO_MATCHES_FOUND",		// TMM_GROUP_NO_MATCHES_FOUND
        L"TMM_GROUP_NO_SERVERS_FOUND",		// TMM_GROUP_NO_SERVERS_FOUND
        L"TMM_POPULARITY_UPDATE",			// TMM_POPULARITY_UPDATE
        L"TMM_FOUND_MATCH_UPDATE",			// TMM_FOUND_MATCH_UPDATE
        L"TMM_GROUP_FOUND_SERVER",			// TMM_GROUP_FOUND_SERVER
        L"TMM_MATCHMAKING_DISABLED",		// TMM_MATCHMAKING_DISABLED
       };
       //assert_compile_time(STATIC_ARRAY_SIZE(g_sTMMUpdateTypes) == NUM_TMM_UPDATE_TYPES); // This currently doesn't exist on the chat server

       enum ETMMFailedToJoinReason
       {
        TMMFTJR_LEAVER = 0,				// Group has a leaver
        TMMFTJR_DISABLED,				// Matchmaking is disabled
        TMMFTJR_BUSY,					// Matchmaking is full
        TMMFTJR_OPTION_UNAVAILABLE,		// An option selected is currently unavailable
        TMMFTJR_INVALID_VERSION,		// Client's version is out of date
        TMMFTJR_GROUP_FULL,				// The group you're trying to join is full
        TMMFTJR_BAD_STATS,				// Unable to retrieve player's stats
        TMMFTJR_ALREADY_QUEUED,			// The group you're trying to join is in queue
        TMMFTJR_TRIAL,					// Trial accounts aren't allowed to play matchmaking (deprecated)
        TMMFTJR_BANNED,					// You're currently banned from matchmaking
        TMMFTJR_LOBBY_FULL,
        TMMFTJR_WRONG_PASSWORD,
        TMMFTJR_CAMPAIGN_NOT_ELIGIBLE,
       };

       enum EScheduledMatchUpdateType
       {
        SM_UPDATE,
        SM_PLAYER_JOIN,
        SM_PLAYER_LEAVE,
        SM_PLAYER_READY,
        SM_PLAYER_UNREADY,
        SM_LOADING_UPDATE,
        SM_SERVER_LOCATING,
        SM_SERVER_LOADING,
        SM_SERVER_READY,
        SM_REMOVED,
        SM_FORFEIT,

        NUM_SM_UPDATE_TYPES
       };

       enum EServerStatus
       {
        SERVER_STATUS_SLEEPING = 0,
        SERVER_STATUS_IDLE,
        SERVER_STATUS_LOADING,
        SERVER_STATUS_ACTIVE,
        SERVER_STATUS_CRASHED,
        SERVER_STATUS_KILLED,

        SERVER_STATUS_UNKNOWN
       };

       enum EMatchAbortedReason
       {
        MATCH_ABORTED_UNKNOWN = 0,

        MATCH_ABORT_CONNECT_TIMEOUT,
        MATCH_ABORT_START_TIMEOUT,
        MATCH_ABORT_PLAYER_LEFT,
        MATCH_ABORT_NEVERENDING
       };

       enum EMatchEndedReason
       {
        MATCH_ENDED_FINISHED,
        MATCH_ENDED_REMADE
       };

       enum EChatModeType
       {
        CHAT_MODE_AVAILABLE,
        CHAT_MODE_AFK,
        CHAT_MODE_DND,
        CHAT_MODE_INVISIBLE,
       };

       enum EAdminLevel
       {
        CHAT_CLIENT_ADMIN_NONE = 0,
        CHAT_CLIENT_ADMIN_OFFICER,
        CHAT_CLIENT_ADMIN_LEADER,
        CHAT_CLIENT_ADMIN_ADMINISTRATOR,
        CHAT_CLIENT_ADMIN_STAFF,
        CHAT_NUM_ADMIN_LEVELS,
       };

       enum EMatchIDResult
       {
        MIDR_FIRST = 0,

        MIDR_SUCCESS = MIDR_FIRST,
        MIDR_ERROR_SPAWN_REQUEST,
        MIDR_ERROR_UNSUCCESSFUL,
        MIDR_ERROR_EMPTY_RESPONSE,
        MIDR_ERROR_EMPTY_ARRAY,
        MIDR_ERROR_INVALID_RESPONSE,
        MIDR_ERROR_CONNECT,
        MIDR_ERROR_INVALID_MATCH_ID,
        MIDR_ERROR_NO_GAME_INFO,
        MIDR_ERROR_TIMEOUT,
        MIDR_LAST_ERROR = MIDR_ERROR_TIMEOUT,

        MIDR_FAILED,

        NUM_MATCH_ID_RESULTS,
       };

       enum EClientAuthResult
       {
        CAR_FIRST = 0,

        CAR_SUCCESS = CAR_FIRST,
        CAR_ERROR_UNSUCCESSFUL,
        CAR_ERROR_EMPTY_RESPONSE,
        CAR_ERROR_EMPTY_ARRAY,
        CAR_ERROR_INVALID_RESPONSE,
        CAR_ERROR_INVALID_COOKIE,
        CAR_ERROR_INVALID_ACCOUNT_ID,
        CAR_ERROR_CONNECT,
        CAR_ERROR_TIMEOUT,
        CAR_LAST_ERROR = CAR_ERROR_TIMEOUT,

        CAR_FAILED,

        NUM_CLIENT_AUTH_RESULTS,
       };

       enum EStatSubmissionResult
       {
        SSR_FIRST = 0,

        SSR_SUCCESS = SSR_FIRST,
        SSR_ERROR_UNSUCCESSFUL,
        SSR_ERROR_EMPTY_RESPONSE,
        SSR_ERROR_EMPTY_ARRAY,
        SSR_ERROR_INVALID_RESPONSE,
        SSR_ERROR_CONNECT,
        SSR_LAST_ERROR = SSR_ERROR_CONNECT,

        SSR_FAILED,

        NUM_STAT_SUBMISSION_RESULTS,
       };

       enum EPlayerSpectateRequest
       {
        PLAYER_SPECTATE_REQUEST = 0,
        PLAYER_SPECTATE_REQUEST_RESPONSE,
       };

       enum EPlayerSpectateRequestResponses
       {
        PSRR_ALLOW,
        PSRR_DENY,
        PSRR_FULL_PLAYER,
        PSRR_FULL_SERVER,
        PSSR_TOO_DEEP,
       };

       enum EMatchmakingBroadcastTracking
       {
        MM_MATCH_FOUND,
        MM_SERVER_FOUND,
        MM_MATCH_READY,
        MM_SERVER_NOT_IDLE,
        MM_MATCH_READY_REMINDER,

        NUM_MATCHMAKING_BROADCASTS
       };

       enum EActionCampaigns
       {
        AC_DAILY_LOGINS = 0,
        AC_CLICKED_HON_STORE,
        AC_CLICKED_BUY_COINS,
        AC_CLICKED_MOTD_ADS,
        AC_CLICKED_SPECIALS,
        AC_CLICKED_EARLY_ACCESS,
        AC_CLICKED_HEROES,
        AC_CLICKED_ALT_AVATARS,
        AC_CLICKED_ACCOUNT_ICONS,
        AC_CLICKED_NAME_COLORS,
        AC_CLICKED_SYMBOLS,
        AC_CLICKED_TAUNT,
        AC_CLICKED_ANNOUNCERS,
        AC_CLICKED_COURIERS,
        AC_CLICKED_OTHER,
        AC_CLICKED_BUNDLES,
        AC_CLICKED_OPEN_VAULT,
        AC_MATCHMAKING_MATCH_FOUND,
        AC_MATCHMAKING_SERVER_FOUND,
        AC_MATCHMAKING_MATCH_READY,
        AC_MATCHMAKING_SERVER_NOT_IDLE,
        AC_MATCHMAKING_MATCH_READY_REMINDER,
        AC_ADDITIONAL_CAMPAIGN_ONE,
        AC_ADDITIONAL_CAMPAIGN_TWO,
        AC_ADDITIONAL_CAMPAIGN_THREE,
        AC_ADDITIONAL_CAMPAIGN_FOUR,
        AC_ADDITIONAL_CAMPAIGN_FIVE,
        AC_CLICKED_CUSTOM_WARDS,

        NUM_ACTION_CAMPAIGNS
       };

       enum EGenericResponses
       {
        GR_MAX_MATCH_FIDELITY_DIFFERENCE = 0,
        GR_SCHEDULED_MATCH_FULL,

        NUM_GENERIC_RESPONSES
       };

       enum EOSType
       {
        UNKNOWN_OS = 0,
        WINDOWS_OS,
        APPLE_OS,
        LINUX_OS,

        NUM_OS_TYPES
       };

       enum EOSVersion
       {
        EOSV_WINDOWS_XP_32,
        EOSV_WINDOWS_XP_64,
        EOSV_WINDOWS_VISTA_32,
        EOSV_WINDOWS_VISTA_64,
        EOSV_WINDOWS_7_32,
        EOSV_WINDOWS_7_64,
        EOSV_WINDOWS_8_32,
        EOSV_WINDOWS_8_64,
        EOSV_WINDOWS_10_32,
        EOSV_WINDOWS_10_64,
        EOSV_WINDOWS_OTHER,

        EOSV_MAC_OSX_10_5,
        EOSV_MAC_OSX_10_6,
        EOSV_MAC_OSX_10_7,
        EOSV_MAC_OSX_10_8,
        EOSV_MAC_OSX_10_9,
        EOSV_MAC_OSX_10_10,
        EOSV_MAC_OSX_10_11,
        EOSV_MAC_OSX_OTHER,

        EOSV_LINUX_32,
        EOSV_LINUX_64,

        EOSV_UNKNOWN,

        NUM_OS_VERSIONS
       };

       enum EExcessiveGamePlayType
       {
        EEGPT_NONE = 0,			// No message
        EEGPT_1_HOUR,			// You've played 1 hour
        EEGPT_2_HOURS,			// You've played 2 hours
        EEGPT_FATIGUE,			// You've played too long and are now experiencing fatigue, your game benefits are now cut down to half.
        EEGPT_UNHEALTHY,		// You've played long enough to experience unhealthy side effects, you will no longer receive any game benefits.
        EEGPT_GENERAL = 255		// A general message for excessive game play broadcasts (used only for Korea)
       };

       enum EExcessiveGamePlayBenefit
       {
        EEGPB_NORMAL = 0,		// Normal benefits
        EEGPB_HALF,				// Half benefits
        EEGPB_NONE,				// No benefits
       };

       enum ECrashReportingClientState
       {
        CRCS_NO_CRASH = 0,				// exited normally
        CRCS_IDLE,						// idle, before joining a game
        CRCS_CONNECTING,				// connecting to a server
        CRCS_LOADING,					// loading map resources
        CRCS_LOBBY,						// in game lobby
        CRCS_LOADING_HEROES,			// loading heroes
        CRCS_IN_GAME,					// in game
        CRCS_DISCONNECTING_NEXT_FRAME,	// client will disconnect next frame (one of the Disconnect commands called)
        CRCS_DISCONNECTING,				// disconnecting from server
        CRCS_UNLOADING_RESOURCES,		// unloading world, etc
        CRCS_UNKNOWN,					// crashed at unknown state
        CRCS_UPDATING,					// updating
        NUM_CRCS
       };

       enum EChatClientStatus
       {
        CHAT_CLIENT_STATUS_DISCONNECTED = 0,
        CHAT_CLIENT_STATUS_CONNECTING,
        CHAT_CLIENT_STATUS_WAITING_FOR_AUTH,
        CHAT_CLIENT_STATUS_CONNECTED,
        CHAT_CLIENT_STATUS_JOINING_GAME,
        CHAT_CLIENT_STATUS_IN_GAME,

        NUM_CHAT_CLIENT_STATUSES,
       };

       struct SRosterInfo
       {
        uint				uiAccountID;
        byte				yTeamSlot;

        SRosterInfo(uint _uiAccountID = 0, byte _yTeamSlot = 0) :
        uiAccountID(_uiAccountID),
        yTeamSlot(_yTeamSlot)
        {
        }
       };

       // Game client send it to chat server to be put in the correct queue
       // Normal and casual need to stay at 1 and 2, unless we refactor some things
       enum ETMMGameTypes
       {
        TMM_GAME_TYPE_NONE = -1,

        TMM_GAME_TYPE_NORMAL = 1,
        TMM_GAME_TYPE_CASUAL = 2,
        TMM_GAME_TYPE_MIDWARS = 3,
        TMM_GAME_TYPE_RIFTWARS = 4,
        TMM_GAME_TYPE_CUSTOM = 5,
        TMM_GAME_TYPE_CAMPAIGN_NORMAL = 6,
        TMM_GAME_TYPE_CAMPAIGN_CASUAL = 7,
        TMM_GAME_TYPE_REBORN_NORMAL = 8,
        TMM_GAME_TYPE_REBORN_CASUAL = 9,
        TMM_GAME_TYPE_MIDWARS_REBORN = 10,

        TMM_NUM_GAME_TYPES
       };

       enum ETMMTypes
       {
        TMM_TYPE_SOLO = 1,
        TMM_TYPE_PVP = 2,
        TMM_TYPE_COOP = 3,
        TMM_TYPE_CAMPAIGN = 4
       };

       // Make sure to update CMMCommon::GetMapFromMask if you change this
       enum ETMMGameMaps
       {
        TMM_GAME_MAP_NONE = -1,

        TMM_GAME_MAP_FORESTS_OF_CALDAVAR = 0,
        TMM_GAME_MAP_GRIMMS_CROSSING,
        TMM_GAME_MAP_MIDWARS,
        TMM_GAME_MAP_RIFTWARS,
        TMM_GAME_MAP_PROPHETS,
        TMM_GAME_MAP_THE_GRIMM_HUNT,
        TMM_GAME_MAP_CAPTURE_THE_FLAG,
        TMM_GAME_MAP_DEVO_WARS,
        TMM_GAME_MAP_SOCCER,
        TMM_GAME_MAP_SOLO_MAP,
        TMM_GAME_MAP_TEAM_DEATHMATCH,
        TMM_GAME_MAP_CALDAVAR_REBORN,
        TMM_GAME_MAP_MIDWARS_REBORN,

        TMM_NUM_GAME_MAPS
       };
       const uint MAX_TMM_GAME_MAPS_SELECTABLE(1);

       // Make sure to update CMMCommon::GetGameModeFromMask if you change this
       enum ETMMGameModes
       {
        TMM_GAME_MODE_NONE = -1,

        TMM_GAME_MODE_ALL_PICK = 0,
        TMM_GAME_MODE_ALL_PICK_GATED,
        TMM_GAME_MODE_ALL_PICK_DUPLICATE_HERO,
        TMM_GAME_MODE_SINGLE_DRAFT,
        TMM_GAME_MODE_BANNING_DRAFT,
        TMM_GAME_MODE_BANNING_PICK,
        TMM_GAME_MODE_ALL_RANDOM,
        TMM_GAME_MODE_LOCK_PICK,
        TMM_GAME_MODE_BLIND_BAN,
        TMM_GAME_MODE_BLIND_BAN_GATED,
        TMM_GAME_MODE_BLIND_BAN_RAPID_FIRE,
        TMM_GAME_MODE_BOT_MATCH,
        TMM_GAME_MODE_CAPTAINS_PICK,
        TMM_GAME_MODE_BALANCED_RANDOM,
        TMM_GAME_MODE_KROS_MODE,
        TMM_GAME_MODE_RANDOM_DRAFT,
        TMM_GAME_MODE_BANNING_DRAFT_RAPID_FIRE,
        TMM_GAME_MODE_COUNTER_PICK,
        TMM_GAME_MODE_FORCE_PICK,
        TMM_GAME_MODE_SOCCER_PICK,
        TMM_GAME_MODE_SOLO_SAME,
        TMM_GAME_MODE_SOLO_DIFF,
        TMM_GAME_MODE_HERO_BAN,
        TMM_GAME_MODE_MIDWARS_BETA,
        TMM_GAME_MODE_REBORN,

        TMM_NUM_GAME_MODES
       };
       const uint MAX_TMM_GAME_MODES_SELECTABLE(6);

       // Make sure to update CMMCommon::GetRegionFromMask if you change this
       enum ETMMGameRegions
       {
        TMM_GAME_REGION_NONE = -1,

        TMM_GAME_REGION_USE = 0,
        TMM_GAME_REGION_USW,
        TMM_GAME_REGION_EU,
        TMM_GAME_REGION_SG,
        TMM_GAME_REGION_MY,
        TMM_GAME_REGION_PH,
        TMM_GAME_REGION_TH,
        TMM_GAME_REGION_ID,
        TMM_GAME_REGION_VN,
        TMM_GAME_REGION_RU,
        TMM_GAME_REGION_KR,
        TMM_GAME_REGION_AU,
        TMM_GAME_REGION_LAT,
        TMM_GAME_REGION_DX,
        TMM_GAME_REGION_CN,
        TMM_GAME_REGION_BR,
        TMM_GAME_REGION_TR,

        NUM_TMM_GAME_REGIONS
       };
       const uint MAX_TMM_GAME_REGIONS_SELECTABLE(6);

       enum ETMMRankType
       {
        TMM_OPTION_UNRANKED = 0,
        TMM_OPTION_RANKED = 1,

        TMM_NUM_OPTION_RANK_TYPES
       };

       struct STMMPopularities
       {
        byte		ayGameType[TMM_NUM_GAME_TYPES][TMM_NUM_GAME_MAPS][TMM_NUM_OPTION_RANK_TYPES];
        byte		ayGameMap[TMM_NUM_GAME_MAPS][TMM_NUM_GAME_TYPES][TMM_NUM_OPTION_RANK_TYPES];
        byte		ayGameMode[TMM_NUM_GAME_MODES][TMM_NUM_GAME_MAPS][TMM_NUM_GAME_TYPES][TMM_NUM_OPTION_RANK_TYPES];
        byte		ayRegion[NUM_TMM_GAME_REGIONS][TMM_NUM_GAME_MAPS][TMM_NUM_GAME_TYPES][TMM_NUM_OPTION_RANK_TYPES];

        void Clear()
        {
            for (uint i(0); i < TMM_NUM_GAME_TYPES; ++i)
                for (uint j(0); j < TMM_NUM_GAME_MAPS; ++j)
                    for (uint k(0); k < TMM_NUM_OPTION_RANK_TYPES; ++k)
                        ayGameType[i][j][k] = 0;

            for (uint i(0); i < TMM_NUM_GAME_MAPS; ++i)
                for (uint j(0); j < TMM_NUM_GAME_TYPES; ++j)
                    for (uint k(0); k < TMM_NUM_OPTION_RANK_TYPES; ++k)
                        ayGameMap[i][j][k] = 0;

            for (uint i(0); i < TMM_NUM_GAME_MODES; ++i)
                for (uint j(0); j < TMM_NUM_GAME_MAPS; ++j)
                    for (uint k(0); k < TMM_NUM_GAME_TYPES; ++k)
                        for (uint l(0); l < TMM_NUM_OPTION_RANK_TYPES; ++l)
                            ayGameMode[i][j][k][l] = 0;

            for (uint i(0); i < NUM_TMM_GAME_REGIONS; ++i)
                for (uint j(0); j < TMM_NUM_GAME_MAPS; ++j)
                    for (uint k(0); k < TMM_NUM_GAME_TYPES; ++k)
                        for (uint l(0); l < TMM_NUM_OPTION_RANK_TYPES; ++l)
                            ayRegion[i][j][k][l] = 0;
        }
       };



       const byte CHAT_CLIENT_IS_OFFICER		(BIT(0));
       const byte CHAT_CLIENT_IS_CLAN_LEADER	(BIT(1));
       const byte CHAT_CLIENT_IS_STAFF			(BIT(5));
       const byte CHAT_CLIENT_IS_PREMIUM		(BIT(6));
       const byte CHAT_CLIENT_IS_VERIFIED		(BIT(7));

       const uint CHAT_CHANNEL_FLAG_PERMANENT		(BIT(0));
       const uint CHAT_CHANNEL_FLAG_SERVER			(BIT(1)); // Channel for post-match chat
       const uint CHAT_CHANNEL_FLAG_HIDDEN			(BIT(2));
       const uint CHAT_CHANNEL_FLAG_RESERVED		(BIT(3)); // System created channels (e.g. general, clan, stream, etc.)
       const uint CHAT_CHANNEL_FLAG_GENERAL_USE	(BIT(4)); // e.g. HoN 1
       const uint CHAT_CHANNEL_FLAG_UNJOINABLE		(BIT(5));
       const uint CHAT_CHANNEL_FLAG_AUTH_REQUIRED	(BIT(6));
       const uint CHAT_CHANNEL_FLAG_CLAN			(BIT(7));
       // Flags beyond this point are untransmitted until the client gets updated!
       const uint CHAT_CHANNEL_FLAG_STREAM_USE		(BIT(8));

       const uint MAX_USERS_PER_HON_CHANNEL(50);
       const uint MAX_USERS_PER_CHANNEL(250);
       const uint INVALID_CHAT_CHANNEL(-1);


       enum EUploadUpdateType
       {
        EUUT_NONE = -1,
        EUUT_GENERAL_FAILURE = 0,
        EUUT_FILE_DOES_NOT_EXIST,
        EUUT_FILE_INVALID_HOST,
        EUUT_FILE_ALREADY_UPLOADED,
        EUUT_FILE_ALREADY_QUEUED,
        EUUT_FILE_QUEUED,
        EUUT_FILE_UPLOADING,
        EUUT_FILE_UPLOAD_COMPLETE
       };

       extern const char* const g_aCRCSStrings[NUM_CRCS];


       enum EDisconnectReason
       {
        DISCONNECT_INVALID = 0,
        DISCONNECT_AUTH_FAILED,
        DISCONNECT_BANNED,
        DISCONNECT_BASIC,
        DISCONNECT_CLIENT_NUMBER_MISMATCH,
        DISCONNECT_CONSOLE,
        DISCONNECT_DUPLICATE_CONNECT,
        DISCONNECT_DUPLICATE_LOGIN,
        DISCONNECT_FLOODING,
        DISCONNECT_GAME_IN_PROGRESS,
        DISCONNECT_GAME_OVER,
        DISCONNECT_INVALID_CONNECTION,
        DISCONNECT_INVALID_PSR,
        DISCONNECT_INVALID_SERVER_SESSION,
        DISCONNECT_KICKED,
        DISCONNECT_KICKED_MENTOR,
        DISCONNECT_KICKED_SPEC,
        DISCONNECT_KICKED_CLIENT_FLOODING,
        DISCONNECT_LEAVER,
        DISCONNECT_LOAD_FAILED,
        DISCONNECT_MATCH_NEVERENDING,
        DISCONNECT_MAX_SPEC_CLIENTS,
        DISCONNECT_MISSING_SPECTATE_PLAYER,
        DISCONNECT_MM_CONNECT_TIMEOUT,
        DISCONNECT_MM_PLAYER_LEFT,
        DISCONNECT_MM_START_TIMEOUT,
        DISCONNECT_MODIFIED_CORE_FILES,
        DISCONNECT_NO_COOKIE,
        DISCONNECT_NO_INCEPTION_MENTOR,
        DISCONNECT_NO_INCEPTION_SPECTATOR,
        DISCONNECT_NO_MATCH_ID,
        DISCONNECT_NO_MODS_ALLOWED,
        DISCONNECT_NO_TOKENS,
        DISCONNECT_NOT_ACCEPTED,
        DISCONNECT_NOT_HOST,
        DISCONNECT_NOT_INVITED,
        DISCONNECT_NOT_ON_ROSTER,
        DISCONNECT_NOT_VERIFIED,
        DISCONNECT_NOT_VIP,
        DISCONNECT_OVERFLOW,
        DISCONNECT_REMAKE,
        DISCONNECT_SERVER_ERROR,
        DISCONNECT_SERVER_FULL,
        DISCONNECT_SERVER_INVALID,
        DISCONNECT_SERVER_UPDATING,
        DISCONNECT_SNAPSHOT_FRAGMENT,
        DISCONNECT_STAFF_SPEC_NOT_STAFF,
        DISCONNECT_TERMINATED,
        DISCONNECT_TIMED_OUT,
        DISCONNECT_TOO_MANY_PLAYER_SPECTATORS,
        DISCONNECT_TOO_MANY_PLAYER_SPECTATORS_TOTAL,
        DISCONNECT_TOURNEY_RULES_NO_SPEC,
        DISCONNECT_TRIAL_EXPIRED,
        DISCONNECT_TRIAL_NOT_ALLOWED,
        DISCONNECT_UNASSIGNED,
        DISCONNECT_UNIQUE_LOGIN_FAILED,
        DISCONNECT_UNKNOWN,
        DISCONNECT_VOTE_KICKED,
        DISCONNECT_DISCONNECTED,

        NUM_DISCONNECT_REASONS
        // If you add to this list, be sure to update tables in public_strings.cpp
       };

       extern const bool g_transmittedDisconnects[NUM_DISCONNECT_REASONS];

       extern const char* const g_aDisconnectStrings[NUM_DISCONNECT_REASONS];

       enum EQuestsAvailabilityType
       {
        EQAT_INVALID = -1,
        EQAT_DISABLED_GENERAL,
        EQAT_ENABLED,
        EQAT_DISABLED_TECHNICAL,
        EQAT_DISABLED_SEASON_CLOSED,
        EQAT_DISABLED_RESERVED_1,
        EQAT_DISABLED_RESERVED_2,
        EQAT_DISABLED_RESERVED_3,

        NUM_QUEST_AVAILABILITY_TYPES
       };

       // Server Status Flags
       #define SSF_OFFICIAL				BIT(0)
       #define SSF_OFFICIAL_WITH_STATS		BIT(1)
       #define SSF_NOLEAVER				BIT(2)
       #define SSF_VERIFIED_ONLY			BIT(3)
       #define SSF_PRIVATE					BIT(4)
       #define SSF_TIER_NOOBS_ALLOWED		BIT(5)
       #define SSF_TIER_PRO				BIT(6)
       #define SSF_ALL_HEROES				BIT(7)
       #define SSF_CASUAL					BIT(8)
       #define SSF_GATED					BIT(9)
       #define SSF_FORCE_RANDOM			BIT(10)
       #define SSF_AUTO_BALANCE			BIT(11)
       #define SSF_ADV_OPTIONS				BIT(12)
       #define SSF_DEV_HEROES				BIT(13)
       #define SSF_HARDCORE				BIT(14)
       //=============================================================================

       #endif //__CHATSERVER_PROTOCOL_H__
     */
}
