using folder_browser.Models;

public class FileSystemHelper
{
    private static SortedDictionary<string, string>? _fileIcons;
    private static object _iconsLoadLock = new object();

    public static void LoadIcons(Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment)
    {
        if (_fileIcons == null)
        {
            lock (_iconsLoadLock)
            {
                if (_fileIcons == null)
                {
                    string fileIconsPatternBegin = "icon-file-type-";
                    _fileIcons = new SortedDictionary<string, string>();
                    var filenames = Directory.GetFiles(Path.Combine(webHostEnvironment.WebRootPath, "images", "icons"), $"{fileIconsPatternBegin}*.png");
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

    public static string GetFileIcon(string filename)
    {
        string ext = Path.GetExtension(filename).ToLower();
        string? imageFile = null;
        _fileIcons?.TryGetValue(ext, out imageFile);
        if (string.IsNullOrWhiteSpace(imageFile))
            imageFile = "icon-file-1.png";
        return imageFile;
    }

    public static FileSystemItem GetFileSystemItemInfo(string filePath, bool isFolder)
    {
        string itemName = Path.GetFileName(filePath);
        FileInfo? fileInfo = null;
        DirectoryInfo? directoryInfo = null;
        if (!isFolder)
            fileInfo = new FileInfo(filePath);
        else
            directoryInfo = new DirectoryInfo(filePath);
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
            LastModifyDate = isFolder ? directoryInfo?.LastWriteTime : fileInfo?.LastWriteTime,
            IconImageFileName = iconFilename,
        };
        return fileSystemItem;
    }

    public static List<FileSystemItem> GetFileSystemItemInfos(List<string> folders, List<string> files)
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