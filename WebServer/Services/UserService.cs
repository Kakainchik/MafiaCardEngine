using WebServer.Helpers;
using WebServer.Model.User;

namespace WebServer.Services
{
    public class UserService : IUserService
    {
        private const int PAGE_SIZE = 100;

        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<bool> CreateUser(CreateUserDTO dto)
        {
            bool any = await userRepository.IsUserExist(dto.Username);
            if(any)
            {
                //Such a user exists
                return false;
            }

            UserEntity newEntity = new UserEntity()
            {
                Username = dto.Username,
                Hash = PasswordHasher.CreateHash(dto.Password),
                RefreshTokens = new List<RefreshTokenEntity>()
            };
            return await userRepository.AddUser(newEntity);
        }

        public async Task<UserDTO?> GetUser(long userId)
        {
            UserEntity? entity = await userRepository.GetUser(userId);

            if(entity == null)
            {
                return null;
            }

            return new UserDTO(entity.Id, entity.Username);
        }

        public async Task<IEnumerable<UserDTO>> GetUsers(int page)
        {
            page = Math.Abs(page);
            int offset = PAGE_SIZE * (page - 1);
            Range listRange = offset..(offset + PAGE_SIZE);

            IEnumerable<UserEntity> entities = await userRepository.GetUsers(listRange);

            //Map to UserDTO
            return entities.Select<UserEntity, UserDTO>(e => new UserDTO(e.Id, e.Username));
        }
    }
}