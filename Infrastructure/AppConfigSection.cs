public class AppConfigSection
{
    private static AppConfigSection _current;
    private static object _currentLock = new object();

    public static AppConfigSection Current 
    {
        get
        {
            if (_current == null)
            {
                lock (_currentLock)
                {
                    if (_current == null)
                    {
                        _current = new AppConfigSection
                        {
                            HiddenFileNames = new List<string>
                            {
                                
                            }
                        };
                    }
                }
            }
            return _current;
        }
        set
        {
            _current = value;
        }
    }

    public string? RootPath { get; set; }
    public List<string> HiddenFileNames { get; set; }
    public string Title { get; set; }
}