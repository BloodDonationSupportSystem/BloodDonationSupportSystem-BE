using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodDonationSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostsController : BaseApiController
    {
        private readonly IBlogPostService _blogPostService;

        public BlogPostsController(IBlogPostService blogPostService)
        {
            _blogPostService = blogPostService;
        }

        // GET: api/BlogPosts
        [HttpGet]
        [ProducesResponseType(typeof(PagedApiResponse<BlogPostDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetBlogPosts([FromQuery] BlogPostParameters parameters)
        {
            var response = await _blogPostService.GetPagedBlogPostsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/BlogPosts/all
        [HttpGet("all")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BlogPostDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAllBlogPosts()
        {
            var response = await _blogPostService.GetAllBlogPostsAsync();
            return HandleResponse(response);
        }

        // GET: api/BlogPosts/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<BlogPostDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetBlogPost(Guid id)
        {
            var response = await _blogPostService.GetBlogPostByIdAsync(id);
            return HandleResponse(response);
        }

        // POST: api/BlogPosts
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BlogPostDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostBlogPost([FromBody] CreateBlogPostDto blogPostDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<BlogPostDto>(ModelState));
            }

            var response = await _blogPostService.CreateBlogPostAsync(blogPostDto);
            return HandleResponse(response);
        }

        // PUT: api/BlogPosts/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<BlogPostDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutBlogPost(Guid id, [FromBody] UpdateBlogPostDto blogPostDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<BlogPostDto>(ModelState));
            }

            var response = await _blogPostService.UpdateBlogPostAsync(id, blogPostDto);
            return HandleResponse(response);
        }

        // DELETE: api/BlogPosts/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteBlogPost(Guid id)
        {
            var response = await _blogPostService.DeleteBlogPostAsync(id);
            return HandleResponse(response);
        }
    }
}