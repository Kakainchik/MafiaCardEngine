using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebServer.Model.Room;
using WebServer.Model.User;
using WebServer.Services;

namespace WebServer.Controllers.Hall
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HallController : ControllerBase
    {
        private readonly IHallService hallService;

        public HallController(IHallService hallService)
        {
            this.hallService = hallService;
        }

        /// <summary>
        /// Get a list of active open lobbies by page.
        /// <code>GET: api/hall/{page?}</code>
        /// Parameters:
        /// <list type="bullet">
        /// <paramref name="onlyValid"/>: whether unclude only valid lobbies or all
        /// </list>
        /// </summary>
        [HttpGet("{page:int?}/{onlyValid:bool?}")]
        public IActionResult Index([FromRoute] int page = 1, [FromQuery] bool onlyValid = true)
        {
            if(page < 1)
            {
                return NotFound();
            }

            IEnumerable<WaitRoomDTO> response = hallService.GetRooms(page, onlyValid);

            return Ok(response);
        }

        /// <summary>
        /// Request to crate a new lobby.
        /// <code>POST: api/hall/new_lobby</code>
        /// </summary>
        [HttpPost("new_lobby")]
        public IActionResult CreateNewLobby([FromBody] CreateRoomDTO request)
        {
            long hostId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string username = User.FindFirstValue(ClaimTypes.Name)!;
            UserDTO host = new UserDTO(hostId, username);

            WaitRoomDTO response = hallService.CreateRoom(request, host);

            string newAddress = Request.Path + $"/{response.Id}";
            return Created(newAddress, response);
        }

        /// <summary>
        /// Get an open lobby by its Id.
        /// <code>GET: api/hall/lobby?{id}</code>
        /// </summary>
        [HttpGet("lobby")]
        public IActionResult GetLobbyById([FromQuery][Required] int id)
        {
            WaitRoomDTO? lobby = hallService.GetRoom(id);

            if(lobby != null)
            {
                return Ok(lobby);
            }
            else
            {
                return NotFound();
            }
        }
    }
}