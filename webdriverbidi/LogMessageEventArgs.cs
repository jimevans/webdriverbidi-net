namespace WebDriverBidi;

public class LogMessageEventArgs : EventArgs
{
    private string message;
    private WebDriverBidiLogLevel level;

    public LogMessageEventArgs(string message) : this(message, WebDriverBidiLogLevel.Info)
    {
    }

    public LogMessageEventArgs(string message, WebDriverBidiLogLevel level)
    {
        this.message = message;
        this.level = level;
    }

    public string Message => this.message;

    public WebDriverBidiLogLevel Level => this.level;
}