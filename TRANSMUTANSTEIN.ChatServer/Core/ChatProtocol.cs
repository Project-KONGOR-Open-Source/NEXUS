namespace TRANSMUTANSTEIN.ChatServer.Core;

public static class ChatProtocol
{
    // Thank you, Shawn Presser, for making these values public: https://github.com/shawwn/hon/blob/f1aa2dfb7d07c447e930aa36f571e547714f4a57/lib/k2public/chatserver_protocol.h.
    // The symbols added since the chat server protocol version linked above were extracted from the PDB files made public at http://cdn.hon.team/wac/x86_64/4.9.1.3/symbols.zip in February 2021.

    public const uint CHAT_PROTOCOL_VERSION = 68;

    public static class Command
    {
        public const ushort CHAT_CMD_CHANNEL_MSG                            = 0x03; // Used when a user messages a channel
        public const ushort CHAT_CMD_CHANGED_CHANNEL                        = 0x04; // Used when we change channels
        public const ushort CHAT_CMD_JOINED_CHANNEL                         = 0x05; // Used when a new user joins our channel
        public const ushort CHAT_CMD_LEFT_CHANNEL                           = 0x06; // Used when a user leaves our channel
        public const ushort CHAT_CMD_DISCONNECTED                           = 0x07; // Used when we get disconnected
        public const ushort CHAT_CMD_WHISPER                                = 0x08; // Used when one user whispers another
        public const ushort CHAT_CMD_WHISPER_FAILED                         = 0x09; // Used when the whisper target could not be found
        public const ushort CHAT_CMD_LAST_KNOWN_GAME_SERVER                 = 0x0A; // Return the last known game server for myself
        public const ushort CHAT_CMD_INITIAL_STATUS                         = 0x0B; // Sent on connect to update buddy and clan connection status for new client
        public const ushort CHAT_CMD_UPDATE_STATUS                          = 0x0C; // Sent on connect to update buddy and clan connection status for old clients
        public const ushort CHAT_CMD_REQUEST_BUDDY_ADD                      = 0x0D; // Sent from client to chat server to request a buddy add
        public const ushort CHAT_CMD_NOTIFY_BUDDY_REMOVE                    = 0x0E; // Sent from client to chat server to notify a buddy has been removed
        public const ushort CHAT_CMD_JOINING_GAME                           = 0x0F; // Sent when a user starts joining a game
        public const ushort CHAT_CMD_JOINED_GAME                            = 0x10; // Sent when a user finishes joining a game
        public const ushort CHAT_CMD_LEFT_GAME                              = 0x11; // Sent when a user leaves a game
        public const ushort EMPTY_0x12                                      = 0x12;
        public const ushort CHAT_CMD_CLAN_WHISPER                           = 0x13; // Sent when whispering an entire clan
        public const ushort CHAT_CMD_CLAN_WHISPER_FAILED                    = 0x14; // Sent when a whisper to a clan fails
        public const ushort CHAT_CMD_CLAN_PROMOTE_NOTIFY                    = 0x15; // Sent with notification keys for the server to verify on clan promotion
        public const ushort CHAT_CMD_CLAN_DEMOTE_NOTIFY                     = 0x16; // Sent with notification keys for the server to verify on clan demotion
        public const ushort CHAT_CMD_CLAN_REMOVE_NOTIFY                     = 0x17; // Sent with notification keys for the server to verify on clan removal
        public const ushort EMPTY_0x18                                      = 0x18;
        public const ushort EMPTY_0x19                                      = 0x19;
        public const ushort EMPTY_0x1A                                      = 0x1A;
        public const ushort CHAT_CMD_FLOODING                               = 0x1B; // Warning to user that their message wasn't sent due to flood control
        public const ushort CHAT_CMD_IM                                     = 0x1C; // Used when a user recieves/sends an IM through the CC panel
        public const ushort CHAT_CMD_IM_FAILED                              = 0x1D; // Used when a user fails to send an IM
        public const ushort CHAT_CMD_JOIN_CHANNEL                           = 0x1E; // Sent by user when joining a new channel
        public const ushort CHAT_CMD_DYANMIC_PRODUCT_LIST                   = 0x1F; // Dynamic products that are updated in the chat server heartbeat then sent to the client when changed.
        public const ushort CHAT_CMD_WHISPER_BUDDIES                        = 0x20; // Sending whisper to all buddies
        public const ushort CHAT_CMD_MAX_CHANNELS                           = 0x21; // Error sent when user has joined max. # of channels
        public const ushort CHAT_CMD_LEAVE_CHANNEL                          = 0x22; // Sent by user when leaving a channel
        public const ushort CHAT_CMD_INVITE_USER_ID                         = 0x23; // Sent by game server to invite a user to a game by account ID
        public const ushort CHAT_CMD_INVITE_USER_NAME                       = 0x24; // Sent by game server to invite a user to a game by account name
        public const ushort CHAT_CMD_INVITED_TO_SERVER                      = 0x25; // Sent by chat server to notify a user of a pending server invite
        public const ushort CHAT_CMD_INVITE_FAILED_USER                     = 0x26; // Notifies a user that their invite request failed because the target was not found
        public const ushort CHAT_CMD_INVITE_FAILED_GAME                     = 0x27; // Notifies a user that their invite request failed because they are not in a game
        public const ushort CHAT_CMD_INVITE_REJECTED                        = 0x28; // Indicates that a recieved invite was rejected
        public const ushort EMPTY_0x29                                      = 0x29;
        public const ushort CHAT_CMD_USER_INFO                              = 0x2A; // Returns information on a user
        public const ushort CHAT_CMD_USER_INFO_NO_EXIST                     = 0x2B; // The requested user does not exist
        public const ushort CHAT_CMD_USER_INFO_OFFLINE                      = 0x2C; // Returns information on an offline user
        public const ushort CHAT_CMD_USER_INFO_ONLINE                       = 0x2D; // Returns information on an online user
        public const ushort CHAT_CMD_USER_INFO_IN_GAME                      = 0x2E; // Returns information on a user in a game
        public const ushort CHAT_CMD_CHANNEL_UPDATE                         = 0x2F; // Update channel information
        public const ushort CHAT_CMD_CHANNEL_TOPIC                          = 0x30; // Set/get channel topic
        public const ushort CHAT_CMD_CHANNEL_KICK                           = 0x31; // Kick user from channel
        public const ushort CHAT_CMD_CHANNEL_BAN                            = 0x32; // Ban user from channel
        public const ushort CHAT_CMD_CHANNEL_UNBAN                          = 0x33; // Unban user from channel
        public const ushort CHAT_CMD_CHANNEL_IS_BANNED                      = 0x34; // User is banned from channel
        public const ushort CHAT_CMD_CHANNEL_SILENCED                       = 0x35; // User is silenced in this channel
        public const ushort CHAT_CMD_CHANNEL_SILENCE_LIFTED                 = 0x36; // User is no longer silenced in a channel
        public const ushort CHAT_CMD_CHANNEL_SILENCE_PLACED                 = 0x37; // User is now silenced in a channel
        public const ushort CHAT_CMD_CHANNEL_SILENCE_USER                   = 0x38; // Request to silence a user in a channel
        public const ushort CHAT_CMD_MESSAGE_ALL                            = 0x39; // Administrator message to all users
        public const ushort CHAT_CMD_CHANNEL_PROMOTE                        = 0x3A; // Request to promote a user in a channel
        public const ushort CHAT_CMD_CHANNEL_DEMOTE                         = 0x3B; // Request to demote a user in a channel
        public const ushort EMPTY_0x3C                                      = 0x3C;
        public const ushort EMPTY_0x3D                                      = 0x3D;
        public const ushort CHAT_CMD_CHANNEL_SET_AUTH                       = 0x3E; // User wants to enable authorization on a channel
        public const ushort CHAT_CMD_CHANNEL_REMOVE_AUTH                    = 0x3F; // User wants to disable authorization on a channel
        public const ushort CHAT_CMD_CHANNEL_ADD_AUTH_USER                  = 0x40; // User wants to add a user to the authorization list for a channel
        public const ushort CHAT_CMD_CHANNEL_REMOVE_AUTH_USER               = 0x41; // User wants to remove a user from the authorization list for a channel
        public const ushort CHAT_CMD_CHANNEL_LIST_AUTH                      = 0x42; // User wants to get the authorization list for a channel
        public const ushort CHAT_CMD_CHANNEL_SET_PASSWORD                   = 0x43; // User wants to set the password for a channel
        public const ushort CHAT_CMD_CHANNEL_ADD_AUTH_FAIL                  = 0x44; // Failed to add the user to the channel authorization list
        public const ushort CHAT_CMD_CHANNEL_REMOVE_AUTH_FAIL               = 0x45; // Failed to remove the user from the channel authorization list
        public const ushort CHAT_CMD_JOIN_CHANNEL_PASSWORD                  = 0x46; // Channel join with password
        public const ushort CHAT_CMD_CLAN_ADD_MEMBER                        = 0x47; // Request to add a new clan member
        public const ushort CHAT_CMD_CLAN_ADD_REJECTED                      = 0x48; // Request to add a member was rejected
        public const ushort CHAT_CMD_CLAN_ADD_FAIL_ONLINE                   = 0x49; // Request to add a member failed, user was not online
        public const ushort CHAT_CMD_CLAN_ADD_FAIL_CLAN                     = 0x4A; // Request to add a member failed, user is in a clan
        public const ushort CHAT_CMD_CLAN_ADD_FAIL_INVITED                  = 0x4B; // Request to add a member failed, user has already been invited
        public const ushort CHAT_CMD_CLAN_ADD_FAIL_PERMS                    = 0x4C; // Request to add a member failed, user does not have proper permissions
        public const ushort CHAT_CMD_CLAN_ADD_FAIL_UNKNOWN                  = 0x4D; // Request to add a member failed
        public const ushort CHAT_CMD_NEW_CLAN_MEMBER                        = 0x4E; // New user added to clan
        public const ushort CHAT_CMD_CLAN_ADD_ACCEPTED                      = 0x4F; // Request to add a member was accepted
        public const ushort CHAT_CMD_CLAN_RANK_CHANGE                       = 0x50; // Clan member's rank changed
        public const ushort CHAT_CMD_CLAN_CREATE_REQUEST                    = 0x51; // Create clan request
        public const ushort CHAT_CMD_CLAN_CREATE_ACCEPT                     = 0x52; // One of the founding members accepted the request
        public const ushort CHAT_CMD_CLAN_CREATE_REJECT                     = 0x53; // One of the founding members rejected the request
        public const ushort CHAT_CMD_CLAN_CREATE_COMPLETE                   = 0x54; // Clan creation completed successfully
        public const ushort CHAT_CMD_CLAN_CREATE_FAIL_CLAN                  = 0x55; // Clan creation failed, one or more users are already in a clan
        public const ushort CHAT_CMD_CLAN_CREATE_FAIL_INVITE                = 0x56; // Clan creation failed, one or more users have an outstanding clan invitation
        public const ushort CHAT_CMD_CLAN_CREATE_FAIL_FIND                  = 0x57; // Clan creation failed, one or more users could not be found
        public const ushort CHAT_CMD_CLAN_CREATE_FAIL_DUPE                  = 0x58; // Clan creation failed, duplicate founding members
        public const ushort CHAT_CMD_CLAN_CREATE_FAIL_PARAM                 = 0x59; // Clan creation failed, one or more parameters are invalid
        public const ushort CHAT_CMD_NAME_CHANGE                            = 0x5A; // A user's name has changed
        public const ushort CHAT_CMD_CLAN_CREATE_FAIL_NAME                  = 0x5B; // Clan creation failed, name invalid
        public const ushort CHAT_CMD_CLAN_CREATE_FAIL_TAG                   = 0x5C; // Clan creation failed, tag invalid
        public const ushort CHAT_CMD_CLAN_CREATE_FAIL_UNKNOWN               = 0x5D; // Clan creation failed, unknown error

