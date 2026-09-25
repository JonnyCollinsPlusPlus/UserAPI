using static UserAPI.UserRepository;
using Microsoft.EntityFrameworkCore;
namespace UserAPI
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context; // Database context for interacting with the database

        public UserRepository(ApplicationDbContext context)
        {
            _context = context; // Injecting database context via constructor
        }

        // Retrieves all users from the database
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            // Converts the Users table into a list and returns it asynchronously
            return await _context.Users.ToListAsync();
        }

        // Retrieves a user by their ID
        public async Task<User> GetByIdAsync(int id)
        {
            // Uses FindAsync to search for a user by their primary key (ID)
            return await _context.Users.FindAsync(id);
        }
 

        // Retrieves a user by their email
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }


        // Retrieves a user by their username
        public async Task<User?> GetByNameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        // Adds a new user to the database
        public async Task AddAsync(User user)
        {
            if (await GetByEmailAsync(user.Email) != null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }
            if (await GetByNameAsync(user.Username) != null)
            {
                throw new InvalidOperationException("A user with this username already exists.");
            }
            // Adds the user entity to the database context
            await _context.Users.AddAsync(user);

            // Saves the changes to the database asynchronously
            await _context.SaveChangesAsync();
        }

        // Updates an existing user in the database
        public async Task UpdateAsync(User user)
        {
            // Marks the user entity as updated in the database context
            _context.Users.Update(user);

            // Saves the updated user data to the database asynchronously
            await _context.SaveChangesAsync();
        }

        // Deletes a user by their ID
        public async Task DeleteAsync(int id)
        {
            // Finds the user in the database using the provided ID
            var user = await _context.Users.FindAsync(id);

            // If the user exists, remove it from the database
            if (user != null)
            {
                _context.Users.Remove(user);

                // Saves the changes to the database asynchronously
                await _context.SaveChangesAsync();
            }
        }
    }
}
