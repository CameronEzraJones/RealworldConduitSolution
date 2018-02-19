using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Conduit.Contexts;
using Conduit.Helpers;
using Conduit.Models;
using Conduit.Models.HTTPTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Controllers
{
    [Produces("application/json")]
    public class ArticlesController : Controller
    {
        UserManager<ApplicationUser> _userManager;
        ConduitDbContext _context;

        public ArticlesController(UserManager<ApplicationUser> userManager,
            ConduitDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        //Create a new article
        //POST /api/articles
        [HttpPost("/api/articles")]
        public async Task<IActionResult> createArticle([FromBody] ArticleHTTPTransferObject articleHTTPTransferObject)
        {
            var authedUser = HttpContext.User;
            if (!authedUser.HasClaim(c =>
             c.Type == ClaimTypes.NameIdentifier))
            {
                this.HttpContext.Response.StatusCode = 401;
                var authErrorResponse = new ErrorResponse();
                authErrorResponse.addErrorKey("Missing authentication");
                return Json(authErrorResponse);
            }
            string authUsername = authedUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser authedUserInDB = await _userManager.FindByNameAsync(authUsername);
            if (null == authedUserInDB)
            {
                this.HttpContext.Response.StatusCode = 404;
                var authErrorResponse = new ErrorResponse();
                authErrorResponse.addErrorKey("Authenticated user not in database");
                return Json(authErrorResponse);
            }
            Article article = articleHTTPTransferObject.Article;
            if (null == article.Title || null == article.Description || null == article.Body)
            {
                this.HttpContext.Response.StatusCode = 422;
                var authErrorResponse = new ErrorResponse();
                authErrorResponse.addErrorKey("Article requires a title, description, and body");
                return Json(authErrorResponse);
            }
            try
            {
                article.Slug = SlugUtil.GenerateSlug(article.Title);
                article.AuthorId = authedUserInDB.Id;
                article.CreatedAt = DateTime.Now;
                article.UpdatedAt = DateTime.Now;
                _context.Articles.Add(article);
                _context.SaveChanges();
                foreach (string tag in article.TagList)
                {
                    Tag tagEntity;
                    if (!_context.Tags.Any(e => e.TagName == tag))
                    {
                        tagEntity = new Tag();
                        tagEntity.TagName = tag;
                        _context.Tags.Add(tagEntity);
                    }
                    else
                    {
                        tagEntity = _context.Tags.FirstOrDefault(e => e.TagName == tag);
                    }
                    ArticleTags articleTags = new ArticleTags();
                    articleTags.ArticleId = article.Id;
                    articleTags.TagId = tagEntity.TagId;
                    _context.ArticleTags.Add(articleTags);
                    _context.SaveChanges();
                }
            } catch (DbUpdateException ex)
            {
                this.HttpContext.Response.StatusCode = 422;
                var authErrorResponse = new ErrorResponse();
                authErrorResponse.addErrorKey(ex.Message);
                return Json(authErrorResponse);
            }
            this.HttpContext.Response.StatusCode = 201;
            articleHTTPTransferObject = new ArticleHTTPTransferObject();
            articleHTTPTransferObject.Article = article;
            return Json(article);
        }
    }
}