namespace folder_browser.Models
{
    public class FileSystemItem
    {
        public string? Name {get;set;}
        public bool IsFolder {get;set;}
        public string? Url { get; set; }
        public string? SizeStr { get; set; }
        public string? Type { get; set; }
        public DateTime? LastModifyDate { get; set; }
        public string? IconImageFileName { get; set; }
    }
}