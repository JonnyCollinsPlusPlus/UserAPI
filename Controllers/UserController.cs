using Microsoft.AspNetCore.Mvc;
using UserAPI.DTOs;
namespace UserAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService; // Service instance for business logic

        public UserController(IUserService userService)
        {
            _userService = userService; // Injecting the service via constructor
        }

        // Handles HTTP GET request to fetch all users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync(); // Calls service to get all users
            return Ok(users); // Returns 200 OK response with user data
        }

        // Handles HTTP GET request to fetch a single user by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id); // Calls service to fetch user by ID
                return Ok(user); // Returns 200 OK response if found
            }
            catch (KeyNotFoundException)
            {
                return NotFound(); // Returns 404 Not Found if user does not exist
            }
        }

        // Handles HTTP POST request to add a new user
        [HttpPost]
        public async Task<IActionResult> Add(UserRequestDTO userDto)
        {
            var createdUser = await _userService.AddUserAsync(userDto); // Calls service to add a new user
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
            // Returns 201 Created response with location header pointing to the new user
        }

        // Handles HTTP PUT request to update an existing user
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserRequestDTO userDto)
        {
            try
            {
                await _userService.UpdateUserAsync(id, userDto); // Calls service to update user
                return NoContent(); // Returns 204 No Content response on success
            }
            catch (KeyNotFoundException)
            {
                return NotFound(); // Returns 404 Not Found if user does not exist
            }
        }

        // Handles HTTP DELETE request to delete a user by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id); // Calls service to delete user
                return NoContent(); // Returns 204 No Content response on success
            }
            catch (KeyNotFoundException)
            {
                return NotFound(); // Returns 404 Not Found if user does not exist
            }
        }
    }
}