        public const ushort CHAT_CMD_AUTO_MATCH_CONNECT                     = 0x62; // The match is ready and the client should connect

        public const ushort CHAT_CMD_CHAT_ROLL                              = 0x64; // The user just rolled
        public const ushort CHAT_CMD_CHAT_EMOTE                             = 0x65; // The user just emoted
        public const ushort CHAT_CMD_SET_CHAT_MODE_TYPE                     = 0x66; // Sets the chat mode type
        public const ushort CHAT_CMD_CHAT_MODE_AUTO_RESPONSE                = 0x67; // Used for sending an auto response message

        public const ushort CHAT_CMD_PLAYER_COUNT                           = 0x68; // Reports user counts to players periodically
        public const ushort CHAT_CMD_SERVER_NOT_IDLE                        = 0x69; // Server was not idle
        public const ushort CHAT_CMD_ACTIVE_STREAMS                         = 0x6A; // Active stream list

        public const ushort CHAT_CMD_REQUEST_BUDDY_ADD_RESPONSE             = 0xB2;
        public const ushort CHAT_CMD_REQUEST_BUDDY_APPROVE                  = 0xB3;
        public const ushort CHAT_CMD_REQUEST_BUDDY_APPROVE_RESPONSE         = 0xB4;

        public const ushort CHAT_CMD_REQUEST_GAME_INFO                      = 0xB5;
        public const ushort CHAT_CMD_LIST_DATA                              = 0xB6;
        public const ushort CHAT_CMD_JOIN_STREAM_CHANNEL                    = 0xB7;
        public const ushort CHAT_CMD_PLAYER_SPECTATE_REQUEST                = 0xB8;
        public const ushort CHAT_CMD_TRACK_PLAYER_ACTION                    = 0xB9;
        public const ushort CHAT_CMD_STAFF_JOIN_MATCH_REQUEST               = 0xBA;
        public const ushort CHAT_CMD_STAFF_JOIN_MATCH_RESPONSE              = 0xBB;
        public const ushort CHAT_CMD_EXCESSIVE_GAMEPLAY_MESSAGE             = 0xBC;
        public const ushort CHAT_CMD_MAINTENANCE_MESSAGE                    = 0xBD;
        public const ushort CHAT_CMD_UPLOAD_REQUEST                         = 0xBE;
        public const ushort CHAT_CMD_UPLOAD_STATUS                          = 0xBF;
        public const ushort CHAT_CMD_OPTIONS                                = 0xC0; // Send clients options controlled by the chat server
        public const ushort CHAT_CMD_LOGOUT                                 = 0xC1;
        public const ushort CHAT_CMD_NEW_MESSAGES                           = 0xC2; // The user has recieved new messages and should pull their message list
    }

