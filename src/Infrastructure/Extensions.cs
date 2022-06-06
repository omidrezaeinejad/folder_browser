public static class Extensions
{
    private static string[] sizeUnits = new string[]{ "B", "KB", "MB", "GB" };

    public static string GetFileSizeStr(this long fileLength)
    {
        int unitIndex = 0;
        decimal len = fileLength;
        while (len >= 1024 && unitIndex < sizeUnits.Length - 1)
        {
            len /= 1024;
            unitIndex++;
        }
        return $"{Math.Round(len, 2)} {sizeUnits[unitIndex]}";
    }

    public static string GetMappedFolder(this PathString path)
    {
        List<string> requestPathParts = (path.Value ?? "").GetFilePathParts().ToList();
        requestPathParts.Insert(0, AppConfigSection.Current?.RootPath ?? ".");
        string dirPath = Path.GetFullPath(Path.Combine(requestPathParts.ToArray()));
        return dirPath;
    }

    public static string GetWebAddress(this string filepath)
    {
        string lowerPath = filepath?.ToLower() ?? "";
        if (string.IsNullOrWhiteSpace(lowerPath)) return "/";
        string lowerRoot = AppConfigSection.Current?.RootPath?.ToLower() ?? "";
        if (!lowerPath.StartsWith(lowerRoot)) return "/";
        string? relativePath = filepath?.Substring(AppConfigSection.Current?.RootPath?.Length ?? 0).Trim('/');
        List<string> pathParts = (relativePath ?? "").GetFilePathParts().ToList();
        string webAddress = $"/{string.Join("/", pathParts)}";
        return webAddress;
    }

    public static IEnumerable<string> GetFilePathParts(this PathString path) => GetFilePathParts(path.Value ?? "");
    public static IEnumerable<string> GetFilePathParts(this string path)
    {
        var requestPathParts = (path ?? "")
            .Split('/')
            .Select(e => e.Split('\\'))
            .SelectMany(e => e)
            .Where(e => !string.IsNullOrWhiteSpace(e));
        return requestPathParts;
    }
}