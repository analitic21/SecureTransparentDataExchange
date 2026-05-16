using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecureTransparentDataExchange.Data;
using SecureTransparentDataExchange.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SecureTransparentDataExchange.Services
{
    public class LoginAgreementArticleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoginAgreementArticleService> _logger;

        public LoginAgreementArticleService(ApplicationDbContext context, ILogger<LoginAgreementArticleService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all agreement articles.
        /// </summary>
        public async Task<List<LoginAgreementArticle>> GetAllArticlesAsync(int agreementId)
        {
            return await _context.LoginAgreementArticles
            .Where(a => a.AgreementId == agreementId)
            .ToListAsync();
        }

        /// <summary>
        /// Add an article to the agreement.
        /// </summary> 
        public async Task<bool> AddArticleAsync(LoginAgreementArticle article)
        {
            try
            {
                if (article == null)
                    return false;

                // Check for agreement existence 
                var agreementExists = await _context.LoginAgreements
                .AnyAsync(a => a.Id == article.AgreementId);

                if (!agreementExists)
                {
                    _logger.LogWarning("Agreement not found with ID: {AgreementId}", article.AgreementId);
                    return false;
                }

                _context.LoginAgreementArticles.Add(article);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding article.");
                return false;
            }
        }

        /// <summary> 
        /// Update the agreement clause. 
        /// </summary> 
        public async Task<bool> UpdateArticleAsync(int id, LoginAgreementArticle updatedArticle)
        {
            try
            {
                var existingArticle = await _context.LoginAgreementArticles.FindAsync(id);

                if (existingArticle == null)
                    return false;

                existingArticle.Title = updatedArticle.Title;
                existingArticle.Content = updatedArticle.Content;
                existingArticle.CreatedAt = updatedArticle.CreatedAt;

                _context.LoginAgreementArticles.Update(existingArticle);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating article.");
                return false;
            }
        }

        /// <summary> 
        /// Delete an agreement clause. 
        /// </summary> 
        public async Task<bool> DeleteArticleAsync(int id)
        {
            try
            {
                var article = await _context.LoginAgreementArticles.FindAsync(id);
                if (article == null)
                    return false;

                _context.LoginAgreementArticles.Remove(article);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting article.");
                return false;
            }
        }
    }
}