    public static class Bidirectional
    {
        public const ushort NET_CHAT_PING                                   = 0x2A00;
        public const ushort NET_CHAT_PONG                                   = 0x2A01;
    }

    public static class ClientToChatServer
    {
        public const ushort NET_CHAT_CL_CONNECT                             = 0x0C00; // Client requesting connection
        public const ushort NET_CHAT_CL_GET_CHANNEL_LIST                    = 0x0C01; // Client requests a list of channels
        public const ushort NET_CHAT_CL_CHANNEL_LIST_ACK                    = 0x0C02; // HACK: until TCP connections are handled properly
        public const ushort NET_CHAT_CL_GET_CHANNEL_SUBLIST                 = 0x0C03; // Client requests a sub-list of channels (for auto-complete)
        public const ushort NET_CHAT_CL_CHANNEL_SUBLIST_ACK                 = 0x0C04; // HACK: until TCP connections are handled properly
        public const ushort NET_CHAT_CL_GET_USER_STATUS                     = 0x0C05; // Client requesting status of a specific user
        public const ushort EMPTY_0x0C06                                    = 0x0C06;
        public const ushort EMPTY_0x0C07                                    = 0x0C07;
        public const ushort NET_CHAT_CL_ADMIN_KICK                          = 0x0C08; // Admin request to disconnect target client from chat server
        public const ushort NET_CHAT_CL_REFRESH_UPGRADES                    = 0x0C09; // Client is requesting an upgrade refresh for itself
        public const ushort NET_CHAT_CL_END_MATCH                           = 0x0C1B; // Client is requesting to end match
        public const ushort NET_CHAT_CL_FORCE_GROUP_MATCHUP                 = 0x0C1C; // Client is requesting to force a group into a match
        public const ushort NET_CHAT_CL_SET_MATCHMAKING_VERSION             = 0x0C1D; // Client is requesting to change matchmaking version
        public const ushort NET_CHAT_CL_BLOCK_PHRASE                        = 0x0C1E; // Client is requesting to block a phrase
        public const ushort NET_CHAT_CL_UNBLOCK_PHRASE                      = 0x0C1F; // Client is requesting to unblock a phrase
    }

    public static class GameLobby
    {
        public const ushort NET_CHAT_CL_GAME_LOBBY_SELECT_HERO              = 0x0C10;
        public const ushort NET_CHAT_CL_GAME_LOBBY_READY                    = 0x0C11;
        public const ushort NET_CHAT_CL_GAME_LOBBY_CREATE                   = 0x0C12;
        public const ushort NET_CHAT_CL_GAME_LOBBY_JOIN                     = 0x0C13; //player request to join the lobby
        public const ushort NET_CHAT_CL_GAME_LOBBY_INVITE                   = 0x0C14;
        public const ushort NET_CHAT_CL_GAME_LOBBY_REJECT_INVITE            = 0x0C15;
        public const ushort NET_CHAT_CL_GAME_LOBBY_ACCEPT_INVITE            = 0x0C16;
        public const ushort NET_CHAT_CL_GAME_LOBBY_KICK                     = 0x0C17; //bidirectional, tell a client that he's kcked, or tell a server to kick someone
        public const ushort NET_CHAT_CL_GAME_LOBBY_CHANGE_SLOT              = 0x0C18;
        public const ushort NET_CHAT_CL_GAME_LOBBY_REQUEST_LIST             = 0x0C19;
        public const ushort NET_CHAT_CL_GAME_LOBBY_RETURN                   = 0x0C1A;
    }

