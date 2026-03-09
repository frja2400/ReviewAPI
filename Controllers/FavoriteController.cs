using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviewAPI.Data;
using ReviewAPI.Models;

namespace ReviewAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoritesController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/favorites — hämta alla favoriter för inloggad användare
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.AddedAt)
                .Select(f => new
                {
                    f.Id,
                    f.BookId,
                    f.AddedAt
                })
                .ToListAsync();

            return Ok(favorites);
        }

        // POST api/favorites/{bookId} — lägg till favorit
        [HttpPost("{bookId}")]
        public async Task<IActionResult> AddFavorite(string bookId)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            // Kontrollera att boken inte redan är en favorit
            if (_context.Favorites.Any(f => f.BookId == bookId && f.UserId == userId))
            {
                return BadRequest("Boken finns redan i dina favoriter");
            }

            var favorite = new Favorite
            {
                BookId = bookId,
                UserId = userId
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(favorite);
        }

        // DELETE api/favorites/{bookId} — ta bort favorit
        [HttpDelete("{bookId}")]
        public async Task<IActionResult> DeleteFavorite(string bookId)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.BookId == bookId && f.UserId == userId);

            if (favorite == null)
            {
                return NotFound("Favoriten hittades inte");
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok("Favoriten raderades");
        }
    }
}