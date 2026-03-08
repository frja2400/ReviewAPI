using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviewAPI.Data;
using ReviewAPI.Models;
using System.Security.Claims;

namespace ReviewAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/reviews/{bookId} — hämta alla recensioner för en bok
        [HttpGet("{bookId}")]
        public async Task<IActionResult> GetReviewsByBook(string bookId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.BookId,
                    r.Text,
                    r.Rating,
                    r.CreatedAt,
                    r.UserId,
                    Username = r.User.Username
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // GET api/reviews/user — hämta inloggad användares recensioner
        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserReviews()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var reviews = await _context.Reviews
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.BookId,
                    r.Text,
                    r.Rating,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // GET api/reviews/top-rated — hämta 4 högst betygsatta böcker
        [HttpGet("top-rated")]
        public async Task<IActionResult> GetTopRated()
        {
            var topRated = await _context.Reviews
                .GroupBy(r => r.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    AverageRating = g.Average(r => r.Rating),
                    ReviewCount = g.Count()
                })
                .OrderByDescending(g => g.AverageRating)
                .Take(4)
                .ToListAsync();

            return Ok(topRated);
        }

        // GET api/reviews/latest — hämta 4 senast recenserade böcker
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var latest = await _context.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var uniqueBooks = latest
                .GroupBy(r => r.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    CreatedAt = g.First().CreatedAt
                })
                .Take(4)
                .ToList();

            return Ok(uniqueBooks);
        }

        // POST api/reviews — skapa recension (kräver inloggning)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] ReviewRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Kontrollera att användaren inte redan recenserat denna bok
            if (_context.Reviews.Any(r => r.BookId == request.BookId && r.UserId == userId))
            {
                return BadRequest("Du har redan recenserat denna bok");
            }

            var review = new Review
            {
                BookId = request.BookId,
                UserId = userId,
                Text = request.Text,
                Rating = request.Rating
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(review);
        }

        // PUT api/reviews/{id} — redigera recension (kräver ägande)
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound("Recensionen hittades inte");
            }

            // Kontrollera att användaren äger recensionen
            if (review.UserId != userId)
            {
                return Forbid();
            }

            review.Text = request.Text;
            review.Rating = request.Rating;

            await _context.SaveChangesAsync();

            return Ok(review);
        }

        // DELETE api/reviews/{id} — radera recension (kräver ägande eller admin)
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound("Recensionen hittades inte");
            }

            // Kontrollera att användaren äger recensionen eller är admin
            if (review.UserId != userId && userRole != "admin")
            {
                return Forbid();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok("Recensionen raderades");
        }
    }

    // Klass för att ta emot recensionsdata från frontend
    public class ReviewRequest
    {
        public string BookId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int Rating { get; set; }
    }
}