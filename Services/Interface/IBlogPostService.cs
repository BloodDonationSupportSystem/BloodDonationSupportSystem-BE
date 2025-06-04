using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IBlogPostService
    {
        Task<ApiResponse<IEnumerable<BlogPostDto>>> GetAllBlogPostsAsync();
        Task<ApiResponse<BlogPostDto>> GetBlogPostByIdAsync(Guid id);
        Task<ApiResponse<BlogPostDto>> CreateBlogPostAsync(CreateBlogPostDto blogPostDto);
        Task<ApiResponse<BlogPostDto>> UpdateBlogPostAsync(Guid id, UpdateBlogPostDto blogPostDto);
        Task<ApiResponse> DeleteBlogPostAsync(Guid id);
        Task<PagedApiResponse<BlogPostDto>> GetPagedBlogPostsAsync(BlogPostParameters parameters);
    }
}