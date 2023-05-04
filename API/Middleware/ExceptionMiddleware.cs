using System.Net;
using System.Text.Json;
using API.Error;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly  ILogger<ExceptionMiddleware> _logger;

        private readonly IHostEnvironment _env;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _env = env;
            _logger = logger;
            _next = next; 
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            // return our own exception
            catch(Exception ex)
            {
                _logger.LogError(ex,ex.Message);
                context.Response.ContentType = "application/json"; //return to the client, we are not inside controller,we need to do it ourselves
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment()
                    ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString()) // in developmentMode
                    : new ApiException(context.Response.StatusCode, ex.Message, "Internal Server Error");

                var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase}; //default in controller, also need to be created by ourselves
                var json = JsonSerializer.Serialize(response,options);
                await context.Response.WriteAsync(json);
            }
        }
    }
}