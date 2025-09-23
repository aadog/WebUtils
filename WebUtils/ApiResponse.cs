using Htmx;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebUtils
{
    public static class ApiCodes
    {
        public const string Unknown = "Unknown";
    }

    public class ObjectApiResponse
    {
        public static ApiResponse<object> Create(HttpContext httpContext)
        {
            return new ApiResponse<object>(httpContext) {
            };
        }
    }

    public class ApiResponse<T>(HttpContext _httpContext)
    {
        public string ApiErrorTrigger { get; set; } = "apiError";
        public bool Success { get; set; } = true;  // 默认成功
        public T? Data { get; set; }
        public ApiError? Error { get; set; }

        public JsonResult ToJsonResult(int statusCode = 200)
        {
            if (_httpContext.Request.IsHtmx())
            {
                if (this.Success == false)
                {
                    _httpContext.Response.Htmx(h => h.WithTrigger(ApiErrorTrigger, this));
                }
            }
            return new JsonResult(this) { StatusCode = statusCode };
        }
        public ApiResponse<T> Ok(T data)
        {
            Data = data;
            Success = true;
            Error = null;
            return this;
        }

        public ApiResponse<T> Fail(string code, string message, ModelStateDictionary modelState)
        {
            Success = false;
            Error = new ApiError
            {
                Code = code,
                Message = message,
                FieldErrors = modelState
                    .Where(kvp => kvp.Value?.Errors?.Count > 0)
                    .SelectMany(kvp => kvp.Value!.Errors.Select(err => new FieldError
                    {
                        Field = kvp.Key,
                        Message = err.ErrorMessage
                    }))
                    .ToList()
            };
            return this;
        }
        public ApiResponse<T> Fail(string code, string message, List<FieldError>? fieldErrors = null)
        {
            Success = false;
            Error = new ApiError
            {
                Code = code,
                Message = message,
                FieldErrors = fieldErrors
            };
            return this;
        }
    }

    public class ApiError
    {
        public string Code { get; set; } = "UNKNOWN_ERROR";  // 错误码
        public string? Message { get; set; } = "发生错误";     // 错误提示信息
        public string? Details { get; set; }                // 详细错误，字符串形式
        // 如果需要字段级错误可用 List<FieldError> 替代 Details
        public List<FieldError>? FieldErrors { get; set; } // ← 这里加
    }

    public class FieldError
    {
        public string Field { get; set; } = string.Empty;  // 发生错误的字段名
        public string Message { get; set; } = string.Empty; // 该字段的错误消息
    }
}
