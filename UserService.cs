using UserAPI.DTOs;
namespace UserAPI
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository; // Repository instance for database operations

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository; // Injecting the repository via constructor
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
                CreatedAt = u.CreatedAt
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
                CreatedAt = user.CreatedAt
            };
        }

        // Adds a new user using a request DTO
        public async Task AddUserAsync(UserRequestDTO userDto)
        {
            // Convert DTO to entity
            var user = new User
            {
                Email = userDto.Email,
                Username = userDto.Username,
                Role = userDto.Role
            };

            // Add the new user to the database
            await _userRepository.AddAsync(user);
        }

        // Updates an existing user with new data
        public async Task UpdateUserAsync(int id, UserRequestDTO userDto)
        {
            var user = await _userRepository.GetByIdAsync(id); // Fetch the user by ID

            // If the user does not exist, throw an exception
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Update user fields with new values from DTO
            user.Email = userDto.Email;
            user.Username = userDto.Username;
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
