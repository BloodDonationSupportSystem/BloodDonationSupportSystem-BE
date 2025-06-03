using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace BloodDonationSupportSystem.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected IActionResult HandleResponse<T>(ApiResponse<T> response)
        {
            return StatusCode((int)response.StatusCode, response);
        }

        protected IActionResult HandleResponse(ApiResponse response)
        {
            return StatusCode((int)response.StatusCode, response);
        }

        protected ApiResponse<T> HandleValidationErrors<T>(ModelStateDictionary modelState)
        {
            var response = new ApiResponse<T>(HttpStatusCode.BadRequest, "Validation errors occurred");
            
            foreach (var error in modelState.Values.SelectMany(v => v.Errors))
            {
                response.AddError(error.ErrorMessage);
            }
            
            return response;
        }

        protected ApiResponse HandleException(Exception ex)
        {
            // Log the exception here
            return new ApiResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred");
        }

        protected ApiResponse<T> HandleException<T>(Exception ex)
        {
            // Log the exception here
            return new ApiResponse<T>(HttpStatusCode.InternalServerError, "An unexpected error occurred");
        }
    }
}