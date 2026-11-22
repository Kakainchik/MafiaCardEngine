using WebServer.Model.User;
using WebServer.Shared.ParanoiaCorp.GameObjects;

namespace WebServer.Model.ParanoiaCorp
{
    public class RoleInfoDTO
    {
        public byte Signature { get; set; }
        public string RoleKey { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameRu { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionRu { get; set; } = string.Empty;
        public string AbilityEn { get; set; } = string.Empty;
        public string AbilityRu { get; set; } = string.Empty;
        public byte Team { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamNameRu { get; set; } = string.Empty;
        public string FactionIndicator { get; set; } = string.Empty;
        public string FactionColor { get; set; } = string.Empty;
        public bool IsUnique { get; set; }
        public string ExecutorType { get; set; } = string.Empty;
    }

    public class ParanoiaRoomDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public UserDTO Host { get; set; } = null!;
        public List<ParanoiaRoomPlayerDTO> Players { get; set; } = new();
        public int MaxSeats { get; set; }
        public Dictionary<string, int> RolesPool { get; set; } = new();
        public int TotalRolesCount { get; set; }
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public bool IsGameStarted { get; set; }
    }

    public class ParanoiaRoomPlayerDTO
    {
        public ulong Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsReady { get; set; }
        public bool IsHost { get; set; }
    }

    public class ParanoiaPlayerViewDTO
    {
        public int RoomId { get; set; }
        public string Phase { get; set; } = string.Empty; // "Intro", "Overtime", "Morning", "BoardMeeting", "Evening", "EndGame"
        public string PhaseTitleEn { get; set; } = string.Empty;
        public string PhaseTitleRu { get; set; } = string.Empty;
        public int FiscalTurn { get; set; }
        public string DirectiveEn { get; set; } = string.Empty;
        public string DirectiveRu { get; set; } = string.Empty;

        public PlayerSelfDTO Self { get; set; } = null!;
        public List<PlayerRosterDTO> Roster { get; set; } = new();

        public OvertimeActionDTO? OvertimeAction { get; set; }
        public BoardMeetingDTO? BoardMeeting { get; set; }
        public EveningDTO? Evening { get; set; }
        public EndGameDTO? EndGame { get; set; }

        public List<NewsItemDTO> PublicNews { get; set; } = new();
        public List<ReportItemDTO> ConfidentialReports { get; set; } = new();
        public List<ChatScopeInfoDTO> AvailableChatScopes { get; set; } = new();
    }

    public class PlayerSelfDTO
    {
        public ulong Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsAlive { get; set; }
        public byte RoleSignature { get; set; }
        public string RoleKey { get; set; } = string.Empty;
        public string RoleNameEn { get; set; } = string.Empty;
        public string RoleNameRu { get; set; } = string.Empty;
        public string Faction { get; set; } = string.Empty;
        public string FactionRu { get; set; } = string.Empty;
        public string FactionColor { get; set; } = string.Empty;
        public string PortraitKey { get; set; } = string.Empty;
        public string AbilityEn { get; set; } = string.Empty;
        public string AbilityRu { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionRu { get; set; } = string.Empty;
        public string? UsageLimitsEn { get; set; }
        public string? UsageLimitsRu { get; set; }
        public bool IsGeneralDirector { get; set; }
        public int VetoCount { get; set; }
    }

    public class PlayerRosterDTO
    {
        public ulong Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsAlive { get; set; }
        public bool IsReady { get; set; }
        public bool IsHost { get; set; }
        public string? RevealedRoleKey { get; set; }
        public string? RevealedRoleNameEn { get; set; }
        public string? RevealedRoleNameRu { get; set; }
        public string? RevealedFaction { get; set; }
        public string? RevealedFactionRu { get; set; }
        public string? RevealedFactionColor { get; set; }
        public string StatusEn { get; set; } = "Active";
        public string StatusRu { get; set; } = "В строю";
    }

    public class OvertimeActionDTO
    {
        public bool CanAct { get; set; }
        public string ExecutorType { get; set; } = "NONE"; // NONE, TARGET, TARGET_TARGET, TARGET_ANYTARGET
        public string Target1LabelEn { get; set; } = "Target Employee";
        public string Target1LabelRu { get; set; } = "Сотрудник-цель";
        public string? Target2LabelEn { get; set; }
        public string? Target2LabelRu { get; set; }
        public List<TargetOptionDTO> EligibleTargets1 { get; set; } = new();
        public List<TargetOptionDTO> EligibleTargets2 { get; set; } = new();
        public ulong[] PendingSelections { get; set; } = Array.Empty<ulong>();
        public bool IsActionSubmitted { get; set; }
        public bool IsReady { get; set; }
    }

    public class TargetOptionDTO
    {
        public ulong Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class BoardMeetingDTO
    {
        public List<TargetOptionDTO> CandidatesForFiring { get; set; } = new();
        public bool IsGeneralDirector { get; set; }
        public int VetoCount { get; set; }
        public bool VetoUsed { get; set; }
        public bool CanSkipTurn { get; set; }
        public ulong? SelectedCandidate { get; set; }
    }

    public class EveningDTO
    {
        public ulong DismissedEmployeeId { get; set; }
        public string DismissedEmployeeName { get; set; } = string.Empty;
        public string DismissedRoleKey { get; set; } = string.Empty;
        public string DismissedRoleNameEn { get; set; } = string.Empty;
        public string DismissedRoleNameRu { get; set; } = string.Empty;
        public string DismissedFaction { get; set; } = string.Empty;
        public string DismissedFactionRu { get; set; } = string.Empty;
        public string DismissedFactionColor { get; set; } = string.Empty;
        public bool CanConfirmDismissal { get; set; }
    }

    public class EndGameDTO
    {
        public string? WinnerFaction { get; set; }
        public string WinnerTitleEn { get; set; } = string.Empty;
        public string WinnerTitleRu { get; set; } = string.Empty;
        public string WinnerColor { get; set; } = string.Empty;
        public List<PersonnelRegisterItemDTO> PersonnelRegister { get; set; } = new();
        public List<RoundHistoryDTO> TurnHistory { get; set; } = new();
    }

    public class PersonnelRegisterItemDTO
    {
        public ulong Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsAlive { get; set; }
        public string RoleKey { get; set; } = string.Empty;
        public string RoleNameEn { get; set; } = string.Empty;
        public string RoleNameRu { get; set; } = string.Empty;
        public string Faction { get; set; } = string.Empty;
        public string FactionRu { get; set; } = string.Empty;
        public string FactionColor { get; set; } = string.Empty;
    }

    public class RoundHistoryDTO
    {
        public int Turn { get; set; }
        public ulong? FiredPlayerId { get; set; }
        public string? FiredPlayerName { get; set; }
        public List<string> CandidatesForFiring { get; set; } = new();
        public List<string> OvertimeActionsSummary { get; set; } = new();
    }

    public class NewsItemDTO
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string MessageEn { get; set; } = string.Empty;
        public string MessageRu { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string Category { get; set; } = "GENERAL";
    }

    public class ReportItemDTO
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string MessageEn { get; set; } = string.Empty;
        public string MessageRu { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public bool IsPostponed { get; set; }
    }

    public class ChatScopeInfoDTO
    {
        public string Scope { get; set; } = string.Empty; // PUBLIC, SYNDICATE, SHAREHOLDER, STARTUP, OUTSOURCE, FIRED
        public string LabelEn { get; set; } = string.Empty;
        public string LabelRu { get; set; } = string.Empty;
        public string AudienceBadgeEn { get; set; } = string.Empty;
        public string AudienceBadgeRu { get; set; } = string.Empty;
        public bool CanWrite { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class ParanoiaChatMessageDTO
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public int RoomId { get; set; }
        public ulong SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Scope { get; set; } = "PUBLIC";
        public string Content { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string AudienceLabelEn { get; set; } = string.Empty;
        public string AudienceLabelRu { get; set; } = string.Empty;
    }

    public class GuestLoginDTO
    {
        public string? Username { get; set; }
    }

    public class PlayerActionRequestDTO
    {
        public string ActionType { get; set; } = string.Empty; // "intro_ready", "overtime_action", "overtime_skip", "overtime_ready", "director_start_board", "director_select", "director_fire", "director_veto", "director_skip", "evening_finish"
        public ulong[]? Targets { get; set; }
        public ulong? CandidateId { get; set; }
    }

    public class SendChatMessageRequestDTO
    {
        public string Scope { get; set; } = "PUBLIC";
        public string Message { get; set; } = string.Empty;
    }
}
