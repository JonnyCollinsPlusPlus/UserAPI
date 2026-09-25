using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserAPI.DTOs;
using UserAPI.Services;
namespace UserAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService; // Service instance for business logic

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieves all users. Only accessible by Admins.
        /// </summary>
        /// <response code="200">Returns the list of users.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden - user is not an Admin.</response>
        [Authorize(Roles = "Admin")] // Only allows access to users with the "Admin" role
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync(); // Calls service to get all users
            return Ok(users); // Returns 200 OK response with user data
        }

        /// <summary>
        /// Retrieves a single user by ID. Users may only fetch their own record, Admins may fetch any.
        /// </summary>
        /// <response code="200">Returns the requested user.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden - requesting another user's record without Admin role.</response>
        /// <response code="404">User not found.</response>
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var loggedInUserRole = User.FindFirst(ClaimTypes.Role)!.Value;

            if (loggedInUserId != id && loggedInUserRole != "Admin")
                return Forbid();

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

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <response code="201">User created successfully.</response>
        /// <response code="400">Request body failed validation.</response>
        /// <response code="409">Conflict - email or username already exists.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Add(UserRequestDTO userDto)
        {
            try
            {
                var createdUser = await _userService.AddUserAsync(userDto);
                return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing user. Users may only update their own record, Admins may update any.
        /// </summary>
        /// <response code="204">User updated successfully.</response>
        /// <response code="400">Request body failed validation.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden - updating another user's record without Admin role.</response>
        /// <response code="404">User not found.</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, UpdateRoleDTO updateDTO)
        {
            try
            {
                await _userService.UpdateUserAsync(id, updateDTO); // Calls service to update user
                return NoContent(); // Returns 204 No Content response on success
            }
            catch (KeyNotFoundException)
            {
                return NotFound(); // Returns 404 Not Found if user does not exist
            }
        }

        /// <summary>
        /// Deletes a user by ID. Only accessible by Admins.
        /// </summary>
        /// <response code="204">User deleted successfully.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden - user is not an Admin.</response>
        /// <response code="404">User not found.</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Authenticates a user and returns a JWT.
        /// </summary>
        /// <response code="200">Login successful, returns the token.</response>
        /// <response code="400">Request body failed validation (missing/invalid email or password).</response>
        /// <response code="401">Invalid email or password.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            var token = await _userService.LoginAsync(loginDto);
            if (token == null)
                return Unauthorized();

            return Ok(new { token });
        }
    }
}