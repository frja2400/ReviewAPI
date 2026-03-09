using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviewAPI.Data;

namespace ReviewAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/admin/reviews — hämta alla recensioner
        [HttpGet("reviews")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.BookId,
                    r.Text,
                    r.Rating,
                    r.CreatedAt,
                    Username = r.User.Username
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // DELETE api/admin/reviews/{id} — radera valfri recension
        [HttpDelete("reviews/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound("Recensionen hittades inte");
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok("Recensionen raderades");
        }

        // GET api/admin/users — hämta alla användare
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.CreatedAt,
                    ReviewCount = _context.Reviews.Count(r => r.UserId == u.Id)
                })
                .ToListAsync();

            return Ok(users);
        }

        // DELETE api/admin/users/{id} — radera användare
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("Användaren hittades inte");
            }

            // Kontrollera att admin inte raderar sig själv
            var currentUserId = int.Parse(User.FindFirst("id")!.Value);
            if (user.Id == currentUserId)
            {
                return BadRequest("Du kan inte radera ditt eget adminkonto");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("Användaren raderades");
        }
    }
}