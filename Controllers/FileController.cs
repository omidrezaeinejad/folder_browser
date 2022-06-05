using folder_browser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

[ApiController]
[Route("")]
public class FileController : Controller
{
    private static SortedDictionary<string, string> _fileIcons;
    private Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment;
    private static object _iconsLoadLock = new object();
    public FileController(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
    {
        hostingEnvironment = env;
        LoadIcons();
    }

    private void LoadIcons()
    {
        if (_fileIcons == null)
        {
            lock (_iconsLoadLock)
            {
                if (_fileIcons == null)
                {
                    string fileIconsPatternBegin = "icon-file-type-";
                    _fileIcons = new SortedDictionary<string, string>();
                    var filenames = Directory.GetFiles(Path.Combine(hostingEnvironment.WebRootPath, "images", "icons"), $"{fileIconsPatternBegin}*.png");
                    foreach (var path in filenames)
                    {
                        var filename = Path.GetFileNameWithoutExtension(path);
                        var filetype = filename.Substring(fileIconsPatternBegin.Length).ToLower();
                        _fileIcons[$".{filetype}"] = $"{filename}.png";
                    }
                }
            }
        }
    }

    private string GetFileIcon(string filename)
    {
        string ext = Path.GetExtension(filename).ToLower();
        string imageFile = null;
        _fileIcons.TryGetValue(ext, out imageFile);
        if (string.IsNullOrWhiteSpace(imageFile))
            imageFile = "icon-file-1.png";
        return imageFile;
    }

    [HttpGet]
    [Route("")]
    [Route("{*items}")]
    public async Task<IActionResult> ListDir()
    {
        try
        {
            ViewData["app.title"] = AppConfigSection.Current.Title;
            string targetFolder = Request.Path.GetMappedFolder();
            bool isFile = !Directory.Exists(targetFolder);
            if (isFile)
            {
                if (!System.IO.File.Exists(targetFolder))
                isFile = false;
            }
            if (isFile)
            {
                Stream fileStream = System.IO.File.OpenRead(targetFolder);
                string contentType = MimeHelper.GetMimeTypeFromFilename(targetFolder);
                FileResult fileResult = File(fileStream, contentType);
                Response.Headers.Add("temp-my", $"src: {targetFolder}");
                return fileResult;
            }

            if (!Directory.Exists(targetFolder)) return Redirect("/");

            string targetFolderName = Path.GetFileName(targetFolder);
            string webAddress = targetFolder.GetWebAddress();
            ViewData["folder"] = webAddress;
            ViewData["parentFolder"] = Path.GetFullPath(Path.Combine(targetFolder, "../")).GetWebAddress();
            ViewData["folderName"] = "";
            if (Request.Path != "/")
                ViewData["folderName"] = targetFolderName;

            string folderTitle = webAddress?.Trim('/');
            if (string.IsNullOrWhiteSpace(folderTitle))
                folderTitle = "Home";
            ViewData["folderTitle"] = folderTitle;

            List<string> folders = Directory.GetDirectories(targetFolder).ToList();
            List<string> files = Directory.GetFiles(targetFolder).ToList();
            List<FileSystemItem> fileSystemItems = new List<FileSystemItem>();

            fileSystemItems.AddRange(GetFileSystemItemInfos(folders, files));
            ViewData["folder_content"] = fileSystemItems;
            ViewResult viewResult = View("~/Pages/Index.cshtml");
            return viewResult;
        }
        catch (Exception ex)
        {
            return Content($"Error while gathering directory content({Request.Path}): {ex.Message}");
        }
    }

    private FileSystemItem GetFileSystemItemInfo(string filePath, bool isFolder)
    {
        string itemName = Path.GetFileName(filePath);
        FileInfo fileInfo = null;
        if (!isFolder)
        {
            fileInfo = new FileInfo(filePath);
        }
        string url = filePath.GetWebAddress();
        string type = isFolder ? "<dir>" : Path.GetExtension(itemName).Trim('.').ToLower();
        string iconFilename = isFolder ? "icon-folder-2.png" : GetFileIcon(itemName);
        FileSystemItem fileSystemItem = new FileSystemItem
        {
            IsFolder = isFolder,
            Name = itemName,
            Url = url,
            SizeStr = fileInfo != null ? fileInfo.Length.GetFileSizeStr() : null,
            Type = type,
            LastModifyDate = fileInfo?.LastWriteTime,
            IconImageFileName = iconFilename,
        };
        return fileSystemItem;
    }

    private List<FileSystemItem> GetFileSystemItemInfos(List<string> folders, List<string> files)
    {
        List<FileSystemItem> fileSystemItems = new List<FileSystemItem>();
        for (int i = 0; i < folders.Count(); i++)
        {
            string itemPath = folders[i];
            try
            {
                var itemInfo = new DirectoryInfo(itemPath);
                if (!itemInfo.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    fileSystemItems.Add(GetFileSystemItemInfo(itemPath, true));
                }
            }
            catch { }
        }

        for (int i = 0; i < files.Count(); i++)
        {
            string itemPath = files[i];
            try
            {
                var itemInfo = new FileInfo(itemPath);
                if (!itemInfo.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    fileSystemItems.Add(GetFileSystemItemInfo(itemPath, false));
                }
            }
            catch { }
        }
        return fileSystemItems;
    }
}