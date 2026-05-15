using GameLogic.Attributes;
using WebServer.Shared.GameObjects;

namespace WebServer.Shared.HubObjects
{
    /// <summary>
    /// The complex context that provides all info of the entire game
    /// with its history.
    /// </summary>
    public record class EndGameContext : Context
    {
        public required Team? Winner { get; init; }
        public required EndGamePlayerState[] Players { get; init; }
        public required EndGameHistory[] History { get; init; }

        public sealed record class EndGamePlayerState
        {
            public required ulong Id { get; init; }
            public required string Nickname {  get; init; }
            public required RGB NColor { get; init; }
            public required RoleSignature Role { get; init; }
            public required bool IsAlive { get; init; }
        }

        public sealed record class EndGameHistory
        {
            public required int Turn { get; init; }
            public required string? DayNicknameElected { get; init; }
            public required RGB DayNColor { get; init; }
            public required int DayVotesCount { get; init; }
            public required string? LynchLastMessage { get; init; }
            public required EndGameNight[] NightActions { get; init; }
            public required string[] MorningDeathNicknames { get; init; }

            public sealed record class EndGameNight
            {
                public required string Executor { get; init; }
                public required RoleSignature ExecutorRole { get; init; }
                public required string[] Targets { get; init; }
                public required bool Success { get; init; }
            }
        }
    }
}