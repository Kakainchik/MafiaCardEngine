using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebServer.Model.ParanoiaCorp;
using WebServer.Services;
using ParanoiaRoleSignature = WebServer.Shared.ParanoiaCorp.GameObjects.RoleSignature;

namespace WebServer.Controllers.ParanoiaCorp
{
    [Authorize]
    [Route("api/paranoia")]
    [ApiController]
    public class ParanoiaController : ControllerBase
    {
        private readonly IParanoiaLobbyService lobbyService;
        private readonly IParanoiaGameRepository gameRepository;

        public ParanoiaController(
            IParanoiaLobbyService lobbyService,
            IParanoiaGameRepository gameRepository)
        {
            this.lobbyService = lobbyService;
            this.gameRepository = gameRepository;
        }

        [AllowAnonymous]
        [HttpGet("roles")]
        public IActionResult GetRoleDirectory()
        {
            var roles = ParanoiaRoleCatalogue.GetAllRoles();
            return Ok(roles);
        }

        [HttpGet("room/{roomId:int}")]
        public IActionResult GetRoomLobby([FromRoute] int roomId)
        {
            try
            {
                var room = lobbyService.GetLobby(roomId);
                return Ok(room);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Room {roomId} not found." });
            }
        }

        public class UpdateRolePoolRequest
        {
            public string Role { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        [HttpPost("room/{roomId:int}/role")]
        public async Task<IActionResult> UpdateRolePool([FromRoute] int roomId, [FromBody] UpdateRolePoolRequest req)
        {
            if (!Enum.TryParse<ParanoiaRoleSignature>(req.Role, true, out var roleSig))
            {
                return BadRequest(new { error = $"Invalid role signature: {req.Role}" });
            }

            await lobbyService.SetRoleAsync(roomId, GetCurrentUserId(), roleSig, req.Count);
            return Ok(lobbyService.GetLobby(roomId));
        }

        [HttpPost("room/{roomId:int}/clear_roles")]
        public async Task<IActionResult> ClearRolePool([FromRoute] int roomId)
        {
            await lobbyService.ClearRolesAsync(roomId, GetCurrentUserId());
            return Ok(lobbyService.GetLobby(roomId));
        }

        [HttpPost("room/{roomId:int}/end")]
        public IActionResult EndSession([FromRoute] int roomId)
        {
            lobbyService.EndSession(roomId, GetCurrentUserId());
            return Ok(new { success = true });
        }

        [HttpPost("room/{roomId:int}/start")]
        public async Task<IActionResult> StartGame([FromRoute] int roomId)
        {
            ulong userId = GetCurrentUserId();
            try
            {
                var session = await lobbyService.StartGameAsync(roomId, userId);
                return Ok(new { success = true, roomId = session.RoomId });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("room/{roomId:int}/state")]
        public IActionResult GetPlayerState([FromRoute] int roomId)
        {
            ulong userId = GetCurrentUserId();
            var session = gameRepository.GetGame(roomId);
            if (session == null)
            {
                return NotFound(new { error = $"Active game session not found for room {roomId}." });
            }

            try
            {
                var state = session.GetPlayerView(userId);
                return Ok(state);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("room/{roomId:int}/action")]
        public async Task<IActionResult> SubmitAction([FromRoute] int roomId, [FromBody] PlayerActionRequestDTO req)
        {
            ulong userId = GetCurrentUserId();
            var session = gameRepository.GetGame(roomId);
            if (session == null)
            {
                return NotFound(new { error = $"Active game session not found for room {roomId}." });
            }

            switch (req.ActionType.ToLowerInvariant())
            {
                case "intro_ready":
                    await session.HandleIntroReadyAsync(userId);
                    break;
                case "overtime_action":
                    await session.HandleOvertimeActionAsync(userId, req.Targets ?? Array.Empty<ulong>(), isSkip: false);
                    break;
                case "overtime_skip":
                    await session.HandleOvertimeActionAsync(userId, Array.Empty<ulong>(), isSkip: true);
                    break;
                case "overtime_ready":
                    await session.HandleOvertimeReadyAsync(userId);
                    break;
                case "director_start_board":
                    await session.HandleDirectorStartBoardAsync(userId);
                    break;
                case "director_select":
                    if (!req.CandidateId.HasValue) return BadRequest(new { error = "CandidateId is required." });
                    await session.HandleDirectorSelectCandidateAsync(userId, req.CandidateId.Value);
                    break;
                case "director_fire":
                    await session.HandleDirectorFireAsync(userId, req.CandidateId);
                    break;
                case "director_veto":
                    await session.HandleDirectorUseVetoAsync(userId);
                    break;
                case "director_skip":
                    await session.HandleDirectorSkipAsync(userId);
                    break;
                case "evening_finish":
                    await session.HandleEveningFinishAsync(userId);
                    break;
                default:
                    return BadRequest(new { error = $"Unknown action type: {req.ActionType}" });
            }

            var updatedState = session.GetPlayerView(userId);
            return Ok(updatedState);
        }

        [HttpGet("room/{roomId:int}/chat")]
        public IActionResult GetChat([FromRoute] int roomId, [FromQuery] string scope = "PUBLIC")
        {
            ulong userId = GetCurrentUserId();
            var session = gameRepository.GetGame(roomId);
            if (session == null)
            {
                return NotFound(new { error = $"Active game session not found for room {roomId}." });
            }

            var messages = session.GetChatHistory(userId, scope);
            return Ok(messages);
        }

        [HttpPost("room/{roomId:int}/chat")]
        public async Task<IActionResult> SendChat([FromRoute] int roomId, [FromBody] SendChatMessageRequestDTO req)
        {
            ulong userId = GetCurrentUserId();
            var session = gameRepository.GetGame(roomId);
            if (session == null)
            {
                return NotFound(new { error = $"Active game session not found for room {roomId}." });
            }

            bool sent = await session.SendChatMessageAsync(userId, req.Scope, req.Message);
            if (!sent)
            {
                return Forbid();
            }

            return Ok(new { success = true });
        }

        private ulong GetCurrentUserId()
        {
            string? idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ulong.TryParse(idClaim, out ulong id))
            {
                return id;
            }
            throw new UnauthorizedAccessException("Valid user ID claim not found.");
        }
    }
}
