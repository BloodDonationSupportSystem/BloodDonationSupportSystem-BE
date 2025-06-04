using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class BlogPostService : IBlogPostService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BlogPostService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<IEnumerable<BlogPostDto>>> GetAllBlogPostsAsync()
        {
            try
            {
                var blogPosts = await _unitOfWork.BlogPosts.GetAllAsync();
                var blogPostDtos = blogPosts.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<BlogPostDto>>(blogPostDtos)
                {
                    Message = $"Retrieved {blogPostDtos.Count} blog posts successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<BlogPostDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BlogPostDto>> GetBlogPostByIdAsync(Guid id)
        {
            try
            {
                var blogPost = await _unitOfWork.BlogPosts.GetByIdWithAuthorAsync(id);
                
                if (blogPost == null)
                    return new ApiResponse<BlogPostDto>(HttpStatusCode.NotFound, $"Blog post with ID {id} not found");

                return new ApiResponse<BlogPostDto>(MapToDto(blogPost));
            }
            catch (Exception ex)
            {
                return new ApiResponse<BlogPostDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BlogPostDto>> CreateBlogPostAsync(CreateBlogPostDto blogPostDto)
        {
            try
            {
                // Verify that the author exists
                var author = await _unitOfWork.Users.GetByIdAsync(blogPostDto.AuthorId);
                if (author == null)
                {
                    return new ApiResponse<BlogPostDto>(HttpStatusCode.BadRequest, $"User with ID {blogPostDto.AuthorId} not found");
                }

                var blogPost = new BlogPost
                {
                    Title = blogPostDto.Title,
                    Body = blogPostDto.Body,
                    IsPublished = blogPostDto.IsPublished,
                    AuthorId = blogPostDto.AuthorId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.BlogPosts.AddAsync(blogPost);
                await _unitOfWork.CompleteAsync();

                // Fetch the blog post with author details
                var createdBlogPost = await _unitOfWork.BlogPosts.GetByIdWithAuthorAsync(blogPost.Id);

                return new ApiResponse<BlogPostDto>(MapToDto(createdBlogPost), "Blog post created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<BlogPostDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BlogPostDto>> UpdateBlogPostAsync(Guid id, UpdateBlogPostDto blogPostDto)
        {
            try
            {
                var blogPost = await _unitOfWork.BlogPosts.GetByIdAsync(id);
                
                if (blogPost == null)
                    return new ApiResponse<BlogPostDto>(HttpStatusCode.NotFound, $"Blog post with ID {id} not found");

                blogPost.Title = blogPostDto.Title;
                blogPost.Body = blogPostDto.Body;
                blogPost.IsPublished = blogPostDto.IsPublished;
                blogPost.LastUpdatedTime = DateTimeOffset.Now;

                _unitOfWork.BlogPosts.Update(blogPost);
                await _unitOfWork.CompleteAsync();

                // Fetch the updated blog post with author details
                var updatedBlogPost = await _unitOfWork.BlogPosts.GetByIdWithAuthorAsync(id);

                return new ApiResponse<BlogPostDto>(MapToDto(updatedBlogPost), "Blog post updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<BlogPostDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteBlogPostAsync(Guid id)
        {
            try
            {
                var blogPost = await _unitOfWork.BlogPosts.GetByIdAsync(id);
                
                if (blogPost == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Blog post with ID {id} not found");

                // Soft delete - update DeletedTime
                blogPost.DeletedTime = DateTimeOffset.Now;
                _unitOfWork.BlogPosts.Update(blogPost);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<PagedApiResponse<BlogPostDto>> GetPagedBlogPostsAsync(BlogPostParameters parameters)
        {
            try
            {
                // Build the filter expression based on the parameters
                Expression<Func<BlogPost, bool>> filter = bp => bp.DeletedTime == null;

                // Apply AuthorId filter if specified
                if (parameters.AuthorId.HasValue)
                {
                    filter = filter.And(bp => bp.AuthorId == parameters.AuthorId.Value);
                }

                // Apply IsPublished filter if specified
                if (parameters.IsPublished.HasValue)
                {
                    filter = filter.And(bp => bp.IsPublished == parameters.IsPublished.Value);
                }

                // Apply date range filters if specified
                if (parameters.CreatedDateFrom.HasValue)
                {
                    filter = filter.And(bp => bp.CreatedTime >= parameters.CreatedDateFrom.Value);
                }

                if (parameters.CreatedDateTo.HasValue)
                {
                    filter = filter.And(bp => bp.CreatedTime <= parameters.CreatedDateTo.Value);
                }

                if (parameters.UpdatedDateFrom.HasValue)
                {
                    filter = filter.And(bp => bp.LastUpdatedTime >= parameters.UpdatedDateFrom.Value);
                }

                if (parameters.UpdatedDateTo.HasValue)
                {
                    filter = filter.And(bp => bp.LastUpdatedTime <= parameters.UpdatedDateTo.Value);
                }

                // Apply search term if specified
                if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                {
                    filter = filter.And(bp => bp.Title.Contains(parameters.SearchTerm) || 
                                            bp.Body.Contains(parameters.SearchTerm));
                }

                var (blogPosts, totalCount) = await _unitOfWork.BlogPosts.GetPagedAsync(
                    parameters,
                    filter: filter,
                    orderBy: query => parameters.SortBy?.ToLower() switch
                    {
                        "title" => parameters.SortAscending ? query.OrderBy(bp => bp.Title) : query.OrderByDescending(bp => bp.Title),
                        "created" => parameters.SortAscending ? query.OrderBy(bp => bp.CreatedTime) : query.OrderByDescending(bp => bp.CreatedTime),
                        "updated" => parameters.SortAscending ? query.OrderBy(bp => bp.LastUpdatedTime) : query.OrderByDescending(bp => bp.LastUpdatedTime),
                        _ => parameters.SortAscending ? query.OrderBy(bp => bp.CreatedTime) : query.OrderByDescending(bp => bp.CreatedTime)
                    },
                    includeProperties: "User"
                );

                var blogPostDtos = blogPosts.Select(MapToDto).ToList();

                return new PagedApiResponse<BlogPostDto>(
                    blogPostDtos,
                    totalCount,
                    parameters.PageNumber,
                    parameters.PageSize
                );
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<BlogPostDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        private BlogPostDto MapToDto(BlogPost blogPost)
        {
            return new BlogPostDto
            {
                Id = blogPost.Id,
                Title = blogPost.Title,
                Body = blogPost.Body,
                IsPublished = blogPost.IsPublished,
                AuthorId = blogPost.AuthorId,
                AuthorName = blogPost.User != null ? $"{blogPost.User.FirstName} {blogPost.User.LastName}" : string.Empty,
                CreatedTime = blogPost.CreatedTime,
                LastUpdatedTime = blogPost.LastUpdatedTime
            };
        }
    }

    // Extension method to combine expressions with AND operator
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));
            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);
            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
        }

        private class ReplaceExpressionVisitor : System.Linq.Expressions.ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }
    }
}