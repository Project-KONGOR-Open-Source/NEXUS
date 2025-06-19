namespace TRANSMUTANSTEIN.ChatServer.Matchmaking;

public class MatchmakingGroup
{
    public enum GroupState
    {
        WaitingToStart,
        LoadingResources,
        InQueue
    }

    public enum GroupUpdateType
    {
        GroupCreated = 0,
        Full = 1,
        Partial = 2,
        ParticipantAdded = 3,
        ParticipantRemoved = 4,
        ParticipantKicked = 5
    }

    public class Participant
    {
        public int AccountId { get; init; }
        public ClientInformation ClientInformation { get; init; }
        public byte LoadingStatus { get; set; }
        public bool TopOfTheQueue { get; init; }

        public Participant(int accountId, ClientInformation clientInformation, bool topOfTheQueue = false)
        {
            AccountId = accountId;
            ClientInformation = clientInformation;
            LoadingStatus = 0;
            TopOfTheQueue = topOfTheQueue;
        }
    }

    private Participant[] _participants = [];
    private static int _nextGroupId = 0;
    private readonly int _groupId = Interlocked.Increment(ref _nextGroupId);

    private readonly ChatProtocol.TMMType _groupType;
    private readonly ChatProtocol.TMMGameType _gameType;
    private readonly string _gameModes;
    private readonly string _regions;
    private readonly bool _ranked;
    private readonly bool _matchFidelity;
    private readonly byte _botDifficulty;
    private readonly bool _randomizeBots;
    
    private GroupState _state = GroupState.WaitingToStart;
    private ChatChannel? _chatChannel = null;
    private readonly byte _maxGroupSize;

    public MatchmakingGroup(ChatProtocol.TMMType groupType, ChatProtocol.TMMGameType gameType, 
        string gameModes, string regions, bool ranked, bool matchFidelity, 
        byte botDifficulty, bool randomizeBots, byte maxGroupSize = 5)
    {
        _groupType = groupType;
        _gameType = gameType;
        _gameModes = gameModes;
        _regions = regions;
        _ranked = ranked;
        _matchFidelity = matchFidelity;
        _botDifficulty = botDifficulty;
        _randomizeBots = randomizeBots;
        _maxGroupSize = maxGroupSize;
    }    public int GroupId => _groupId;
    public GroupState State => _state;
    public IReadOnlyList<Participant> Participants => _participants.ToList().AsReadOnly();
    public int ParticipantCount => _participants.Length;
    public bool IsFull => _participants.Length >= _maxGroupSize;
    public Participant? Leader => _participants.Length > 0 ? _participants[0] : null;
    public ChatChannel? ChatChannel => _chatChannel;
    public ChatProtocol.TMMGameType GameType => _gameType;
    public string Regions => _regions;
    public bool Ranked => _ranked;
    public DateTime QueueJoinTime { get; private set; } = DateTime.UtcNow;
      public float AverageRating => _participants.Length > 0 ? _participants.Average(p => 1500f) : 0f;
    public float RatingDisparity => _participants.Length > 0 ? 
        _participants.Max(p => 1500f) - 
        _participants.Min(p => 1500f) : 0f;

    public bool AddParticipant(ChatSession session, out ChatProtocol.TMMFailedToJoinReason failureReason)
    {
        while (true)
        {
            Participant[] oldParticipants = _participants;
            
            if (oldParticipants.Length >= _maxGroupSize)
            {
                failureReason = ChatProtocol.TMMFailedToJoinReason.TMMFTJR_GROUP_FULL;
                return false;
            }

            if (oldParticipants.Any(p => p.AccountId == session.ClientInformation.Account.ID))
            {
                failureReason = ChatProtocol.TMMFailedToJoinReason.TMMFTJR_GROUP_FULL;
                return false;
            }

            Participant[] newParticipants = new Participant[oldParticipants.Length + 1];
            Array.Copy(oldParticipants, newParticipants, oldParticipants.Length);

            newParticipants[oldParticipants.Length] = new Participant(
                session.ClientInformation.Account.ID, 
                session.ClientInformation, 
                false);

            lock (this)
            {
                if (_state != GroupState.WaitingToStart)
                {
                    failureReason = ChatProtocol.TMMFailedToJoinReason.TMMFTJR_ALREADY_QUEUED;
                    return false;
                }

                if (Interlocked.CompareExchange(ref _participants, newParticipants, oldParticipants) == oldParticipants)
                {
                    failureReason = ChatProtocol.TMMFailedToJoinReason.TMMFTJR_ALREADY_QUEUED;

                    if (newParticipants.Length == 1)
                    {
                        BroadcastUpdate(GroupUpdateType.GroupCreated, newParticipants);
                        return true;
                    }
                    else if (newParticipants.Length == 2)
                    {
                        CreateGroupChatChannel();
                    }

                    if (_chatChannel != null)
                    {
                        Context.MatchmakingGroupChatChannels.TryAdd(session.ClientInformation.Account.ID, _chatChannel);
                    }

                    BroadcastUpdate(GroupUpdateType.Full, newParticipants);
                    return true;
                }
            }
        }
    }

