namespace Cessa.Api.Helpers
{
    using Microsoft.AspNetCore.Http;
    using Cessa.Application.Common.Pagination;
    using System.Collections.Generic;

    public class MetaData
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
        public int FirstItemOnPage { get; set; }
        public int LastItemOnPage { get; set; }
    }

    public class ApiResult<T>
    {
        public int Status { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public object? Errors { get; set; }
        public MetaData? Meta { get; set; }
    }

    public static class ApiResult
    {
        public static ApiResult<object?> Success(string message)
        {
            return new ApiResult<object?>
            {
                Status = StatusCodes.Status200OK,
                Success = true,
                Message = message,
                Data = null
            };
        }

        public static ApiResult<T> Success<T>(T data, string message)
        {
            return new ApiResult<T>
            {
                Status = StatusCodes.Status200OK,
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResult<IEnumerable<T>> Success<T>(PagedResult<T> pagedResult, string message)
        {
            return new ApiResult<IEnumerable<T>>
            {
                Status = StatusCodes.Status200OK,
                Success = true,
                Message = message,
                Data = pagedResult.Items,
                Meta = new MetaData
                {
                    Page = pagedResult.Page,
                    PageSize = pagedResult.PageSize,
                    TotalCount = pagedResult.TotalCount,
                    TotalPages = pagedResult.TotalPages,
                    HasPrevious = pagedResult.HasPreviousPage,
                    HasNext = pagedResult.HasNextPage,
                    FirstItemOnPage = pagedResult.FirstItemOnPage,
                    LastItemOnPage = pagedResult.LastItemOnPage
                }
            };
        }

        public static ApiResult<object> Created(string message)
        {
            return new ApiResult<object>
            {
                Status = StatusCodes.Status201Created,
                Success = true,
                Message = message
            };
        }

        public static ApiResult<T> Created<T>(T data, string message)
        {
            return new ApiResult<T>
            {
                Status = StatusCodes.Status201Created,
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResult<object> BadRequest(string message, object? errors = null)
        {
            return new ApiResult<object>
            {
                Status = StatusCodes.Status400BadRequest,
                Success = false,
                Message = message,
                Errors = errors
            };
        }

        public static ApiResult<object> Unauthorized(string message)
        {
            return new ApiResult<object>
            {
                Status = StatusCodes.Status401Unauthorized,
                Success = false,
                Message = message
            };
        }

        public static ApiResult<object> Forbidden(string message)
        {
            return new ApiResult<object>
            {
                Status = StatusCodes.Status403Forbidden,
                Success = false,
                Message = message
            };
        }

        public static ApiResult<object> NotFound(string message)
        {
            return new ApiResult<object>
            {
                Status = StatusCodes.Status404NotFound,
                Success = false,
                Message = message
            };
        }

        public static ApiResult<object> Conflict(string message, object? errors = null)
        {
            return new ApiResult<object>
            {
                Status = StatusCodes.Status409Conflict,
                Success = false,
                Message = message,
                Errors = errors
            };
        }

        public static ApiResult<object> Error(string message)
        {
            return new ApiResult<object>
            {
                Status = StatusCodes.Status500InternalServerError,
                Success = false,
                Message = message
            };
        }
    }
}