    public static class Matchmaking
    {
        public const ushort NET_CHAT_CL_TMM_GROUP_CREATE                    = 0x0C0A; // Client is requesting a new group be created
        public const ushort NET_CHAT_CL_TMM_GROUP_JOIN                      = 0x0C0B; // Client is joining a group
        public const ushort NET_CHAT_CL_TMM_GROUP_LEAVE                     = 0x0C0C; // Client is leaving a group
        public const ushort NET_CHAT_CL_TMM_GROUP_INVITE                    = 0x0C0D; // Client would like to invite someone to the group
        public const ushort NET_CHAT_CL_TMM_GROUP_INVITE_BROADCAST          = 0x0C0E; // Broadcast that a client would like to invite someone to the group
        public const ushort NET_CHAT_CL_TMM_GROUP_REJECT_INVITE             = 0x0C0F; // Client rejected invite
        public const ushort NET_CHAT_CL_CONNECT_TEST                        = 0x0C10;
        public const ushort NET_CHAT_CL_TMM_GROUP_KICK                      = 0x0D00; // The leader requested to kick a group member
        public const ushort NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE                = 0x0D01; // The group leader wants to join the queue for a match
        public const ushort NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE               = 0x0D02; // The group leader wants to leave the join match queue
        public const ushort NET_CHAT_CL_TMM_GROUP_UPDATE                    = 0x0D03; // Updates that occur whenever something in the group is updated
        public const ushort NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS     = 0x0D04; // Send updates on loading status
        public const ushort NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS       = 0x0D05; // Send updates on whether or not the player is ready
        public const ushort NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE              = 0x0D06; // Send information on the queue times to the group
        public const ushort NET_CHAT_CL_TMM_POPULARITY_UPDATE               = 0x0D07; // Send information on the popularities to all the groups
        public const ushort NET_CHAT_CL_TMM_GAME_OPTION_UPDATE              = 0x0D08; // Send group option updates to players when the group leader changes them
        public const ushort NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE              = 0x0D09; // Send team a match info update when a match is found
        public const ushort NET_CHAT_CL_TMM_SCHEDULED_MATCH_INFO            = 0x0E01; // Used to send scheduled match info to the clients
        public const ushort NET_CHAT_CL_TMM_SCHEDULED_MATCH_UPDATE          = 0x0E02; // Used to send specific scheduled match info to the clients
        public const ushort NET_CHAT_CL_TMM_SCHEDULED_MATCH_COMMAND         = 0x0E03; // Used to send commands from the client to the chat server
        public const ushort NET_CHAT_CL_TMM_SCHEDULED_MATCH_SERVER_INFO     = 0x0E04; // Send team a match info update when a match is found
        public const ushort NET_CHAT_CL_TMM_BOT_SPAWN_LOCAL_MATCH           = 0x0E05;
        public const ushort NET_CHAT_CL_TMM_SWAP_GROUP_TYPE                 = 0x0E06;
        public const ushort NET_CHAT_CL_TMM_BOT_GROUP_UPDATE                = 0x0E07;
        public const ushort NET_CHAT_CL_TMM_BOT_GROUP_BOTS                  = 0x0E08;
        public const ushort NET_CHAT_CL_TMM_BOT_NO_BOTS_SELECTED            = 0x0E09;
        public const ushort NET_CHAT_CL_TMM_FAILED_TO_JOIN                  = 0x0E0A; // Either TMM is disabled or they were not allowed to join due to being a leaver, being banned, or is not verified
        public const ushort NET_CHAT_CL_TMM_REGION_UNAVAILABLE              = 0x0E0B; // One of the regions the group leader selected is unavailable to one of the group members
        public const ushort NET_CHAT_CL_TMM_GROUP_REJOIN_QUEUE              = 0x0E0C; // Notify the group if they have been re-placed into the queue at their previous wait time
        public const ushort NET_CHAT_CL_TMM_GENERIC_RESPONSE                = 0x0E0D; // Used to send back generic responses back to the clients
        public const ushort NET_CHAT_CL_TMM_EVENTS_INFO                     = 0x0E0F; // Used to send event info to the clients
        public const ushort NET_CHAT_CL_TMM_SCHEDULED_MATCH_LOBBY_INFO      = 0x0F00; // For relaying to tournament admins details about the length of time each team was in the lobby and readied up
        public const ushort NET_CHAT_CL_TMM_LEAVER_INFO                     = 0x0F01; // If a player is unable to join due to being a leaver, send the client this data so the UI can display it to them
        public const ushort NET_CHAT_CL_TMM_REQUEST_READY_UP                = 0x0F02; // Group leader requests group members to ready up
        public const ushort NET_CHAT_CL_TMM_START_LOADING                   = 0x0F03; // All group members are ready - load!
        public const ushort NET_CHAT_CL_TMM_PENDING_MATCH                   = 0x0F04; // A match is waiting for your group
        public const ushort NET_CHAT_CL_TMM_ACCEPT_PENDING_MATCH            = 0x0F05; // A player has accepted the pending match
        public const ushort NET_CHAT_CL_TMM_FAILED_TO_ACCEPT_PENDING_MATCH  = 0x0F06; // A player in your group failed to accept the pending match
        public const ushort NET_CHAT_CL_TMM_CAMPAIGN_STATS                  = 0x0F07; // Player's campaign stats
        public const ushort NET_CHAT_CL_TMM_CHANGE_GROUP_TYPE               = 0x0F08;
        public const ushort NET_CHAT_CL_TMM_LEAVER_STRIKE_WARN              = 0x0F09; // Notify client that this user needs Leaver Strike popup warning
    }

    public static class ChatServerToClient
    {
        public const ushort NET_CHAT_CL_ACCEPT                              = 0x1C00; // Accept connection from client
        public const ushort NET_CHAT_CL_REJECT                              = 0x1C01; // Refuse connection from client
        public const ushort NET_CHAT_CL_CHANNEL_INFO                        = 0x1C02; // Basic information about a channel
        public const ushort NET_CHAT_CL_CHANNEL_LIST_SYN                    = 0x1C03; // HACK: until TCP connections are handled properly
        public const ushort NET_CHAT_CL_CHANNEL_SUBLIST_START               = 0x1C04; // Start of a channel sub-list
        public const ushort NET_CHAT_CL_CHANNEL_INFO_SUB                    = 0x1C05; // Basic information about a channel in a sublist
        public const ushort NET_CHAT_CL_CHANNEL_SUBLIST_SYN                 = 0x1C06; // HACK: until TCP connections are handled properly
        public const ushort NET_CHAT_CL_CHANNEL_SUBLIST_END                 = 0x1C07; // End of a channel sub-list
        public const ushort NET_CHAT_CL_USER_STATUS                         = 0x1C08; // User status request reponse
        public const ushort NET_CHAT_CL_GAME_LOBBY_JOINED                   = 0x1C09;
        public const ushort NET_CHAT_CL_GAME_LOBBY_LEFT                     = 0x1C0A;
        public const ushort NET_CHAT_CL_GAME_LOBBY_UPDATE                   = 0x1C0B;
        public const ushort NET_CHAT_CL_GAME_LOBBY_PLAYER_JOINED            = 0x1C0C;
        public const ushort NET_CHAT_CL_GAME_LOBBY_PLAYER_LEFT              = 0x1C0D;
        public const ushort NET_CHAT_CL_GAME_LOBBY_PLAYER_UPDATE            = 0x1C0E;
        public const ushort NET_CHAT_CL_GAME_LOBBY_LAUNCH_GAME              = 0x1C0F;
        public const ushort NET_CHAT_CL_GAME_LOBBY_LIST                     = 0x1C10;
        public const ushort NET_CHAT_CL_GAME_LOBBY_FULL                     = 0x1C11;
        public const ushort NET_CHAT_CL_GAME_LOBBY_SPEC_READY               = 0x1C12; // Tell the spectator others are in hero picking mode
    }

