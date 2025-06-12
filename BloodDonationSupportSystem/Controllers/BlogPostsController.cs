using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize] // M?c ??nh yêu c?u ??ng nh?p cho t?t c? các endpoints
    public class BlogPostsController : BaseApiController
    {
        private readonly IBlogPostService _blogPostService;

        public BlogPostsController(IBlogPostService blogPostService)
        {
            _blogPostService = blogPostService;
        }

        // GET: api/BlogPosts
        [HttpGet]
        [AllowAnonymous] // Cho phép ng??i dùng ch?a ??ng nh?p xem danh sách bài vi?t blog
        [ProducesResponseType(typeof(PagedApiResponse<BlogPostDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetBlogPosts([FromQuery] BlogPostParameters parameters)
        {
            var response = await _blogPostService.GetPagedBlogPostsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/BlogPosts/all
        [HttpGet("all")]
        [AllowAnonymous] // Cho phép ng??i dùng ch?a ??ng nh?p xem t?t c? bài vi?t blog
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BlogPostDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAllBlogPosts()
        {
            var response = await _blogPostService.GetAllBlogPostsAsync();
            return HandleResponse(response);
        }

        // GET: api/BlogPosts/5
        [HttpGet("{id}")]
        [AllowAnonymous] // Cho phép ng??i dùng ch?a ??ng nh?p xem chi ti?t bài vi?t blog
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
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n t?o bài vi?t blog m?i
        [ProducesResponseType(typeof(ApiResponse<BlogPostDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
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
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n c?p nh?t bài vi?t blog
        [ProducesResponseType(typeof(ApiResponse<BlogPostDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
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
        [Authorize(Roles = "Admin")] // Ch? Admin m?i có quy?n xóa bài vi?t blog
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteBlogPost(Guid id)
        {
            var response = await _blogPostService.DeleteBlogPostAsync(id);
            return HandleResponse(response);
        }
    }
}