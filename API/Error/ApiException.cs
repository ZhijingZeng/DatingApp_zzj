namespace API.Error
{
    public class ApiException
    {
        public ApiException(int statusCode, string message, string details)
        {
            StatusCode = statusCode;
            Details = details;
            Message = message;
        }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}