    public static class GameServerToChatServer
    {
        public const ushort NET_CHAT_GS_CONNECT                             = 0x0500; // Game server requesting connection
        public const ushort NET_CHAT_GS_DISCONNECT                          = 0x0501; // Game server disconnecting
        public const ushort NET_CHAT_GS_STATUS                              = 0x0502; // Game server's current status
        public const ushort NET_CHAT_GS_ANNOUNCE_MATCH                      = 0x0503; // An arranged match is ready for clients
        public const ushort NET_CHAT_GS_ABANDON_MATCH                       = 0x0504; // An arranged match failed to start
        public const ushort NET_CHAT_GS_MATCH_STARTED                       = 0x0505; // An arranged match has started successfully (all clients in match and banning/picking phase starting)
        public const ushort NET_CHAT_GS_REMIND_PLAYER                       = 0x0506; // An expected player has not yet connected to an arranged match
        public const ushort EMPTY_0x0507                                    = 0x0507;
        public const ushort NET_CHAT_GS_NOT_IDLE                            = 0x0508; // Server was not idle
        public const ushort NET_CHAT_GS_MATCH_ABORTED                       = 0x0509; // An arranged match failed to start
        public const ushort NET_CHAT_GS_SAVE_DISCONNECT_REASON              = 0x0510; // For tracking reasons we are aborting matches due to disconnected players
        public const ushort NET_CHAT_GS_REPORT_MISSING_PLAYERS              = 0x0511; // For tracking potential problem players abusing the MM system and causing games fail to start
        public const ushort NET_CHAT_GS_MATCH_ID_RESULT                     = 0x0512; // Game server letting the chat server know if we got a match ID
        public const ushort NET_CHAT_GS_CLIENT_AUTH_RESULT                  = 0x0513; // Was this client able to auth with the master server?
        public const ushort NET_CHAT_GS_STAT_SUBMISSION_RESULT              = 0x0514; // What result did the server receive when attempting to submit stats?
        public const ushort NET_CHAT_GS_MATCH_ENDED                         = 0x0515; // An arranged match has finished (stats have been submitted - or at least attempted to submit them)
        public const ushort NET_CHAT_GS_MATCH_ONGOING                       = 0x0516; // A match is a few minutes in and the game server is just letting the chatserver know
        public const ushort NET_CHAT_GS_PLAYER_BENEFITS                     = 0x0517; // Game server requests the benefits for a set of clients, chat server responds back with the benefits for each client
        public const ushort NET_CHAT_GS_REPORT_LEAVER                       = 0x0518; // For tracking leaver players
    }

    public static class ChatServerToGameServer
    {
        public const ushort NET_CHAT_GS_ACCEPT                              = 0x1500; // Accept connection from game server
        public const ushort NET_CHAT_GS_REJECT                              = 0x1501; // Refuse connection from game server
        public const ushort NET_CHAT_GS_CREATE_MATCH                        = 0x1502; // Game server has been selected to host an arranged match
        public const ushort NET_CHAT_GS_END_MATCH                           = 0x1503; // Game server has been told to end a match
        public const ushort NET_CHAT_GS_REMOTE_COMMAND                      = 0x1504; // Execute a console command on this server
        public const ushort NET_CHAT_GS_OPTIONS                             = 0x1505; // Various options to control cvars
        public const ushort NET_CHAT_GS_DYNAMIC_PRODUCTS                    = 0x1506; // NO COMMENT FOR YOU.
    }

    public static class ServerManagerToChatServer
    {
        public const ushort NET_CHAT_SM_CONNECT                             = 0x1600; // Server manager requesting connection
        public const ushort NET_CHAT_SM_DISCONNECT                          = 0x1601; // Server manager disconnecting
        public const ushort NET_CHAT_SM_STATUS                              = 0x1602; // Server manager's current status
        public const ushort NET_CHAT_SM_UPLOAD_UPDATE                       = 0x1603; // Server manager's response to a NET_CHAT_SM_UPLOAD_REQUEST
    }

    public static class ChatServerToServerManager
    {
        public const ushort NET_CHAT_SM_ACCEPT                              = 0x1700;   // Accept A Match Server Connection
        public const ushort NET_CHAT_SM_REJECT                              = 0x1701;   // Refuse A Match Server Connection
        public const ushort NET_CHAT_SM_REMOTE_COMMAND                      = 0x1702;   // Execute A Command On The Server Manager
        public const ushort NET_CHAT_SM_OPTIONS                             = 0x1703;   // Options To Set CVARs
        public const ushort NET_CHAT_SM_UPLOAD_REQUEST                      = 0x1704;   // The Match Replay Or Match Log That The Server Manager Is Requested To Upload
    }

    public static class MasterServerToChatServer
    {
        public const ushort NET_CHAT_WEB_UPLOAD_REQUEST                     = 0x1800;   // The Web Portal Is Requesting The Upload Of A Match Replay Or Match Log
        public const ushort NET_CHAT_QUERY_LEAVER_STRIKE                    = 0x1801;   // The Master Server Is Requesting Leaver Strike Information From The Chat Server
    }

    public const ushort NET_CHAT_INVALID                                    = 0xFFFF;

    public const byte CHAT_MESSAGE_MAX_LENGTH                               = 250;      // The Maximum Length Of Messages Sent To The Channel
    public const byte CHAT_CHANNEL_MAX_LENGTH                               = 35;       // The Maximum Length Of Channel Names
    public const byte CHAT_CHANNEL_TOPIC_MAX_LENGTH                         = 140;      // The Maximum Length Of Channel Topics

    public const int INVALID_CHAT_CHANNEL                                   = -1;

    public const uint MAX_USERS_PER_HON_CHANNEL                             = 50;
    public const uint MAX_USERS_PER_CHANNEL                                 = 250;

    public const uint MAX_TMM_GAME_MAPS_SELECTABLE                          = 1;
    public const uint MAX_TMM_GAME_MODES_SELECTABLE                         = 6;
    public const uint MAX_TMM_GAME_REGIONS_SELECTABLE                       = 6;

