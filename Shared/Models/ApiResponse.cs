using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace Shared.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public ApiResponse()
        {
            Success = true;
            StatusCode = HttpStatusCode.OK;
            Message = "Request completed successfully";
        }

        public ApiResponse(string message)
        {
            Success = true;
            Message = message;
            StatusCode = HttpStatusCode.OK;
        }

        public ApiResponse(HttpStatusCode statusCode, string message = null, bool success = false)
        {
            Success = success;
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }

        private static string GetDefaultMessageForStatusCode(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.OK => "Request completed successfully",
                HttpStatusCode.Created => "Resource created successfully",
                HttpStatusCode.NoContent => "Resource deleted successfully",
                HttpStatusCode.BadRequest => "Invalid request",
                HttpStatusCode.Unauthorized => "Unauthorized access",
                HttpStatusCode.Forbidden => "Access denied",
                HttpStatusCode.NotFound => "Resource not found",
                HttpStatusCode.Conflict => "Request conflict with current state",
                HttpStatusCode.InternalServerError => "An internal server error occurred",
                _ => "An error occurred"
            };
        }

        public void AddError(string error)
        {
            Success = false;
            Errors.Add(error);
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }
        
        public int Count 
        { 
            get
            {
                if (Data == null)
                    return 0;
                
                if (Data is IEnumerable<object> collection)
                    return collection.Count();
                
                return 1;
            }
        }

        public ApiResponse() : base() { }

        public ApiResponse(T data) : base()
        {
            Data = data;
        }

        public ApiResponse(T data, string message) : base(message)
        {
            Data = data;
        }

        public ApiResponse(HttpStatusCode statusCode, string message = null, bool success = false) 
            : base(statusCode, message, success) { }
    }

    public class PagedApiResponse<T> : ApiResponse<IEnumerable<T>>
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        public PagedApiResponse() : base() { }

        public PagedApiResponse(IEnumerable<T> data, int totalCount, int pageNumber, int pageSize) : base(data)
        {
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            HasPreviousPage = PageNumber > 1;
            HasNextPage = PageNumber < TotalPages;
        }
    }
}