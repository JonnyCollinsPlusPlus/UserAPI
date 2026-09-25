using UserAPI.DTOs;
namespace UserAPI
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync();
        Task<UserResponseDTO> GetUserByIdAsync(int id);
        Task<UserResponseDTO> AddUserAsync(UserRequestDTO userDto);
        Task<string?> LoginAsync(LoginDTO loginDto);
        Task UpdateUserAsync(int id, UpdateRoleDTO updateDTO);
        Task DeleteUserAsync(int id);
    }
}