    public enum ChatRejectReason
    {
        ECR_UNKNOWN,
        ECR_BAD_VERSION,
        ECR_AUTH_FAILED,
        ECR_ACCOUNT_SHARING,
        ECR_ACCOUNT_SHARING_WARNING
    };

    public enum GameLobbyState
    {
        GLS_INVALID = -1,

        GLS_ADJUST_LOBBY,
        GLS_HERO_PICK,
        GLS_LAUNCHING,
        GLS_IN_STATS,

        NUM_GAME_LOBBY_STATES
    };

    /// <summary>
    ///     The chat server will send this to the match server on match start.
    ///     The game server will then send this to the master server for further processing.
    /// </summary>
    public enum ArrangedMatchType
    {
        AM_PUBLIC,                  // Public Match
        AM_MATCHMAKING,             // Ranked Normal/Casual Matchmaking
        AM_SCHEDULED_MATCH,         // Scheduled Tournament Match
        AM_UNSCHEDULED_MATCH,       // Unscheduled League Match
        AM_MATCHMAKING_MIDWARS,     // MidWars Matchmaking
        AM_MATCHMAKING_BOTMATCH,    // Bot Co-Op Matchmaking
        AM_UNRANKED_MATCHMAKING,    // Unranked Normal/Casual Matchmaking
        AM_MATCHMAKING_RIFTWARS,    // RiftWars Matchmaking
        AM_PUBLIC_PRELOBBY,
        AM_MATCHMAKING_CUSTOM,      // Custom Map Matchmaking
        AM_MATCHMAKING_CAMPAIGN,    // Ranked Season Normal/Casual Matchmaking

        NUM_ARRANGED_MATCH_TYPES
    };

    public static bool IsMatchmakingType(ArrangedMatchType arrangedMatchType)
    {
        return arrangedMatchType switch
        {
            ArrangedMatchType.AM_PUBLIC                 => false,
            ArrangedMatchType.AM_SCHEDULED_MATCH        => false,
            ArrangedMatchType.AM_UNSCHEDULED_MATCH      => false,

            ArrangedMatchType.AM_MATCHMAKING            => true,
            ArrangedMatchType.AM_MATCHMAKING_MIDWARS    => true,
            ArrangedMatchType.AM_MATCHMAKING_RIFTWARS   => true,
            ArrangedMatchType.AM_MATCHMAKING_BOTMATCH   => true,
            ArrangedMatchType.AM_UNRANKED_MATCHMAKING   => true,
            ArrangedMatchType.AM_MATCHMAKING_CUSTOM     => true,
            ArrangedMatchType.AM_MATCHMAKING_CAMPAIGN   => true,

            _                                           => false
        };
    }

    public enum TMMUpdateType
    {
        TMM_CREATE_GROUP,
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

    public enum LobbyUpdateType
    {
        LOBBY_CLIENT_ENTER,         // Message The Client That Another Non-Bot Player Has Joined
        LOBBY_CLIENT_ON_ENTER,      // Message All Clients That A Human/Bot Player Has Joined
        LOBBY_CLIENT_ON_LEAVE,      // Message All Clients That A Human/Bot Player Has Left
        LOBBY_CLIENT_ON_CHANGE,     // Message All Clients That A Human/Bot Player Has Changed Slot
        LOBBY_STATE_UPDATE,         // Message All Clients That A State Update Has Occured
        LOBBY_INFO_UPDATE,          // Message All Clients That An Information Update Has Occured

        LOBBY_UPDATE_TYPE_NUM
    };

    public static readonly string[] TMMUpdateTypes =
    {
        "TMM_CREATE_GROUP",
        "TMM_FULL_GROUP_UPDATE",
        "TMM_PARTIAL_GROUP_UPDATE",
        "TMM_PLAYER_JOINED_GROUP",
        "TMM_PLAYER_LEFT_GROUP",
        "TMM_PLAYER_KICKED_FROM_GROUP",
        "TMM_GROUP_JOINED_QUEUE",
        "TMM_GROUP_REJOINED_QUEUE",
        "TMM_GROUP_LEFT_QUEUE",
        "TMM_INVITED_TO_GROUP",
        "TMM_PLAYER_REJECTED_GROUP_INVITE",
        "TMM_GROUP_QUEUE_UPDATE",
        "TMM_GROUP_NO_MATCHES_FOUND",
        "TMM_GROUP_NO_SERVERS_FOUND",
        "TMM_POPULARITY_UPDATE",
        "TMM_FOUND_MATCH_UPDATE",
        "TMM_GROUP_FOUND_SERVER",
        "TMM_MATCHMAKING_DISABLED"
    };

    public enum TMMFailedToJoinReason
    {
        TMMFTJR_LEAVER,             // A Player Has Left The Matchmaking Group
        TMMFTJR_DISABLED,           // Matchmaking Is Disabled
        TMMFTJR_BUSY,               // Matchmaking Is Unable To Accept Additional Players
        TMMFTJR_OPTION_UNAVAILABLE, // A Selected Matchmaking Option Is Unavailable
        TMMFTJR_INVALID_VERSION,    // The Game Client Version Does Not Match The Latest Version
        TMMFTJR_GROUP_FULL,         // The Matchmaking Group Is Full
        TMMFTJR_BAD_STATS,          // Unable To Retrieve Player Statistics
        TMMFTJR_ALREADY_QUEUED,     // The Matchmaking Group Is Already In A Queue
        TMMFTJR_TRIAL,              // Matchmaking Is Unavailable For Trial Accounts
        TMMFTJR_BANNED,             // The Account Is Banned From Matchmaking
        TMMFTJR_LOBBY_FULL,
        TMMFTJR_WRONG_PASSWORD,
        TMMFTJR_CAMPAIGN_NOT_ELIGIBLE
    };

    public enum ScheduledMatchUpdateType
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

    public enum ServerStatus
    {
        SERVER_STATUS_SLEEPING,
        SERVER_STATUS_IDLE,
        SERVER_STATUS_LOADING,
        SERVER_STATUS_ACTIVE,
        SERVER_STATUS_CRASHED,
        SERVER_STATUS_KILLED,

        SERVER_STATUS_UNKNOWN
    };

    public enum MatchAbortedReason
    {
        MATCH_ABORTED_UNKNOWN,

