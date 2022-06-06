using folder_browser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

[ApiController]
[Route(GlobalScope.SYS_CALLS_ROUTE)]
public class BrowserController : Controller
{
    private Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment;
    
    public BrowserController(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    {
        hostingEnvironment = env;
    }


    [HttpGet]
    [Route("file/{*items}")]
    public async Task<IActionResult> ServeAppContentFile()
    {
        var taskParams = new
        {
            Request = Request,
            WebHostingEnvironment = hostingEnvironment,
        };
        var task = new Task<IActionResult>(() =>
        {
            var pathParts = taskParams.Request.Path.GetFilePathParts().Where((s,i) => i >= 2).ToList();
            pathParts.Insert(0, taskParams.WebHostingEnvironment.WebRootPath);
            var filePath = Path.GetFullPath(Path.Combine(pathParts.ToArray()));
            if (!System.IO.File.Exists(filePath)) return NotFound();
            var stream = System.IO.File.OpenRead(filePath);
            string contentType = MimeHelper.GetMimeTypeFromFilename(filePath);
            var fileResult = File(stream, contentType);
            return fileResult; 
        });
        task.Start();
        return await task;
    }
}