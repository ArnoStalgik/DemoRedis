namespace DemoRedis.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public object Data { get; set; }
        public string Error { get; set; }
    }
}
