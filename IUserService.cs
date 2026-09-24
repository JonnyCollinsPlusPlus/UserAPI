using UserAPI.DTOs;
namespace UserAPI
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync();
        Task<UserResponseDTO> GetUserByIdAsync(int id);
        Task<UserResponseDTO> AddUserAsync(UserRequestDTO userDto);
        Task UpdateUserAsync(int id, UserRequestDTO userDto);
        Task DeleteUserAsync(int id);
    }
}
