using folder_browser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

[ApiController]
[Route("")]
public class FileController : Controller
{
    private Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment;
    
    public FileController(Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment)
    {
        this.webHostEnvironment = webHostEnvironment;
        FileSystemHelper.LoadIcons(this.webHostEnvironment);
    }

    [HttpGet]
    [Route("")]
    [Route("{*items}")]
    public async Task<IActionResult> ListDir()
    {
        var taskParams = new 
        {
            Request = Request,
            ViewData = ViewData
        };
        var task = new Task<IActionResult>(() =>
        {
            try
            {
                taskParams.ViewData["app.title"] = AppConfigSection.Current.Title;
                string targetFolder = taskParams.Request.Path.GetMappedFolder();
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
                    return fileResult;
                }

                if (!Directory.Exists(targetFolder)) return Redirect("/");

                string targetFolderName = Path.GetFileName(targetFolder);
                string webAddress = targetFolder.GetWebAddress();
                taskParams.ViewData["folder"] = webAddress;
                taskParams.ViewData["parentFolder"] = Path.GetFullPath(Path.Combine(targetFolder, "../")).GetWebAddress();
                taskParams.ViewData["folderName"] = "";
                if (Request.Path != "/")
                    taskParams.ViewData["folderName"] = targetFolderName;

                string? folderTitle = webAddress?.Trim('/');
                if (string.IsNullOrWhiteSpace(folderTitle))
                    folderTitle = "Home";
                taskParams.ViewData["folderTitle"] = folderTitle;

                List<string> folders = Directory.GetDirectories(targetFolder).ToList();
                List<string> files = Directory.GetFiles(targetFolder).ToList();
                List<FileSystemItem> fileSystemItems = new List<FileSystemItem>();

                fileSystemItems.AddRange(FileSystemHelper.GetFileSystemItemInfos(folders, files));
                taskParams.ViewData["folder_content"] = fileSystemItems;
                ViewResult viewResult = View("~/Pages/Index.cshtml");
                return viewResult;
            }
            catch (Exception ex)
            {
                return Content($"Error while gathering directory content({Request.Path}): {ex.Message}");
            }
        });
        task.Start();
        return await task;
    }
}