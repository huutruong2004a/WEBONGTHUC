namespace WEB_CONG_THUC.Models
{
    public class YouTubeVideoData
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Transcript { get; set; }
        public string? ErrorMessage { get; set; }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    }
}