    public void RemoveParticipant(ChatSession session)
    {
        bool tryAgain = true;
        while (tryAgain)
        {
            tryAgain = false;
            Participant[] oldParticipants = _participants;
            
            for (int i = 0; i < oldParticipants.Length; i++)
            {
                if (oldParticipants[i].AccountId == session.ClientInformation.Account.ID)
                {
                    Participant[] newParticipants = new Participant[oldParticipants.Length - 1];
                    Array.Copy(oldParticipants, newParticipants, i);
                    Array.Copy(oldParticipants, i + 1, newParticipants, i, oldParticipants.Length - 1 - i);

                    if (Interlocked.CompareExchange(ref _participants, newParticipants, oldParticipants) == oldParticipants)
                    {
                        Context.MatchmakingGroupChatChannels.TryRemove(session.ClientInformation.Account.ID, out _);

                        if (oldParticipants.Length == 1)
                        {
                            if (_chatChannel != null)
                            {
                                Context.MatchmakingGroupChatChannels.TryRemove(session.ClientInformation.Account.ID, out _);
                            }
                            return;
                        }

                        BroadcastUpdate(GroupUpdateType.ParticipantRemoved, newParticipants, session.ClientInformation.Account.ID);
                        return;
                    }
                    else
                    {
                        tryAgain = true;
                        break;
                    }
                }
            }
        }
    }

    public void EnterQueue()
    {
        Participant[] participants;
        lock (this)
        {
            if (_state == GroupState.LoadingResources || _state == GroupState.InQueue)
            {
                return;
            }

            _state = GroupState.LoadingResources;
            participants = _participants;
        }

        BroadcastUpdate(GroupUpdateType.Partial, participants);
        BroadcastStartLoading(participants);
    }

    public void LeaveQueue(bool initiatedByGameFinder = false)
    {
        Participant[] participants;
        lock (this)
        {
            switch (_state)
            {
                case GroupState.WaitingToStart:
                    return;
                case GroupState.LoadingResources:
                    break;
                case GroupState.InQueue:
                    break;
            }

            _state = GroupState.WaitingToStart;
            participants = _participants;
        }

        BroadcastUpdate(GroupUpdateType.Partial, participants);
        BroadcastLeftQueue(participants);
    }

    public void NotifyLoadingStatusChanged(int accountId, byte loadingStatus)
    {
        Participant[] participants = _participants;

        bool everyoneLoaded = true;
        foreach (Participant participant in participants)
        {
            if (participant.AccountId == accountId)
            {
                participant.LoadingStatus = loadingStatus;
            }
            if (participant.LoadingStatus != 100)
            {
                everyoneLoaded = false;
            }
        }

        bool enteredMatchmaking = false;
        if (everyoneLoaded)
        {
            lock (this)
            {
                if (_state == GroupState.LoadingResources)
                {
                    if (AddToGameFinderQueue(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()))
                    {
                        _state = GroupState.InQueue;
                        enteredMatchmaking = true;
                    }
                    else
                    {
                        _state = GroupState.WaitingToStart;
                    }
                }
            }
        }

        BroadcastUpdate(GroupUpdateType.Partial, participants);

        if (enteredMatchmaking)
        {
            BroadcastEnteredQueue(participants);
        }
    }    private void CreateGroupChatChannel()
    {
        string channelName = $"Group #{_groupId}";
        _chatChannel = new ChatChannel
        {
            Name = channelName,
            Flags = ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_HIDDEN
        };
        Context.ChatChannels.TryAdd(channelName.ToUpperInvariant(), _chatChannel);
    }