        MATCH_ABORT_CONNECT_TIMEOUT,
        MATCH_ABORT_START_TIMEOUT,
        MATCH_ABORT_PLAYER_LEFT,
        MATCH_ABORT_NEVERENDING
    };

    public enum MatchEndedReason
    {
        MATCH_ENDED_FINISHED,
        MATCH_ENDED_REMADE
    };

    public enum ChatModeType
    {
        CHAT_MODE_AVAILABLE,
        CHAT_MODE_AFK,
        CHAT_MODE_DND,
        CHAT_MODE_INVISIBLE
    };

    public enum AdminLevel
    {
        CHAT_CLIENT_ADMIN_NONE,
        CHAT_CLIENT_ADMIN_OFFICER,
        CHAT_CLIENT_ADMIN_LEADER,
        CHAT_CLIENT_ADMIN_ADMINISTRATOR,
        CHAT_CLIENT_ADMIN_STAFF,
        CHAT_NUM_ADMIN_LEVELS
    };

    public enum MatchIDResult
    {
        MIDR_FIRST,

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

        NUM_MATCH_ID_RESULTS
    };

    public enum ClientAuthResult
    {
        CAR_FIRST,

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

        NUM_CLIENT_AUTH_RESULTS
    };

    public enum StatSubmissionResult
    {
        SSR_FIRST,

        SSR_SUCCESS = SSR_FIRST,
        SSR_ERROR_UNSUCCESSFUL,
        SSR_ERROR_EMPTY_RESPONSE,
        SSR_ERROR_EMPTY_ARRAY,
        SSR_ERROR_INVALID_RESPONSE,
        SSR_ERROR_CONNECT,
        SSR_LAST_ERROR = SSR_ERROR_CONNECT,

        SSR_FAILED,

        NUM_STAT_SUBMISSION_RESULTS
    };

    public enum PlayerSpectateRequest
    {
        PLAYER_SPECTATE_REQUEST,
        PLAYER_SPECTATE_REQUEST_RESPONSE
    };

    public enum PlayerSpectateRequestResponses
    {
        PSRR_ALLOW,
        PSRR_DENY,
        PSRR_FULL_PLAYER,
        PSRR_FULL_SERVER,
        PSSR_TOO_DEEP
    };

    public enum MatchmakingBroadcastTracking
    {
        MM_MATCH_FOUND,
        MM_SERVER_FOUND,
        MM_MATCH_READY,
        MM_SERVER_NOT_IDLE,
        MM_MATCH_READY_REMINDER,

        NUM_MATCHMAKING_BROADCASTS
    };

    public enum ActionCampaigns
    {
        AC_DAILY_LOGINS,
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

    public enum GenericResponses
    {
        GR_MAX_MATCH_FIDELITY_DIFFERENCE,
        GR_SCHEDULED_MATCH_FULL,

        NUM_GENERIC_RESPONSES
    };

    public enum OSType
    {
        UNKNOWN_OS,
        WINDOWS_OS,
        APPLE_OS,
        LINUX_OS,

        NUM_OS_TYPES
    };

    public enum OSVersion
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

    public enum ExcessiveGamePlayType
    {
        EEGPT_NONE,                     // No Broadcast For Excessively Long Player Sessions
        EEGPT_1_HOUR,                   // The Player's Session Has Reached A Duration Of 1 Hour
        EEGPT_2_HOURS,                  // The Player's Session Has Reached A Duration Of 2 Hours
        EEGPT_FATIGUE,                  // The Player Has Been Online For An Extensive Amount Of Time, And Their Post-Match Rewards Are Halved
        EEGPT_UNHEALTHY,                // The Player Has Been Online For Too Long, And They No Longer Receive Post-Match Rewards
        EEGPT_GENERAL = 255             // A General Broadcast For Excessively Long Player Sessions
    };

    public enum ExcessiveGamePlayBenefit
    {
        EEGPB_NORMAL,
        EEGPB_HALF,
        EEGPB_NONE
    };

    public enum CrashReportingClientState
    {
        CRCS_NO_CRASH,                  // The Client Has Exited Normally
        CRCS_IDLE,                      // The Client Is Idle (Before Joining A Match)
        CRCS_CONNECTING,                // The Client Is Connecting To A Server
        CRCS_LOADING,                   // The Client Is Loading Map Resources
        CRCS_LOBBY,                     // The Client Is In The Game Lobby
        CRCS_LOADING_HEROES,            // The Client Is Loading Heroes
        CRCS_IN_GAME,                   // The Client Is In-Game
        CRCS_DISCONNECTING_NEXT_FRAME,  // The Client Will Disconnect Next Frame (One Of The "Disconnect" Commands Was Called)
        CRCS_DISCONNECTING,             // The Client Is Disconnecting From The Server
        CRCS_UNLOADING_RESOURCES,       // The Client Is Unloading Map Resources
        CRCS_UNKNOWN,                   // The Client Has Crashed At An Unknown State
        CRCS_UPDATING,                  // The Client Is Updating
        NUM_CRCS
    };

    public enum ChatClientStatus
    {
        CHAT_CLIENT_STATUS_DISCONNECTED,
        CHAT_CLIENT_STATUS_CONNECTING,
        CHAT_CLIENT_STATUS_WAITING_FOR_AUTH,
        CHAT_CLIENT_STATUS_CONNECTED,
        CHAT_CLIENT_STATUS_JOINING_GAME,
        CHAT_CLIENT_STATUS_IN_GAME,

        NUM_CHAT_CLIENT_STATUSES
    };

    public enum TMMGameTypes
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

    public enum TMMTypes
    {
        TMM_TYPE_SOLO = 1,
        TMM_TYPE_PVP = 2,
        TMM_TYPE_COOP = 3,
        TMM_TYPE_CAMPAIGN = 4
    };

    public enum TMMGameMaps
    {
        TMM_GAME_MAP_NONE = -1,

        TMM_GAME_MAP_FORESTS_OF_CALDAVAR,
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

    public enum TMMGameModes
    {
        TMM_GAME_MODE_NONE = -1,

        TMM_GAME_MODE_ALL_PICK,
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

    public enum TMMGameRegions
    {
        TMM_GAME_REGION_NONE = -1,

        TMM_GAME_REGION_USE,
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

    public enum TMMRankType
    {
        TMM_OPTION_UNRANKED,
        TMM_OPTION_RANKED,

        TMM_NUM_OPTION_RANK_TYPES
    };

