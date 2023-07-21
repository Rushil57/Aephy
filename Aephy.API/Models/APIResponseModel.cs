namespace Aephy.API.Models
{
    public class APIResponseModel
    {
        public object StatusCode { get; set; }
        public string Message { get; set; }
        public object Result { get; set; } = "";

        public object IndustryResult { get; set; } = "";

        public object ServiceResult { get; set; } = "";
    }
}
