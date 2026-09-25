using UserAPI.DTOs;
using UserAPI.Services;
namespace UserAPI
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository; // Repository instance for database operations
        private readonly ITokenService _tokenService;

        public UserService(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        // Retrieves all users, converts them to DTOs, and returns the list
        public async Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync(); // Fetch all users from repository

            // Convert each user entity into a UserResponseDTO and return the list
            return users.Select(u => new UserResponseDTO
            {
                Email = u.Email,
                Username = u.Username,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                Id = u.Id
            });
        }

        // Retrieves a user by ID and converts it to a DTO
        public async Task<UserResponseDTO> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id); // Fetch user by ID

            // If the user is not found, throw an exception
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Convert entity to DTO and return it
            return new UserResponseDTO
            {
                Email = user.Email,
                Username = user.Username,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                Id = user.Id
            };
        }


        public async Task<string?> LoginAsync(LoginDTO loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return null;

            return _tokenService.GenerateToken(user.Id, user.Role);
        }

        // Adds a new user using a request DTO
        public async Task<UserResponseDTO> AddUserAsync(UserRequestDTO userDto)
        {
            // Convert DTO to entity
            var user = new User
            {
                Email = userDto.Email,
                Username = userDto.Username,
                Role = "User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password)
            };

            // Add the new user to the database 
            await _userRepository.AddAsync(user);

            // Return the created user as a response DTO
            return new UserResponseDTO
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role
            };
        }

        // Updates an existing user with new data
        public async Task UpdateUserAsync(int id, UpdateRoleDTO userDto)
        {
            var user = await _userRepository.GetByIdAsync(id); // Fetch the user by ID

            // If the user does not exist, throw an exception
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Update user fields with new values from DTO
            user.Role = userDto.Role;

            // Save the updated user in the database
            await _userRepository.UpdateAsync(user);
        }

        // Deletes a user by ID
        public async Task DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id); // Fetch the user by ID

            // If the user does not exist, throw an exception
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Delete the user from the database
            await _userRepository.DeleteAsync(id);
        }
    }
}