    public enum UploadUpdateType
    {
        EUUT_NONE = -1,
        EUUT_GENERAL_FAILURE,
        EUUT_FILE_DOES_NOT_EXIST,
        EUUT_FILE_INVALID_HOST,
        EUUT_FILE_ALREADY_UPLOADED,
        EUUT_FILE_ALREADY_QUEUED,
        EUUT_FILE_QUEUED,
        EUUT_FILE_UPLOADING,
        EUUT_FILE_UPLOAD_COMPLETE
    };

    public enum DisconnectReason
    {
        DISCONNECT_INVALID,
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
    };

    public enum QuestsAvailabilityType
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

    [Flags]
    public enum ChatClientType
    {
        CHAT_CLIENT_IS_OFFICER          = 1 << 0,
        CHAT_CLIENT_IS_CLAN_LEADER      = 1 << 1,
        CHAT_CLIENT_IS_STAFF            = 1 << 5,
        CHAT_CLIENT_IS_PREMIUM          = 1 << 6,
        CHAT_CLIENT_IS_VERIFIED         = 1 << 7
    }

    [Flags]
    public enum ChatChannelType
    {
        CHAT_CHANNEL_FLAG_PERMANENT     = 1 << 0,
        CHAT_CHANNEL_FLAG_SERVER        = 1 << 1, // The Channel For Post-Match Chat
        CHAT_CHANNEL_FLAG_HIDDEN        = 1 << 2,
        CHAT_CHANNEL_FLAG_RESERVED      = 1 << 3, // System-Created Channels (e.g. General, Clan, Stream, etc.)
        CHAT_CHANNEL_FLAG_GENERAL_USE   = 1 << 4, // e.g. KONGOR 1, KONGOR 2, etc.
        CHAT_CHANNEL_FLAG_UNJOINABLE    = 1 << 5,
        CHAT_CHANNEL_FLAG_AUTH_REQUIRED = 1 << 6,
        CHAT_CHANNEL_FLAG_CLAN          = 1 << 7,

        CHAT_CHANNEL_FLAG_STREAM_USE    = 1 << 8
    }

    [Flags]
    public enum ServerType
    {
        SSF_OFFICIAL                    = 1 << 0,
        SSF_OFFICIAL_WITH_STATS         = 1 << 1,
        SSF_NOLEAVER                    = 1 << 2,
        SSF_VERIFIED_ONLY               = 1 << 3,
        SSF_PRIVATE                     = 1 << 4,
        SSF_TIER_NOOBS_ALLOWED          = 1 << 5,
        SSF_TIER_PRO                    = 1 << 6,
        SSF_ALL_HEROES                  = 1 << 7,
        SSF_CASUAL                      = 1 << 8,
        SSF_GATED                       = 1 << 9,
        SSF_FORCE_RANDOM                = 1 << 10,
        SSF_AUTO_BALANCE                = 1 << 11,
        SSF_ADV_OPTIONS                 = 1 << 12,
        SSF_DEV_HEROES                  = 1 << 13,
        SSF_HARDCORE                    = 1 << 14
    }

    public struct TMMPopularities
    {
        public byte [ /* TMM_NUM_GAME_TYPES   */ ] [ /* TMM_NUM_GAME_MAPS  */ ] [ /* TMM_NUM_OPTION_RANK_TYPES */ ]                                     GameType { get; set; }
        public byte [ /* TMM_NUM_GAME_MAPS    */ ] [ /* TMM_NUM_GAME_TYPES */ ] [ /* TMM_NUM_OPTION_RANK_TYPES */ ]                                     GameMap  { get; set; }
        public byte [ /* TMM_NUM_GAME_MODES   */ ] [ /* TMM_NUM_GAME_MAPS  */ ] [ /* TMM_NUM_GAME_TYPES        */ ] [ /* TMM_NUM_OPTION_RANK_TYPES */ ] GameMode { get; set; }
        public byte [ /* NUM_TMM_GAME_REGIONS */ ] [ /* TMM_NUM_GAME_MAPS  */ ] [ /* TMM_NUM_GAME_TYPES        */ ] [ /* TMM_NUM_OPTION_RANK_TYPES */ ] Region   { get; set; }

        public void Clear()
        {
            for (uint i = 0; i < Convert.ToInt32(TMMGameTypes.TMM_NUM_GAME_TYPES); ++i)
                for (uint j = 0; j < Convert.ToInt32(TMMGameMaps.TMM_NUM_GAME_MAPS); ++j)
                    for (uint k = 0; k < Convert.ToInt32(TMMRankType.TMM_NUM_OPTION_RANK_TYPES); ++k)
                        GameType[i][j][k] = 0;

            for (uint i = 0; i < Convert.ToInt32(TMMGameMaps.TMM_NUM_GAME_MAPS); ++i)
                for (uint j = 0; j < Convert.ToInt32(TMMGameTypes.TMM_NUM_GAME_TYPES); ++j)
                    for (uint k = 0; k < Convert.ToInt32(TMMRankType.TMM_NUM_OPTION_RANK_TYPES); ++k)
                        GameMap[i][j][k] = 0;

            for (uint i = 0; i < Convert.ToInt32(TMMGameModes.TMM_NUM_GAME_MODES); ++i)
                for (uint j = 0; j < Convert.ToInt32(TMMGameMaps.TMM_NUM_GAME_MAPS); ++j)
                    for (uint k = 0; k < Convert.ToInt32(TMMGameTypes.TMM_NUM_GAME_TYPES); ++k)
                        for (uint l = 0; l < Convert.ToInt32(TMMRankType.TMM_NUM_OPTION_RANK_TYPES); ++l)
                            GameMode[i][j][k][l] = 0;

            for (uint i = 0; i < Convert.ToInt32(TMMGameRegions.NUM_TMM_GAME_REGIONS); ++i)
                for (uint j = 0; j < Convert.ToInt32(TMMGameMaps.TMM_NUM_GAME_MAPS); ++j)
                    for (uint k = 0; k < Convert.ToInt32(TMMGameTypes.TMM_NUM_GAME_TYPES); ++k)
                        for (uint l = 0; l < Convert.ToInt32(TMMRankType.TMM_NUM_OPTION_RANK_TYPES); ++l)
                            Region[i][j][k][l] = 0;
        }
    };

    public struct RosterInfo(uint accountID = 0, byte teamSlot = 0)
    {
        public uint AccountID { get; set; } = accountID;

        public byte TeamSlot { get; set; } = teamSlot;
    };
}
