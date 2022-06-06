using folder_browser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

[ApiController]
[Route(GlobalScope.SYS_CALLS_ROUTE)]
public class BrowserController : Controller
{
    private Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment;
    
    public BrowserController(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
    {
        hostingEnvironment = env;
    }


    [HttpGet]
    [Route("file/{*items}")]
    public async Task<IActionResult> ServeAppContentFile()
    {
        var pathParts = Request.Path.GetFilePathParts().Where((s,i) => i >= 2).ToList();
        pathParts.Insert(0, hostingEnvironment.WebRootPath);
        var filePath = Path.GetFullPath(Path.Combine(pathParts.ToArray()));
        if (!System.IO.File.Exists(filePath)) return NotFound();
        var stream = System.IO.File.OpenRead(filePath);
        string contentType = MimeHelper.GetMimeTypeFromFilename(filePath);
        var fileResult = File(stream, contentType);
        return fileResult; 
    }
}