    private bool AddToGameFinderQueue(long timestampWhenJoinedQueue)
    {
        // Add this group to the appropriate queue based on group size
        var groupDictionary = _participants.Length switch
        {
            1 => MatchmakingService.SoloPlayerGroups,
            2 => MatchmakingService.TwoPlayerGroups,
            3 => MatchmakingService.ThreePlayerGroups,
            4 => MatchmakingService.FourPlayerGroups,
            5 => MatchmakingService.FivePlayerGroups,
            _ => null
        };

        if (groupDictionary != null && Leader != null)
        {
            groupDictionary.TryAdd(Leader.AccountId, this);
            return true;
        }

        return false;
    }

    private void BroadcastStartLoading(Participant[] participants)
    {
        var response = new MatchmakingStartLoadingResponse();
        BroadcastToParticipants(response, participants);
    }

    private void BroadcastLeftQueue(Participant[] participants)
    {
        var response = new LeftMatchmakingQueueResponse();
        BroadcastToParticipants(response, participants);
    }

    private void BroadcastEnteredQueue(Participant[] participants)
    {
        var response = new EnteredMatchmakingQueueResponse();
        BroadcastToParticipants(response, participants);
    }    private void BroadcastToParticipants<T>(T response, Participant[] participants) where T : IMatchmakingResponse
    {
        byte[] responseData = response.Serialize();
        foreach (Participant participant in participants)
        {
            if (Context.ChatSessions.TryGetValue(participant.ClientInformation.Account.Name, out var session))
            {
                session.SendAsync(responseData);
            }
        }
    }

    private void BroadcastUpdate(GroupUpdateType updateType, Participant[] participants, int removedOrKickedAccountId = 0)
    {
        foreach (Participant participant in participants)
        {            var matchmakingGroupUpdateResponse = new MatchmakingGroupUpdateResponse(
                UpdateType: Convert.ToByte(updateType),
                AccountId: removedOrKickedAccountId,
                GroupSize: Convert.ToByte(participants.Length),
                AverageTMR: Convert.ToInt16(AverageRating),
                LeaderAccountId: participants.Length > 0 ? participants[0].AccountId : 0,
                Unknown1: 1,
                GameType: _gameType,
                MapName: GetMapName(_gameType),
                GameModes: _gameModes,
                Regions: _regions,
                Ranked: _ranked,
                MatchFidelity: _matchFidelity,                BotDifficulty: _botDifficulty,
                RandomizeBots: _randomizeBots,
                Unknown2: "",
                PlayerInvitationResponses: "",
                TeamSize: _maxGroupSize,
                GroupType: _groupType,
                GroupParticipants: CreateGroupParticipants(participants),
                FriendshipStatus: new byte[participants.Length],
                IsLoadingResources: _state == GroupState.LoadingResources
            );            if (Context.ChatSessions.TryGetValue(participant.ClientInformation.Account.Name, out var session))
            {
                byte[] responseData = matchmakingGroupUpdateResponse.Serialize();
                session.SendAsync(responseData);
            }
        }
    }

    private List<MatchmakingGroupUpdateResponse.GroupParticipant> CreateGroupParticipants(Participant[] participants)
    {
        var groupParticipants = new List<MatchmakingGroupUpdateResponse.GroupParticipant>();

        for (int i = 0; i < participants.Length; i++)
        {
            var participant = participants[i];
            bool isReady = participant == participants[0] ? _state != GroupState.WaitingToStart : true;

            groupParticipants.Add(new MatchmakingGroupUpdateResponse.GroupParticipant(
                AccountId: participant.AccountId,
                Name: participant.ClientInformation.Account.Name,
                Slot: Convert.ToByte(i),                NormalRankLevel: 1500,
                CasualRankLevel: 1500,
                NormalRanking: 5,
                CasualRanking: 5,
                EligibleForCampaign: 1,
                Rating: 1500,
                LoadingPercent: participant.LoadingStatus,
                ReadyStatus: Convert.ToByte(isReady),
                InGame: 0,
                Verified: 1,
                ChatNameColor: "",
                AccountIcon: "",
                Country: "US",
                GameModeAccessBool: 1,
                GameModeAccessString: _gameModes
            ));
        }

        return groupParticipants;
    }    private static string GetMapName(ChatProtocol.TMMGameType gameType)
    {
        return gameType switch
        {
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS => "midwars",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_RIFTWARS => "riftwars",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL => "caldavar",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL => "caldavar",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL => "caldavar_old",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL => "caldavar_old",
            _ => $"unknown#{gameType}"
        };
    }    public void StartLoadingResources()
    {
        _state = GroupState.LoadingResources;
        
        // Notify all participants about the state change
        BroadcastUpdate(GroupUpdateType.Partial, _participants);
    }
}
