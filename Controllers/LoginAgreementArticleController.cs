using System;
using System.Linq;
using System.Threading.Tasks;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SecureTransparentDataExchange.Controllers
{
    [Route("api/login-agreement-articles")]
    [ApiController]
    public class LoginAgreementArticleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoginAgreementArticleController> _logger;

        public LoginAgreementArticleController(ApplicationDbContext context, ILogger<LoginAgreementArticleController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all articles of user agreements.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllArticles()
        {
            var articles = await _context.LoginAgreementArticles
                .Include(a => a.Agreement)
                .ToListAsync();

            return Ok(articles);
        }

        /// <summary>
        /// Get a specific article by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticleById(int id)
        {
            var article = await _context.LoginAgreementArticles
                .Include(a => a.Agreement)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
                return NotFound(new { message = "Article not found" });

            return Ok(article);
        }

        /// <summary>
        /// Add a new article to an agreement.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddArticle([FromBody] LoginAgreementArticle article)
        {
            if (article == null)
                return BadRequest(new { message = "Invalid data" });

            try
            {
                // Check if AgreementId exists
                var agreementExists = await _context.LoginAgreements
                    .AnyAsync(a => a.Id == article.AgreementId);

                if (!agreementExists)
                    return NotFound(new { message = "Agreement not found" });

                // Validate that article title and content are not empty
                if (string.IsNullOrWhiteSpace(article.Title) || string.IsNullOrWhiteSpace(article.Content))
                    return BadRequest(new { message = "Article title and content cannot be empty" });

                // Add the article
                _context.LoginAgreementArticles.Add(article);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetArticleById), new { id = article.Id }, article);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding article.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update an existing article.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] LoginAgreementArticle updatedArticle)
        {
            var article = await _context.LoginAgreementArticles.FindAsync(id);
            if (article == null)
                return NotFound(new { message = "Article not found" });

            // Validate that article title and content are not empty
            if (string.IsNullOrWhiteSpace(updatedArticle.Title) || string.IsNullOrWhiteSpace(updatedArticle.Content))
                return BadRequest(new { message = "Article title and content cannot be empty" });

            article.Title = updatedArticle.Title;
            article.Content = updatedArticle.Content;
            article.CreatedAt = updatedArticle.CreatedAt;

            try
            {
                _context.LoginAgreementArticles.Update(article);
                await _context.SaveChangesAsync();
                return Ok(article);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating article.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a specific article by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.LoginAgreementArticles.FindAsync(id);
            if (article == null)
                return NotFound(new { message = "Article not found" });

            try
            {
                _context.LoginAgreementArticles.Remove(article);
                await _context.SaveChangesAsync();
                return NoContent();  // 204 No Content
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting article.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
