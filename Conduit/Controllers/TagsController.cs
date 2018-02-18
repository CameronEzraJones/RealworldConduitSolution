using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Conduit.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Controllers
{
    [Produces("application/json")]
    public class TagsController : Controller
    {
        ConduitDbContext _context;

        public TagsController(ConduitDbContext context)
        {
            _context = context;
        }
        //Get a list of tags
        //GET /api/tags
        [HttpGet("/api/tags")]
        public async Task<IActionResult> getTags()
        {
            var tags = _context.Tags.Select(e => e.TagName).ToList();
            TagsResponse tagsResponse = new TagsResponse();
            tagsResponse.Tags = tags;
            return Ok(tagsResponse);
        }
    }

    public class TagsResponse
    {
        public List<string> Tags { get; set; }
    }
}