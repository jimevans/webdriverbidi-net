namespace WebDriverBidi.TestUtilities;

public class TestCommandSetEventArgs: WebDriverBidiEventArgs
{
    private readonly string commandName;
    private readonly Type resultType;

    public TestCommandSetEventArgs(string commandName, Type resultType)
    {
        this.commandName = commandName;
        this.resultType = resultType;
    }

    public string MethodName => this.commandName;

    public Type ResultType => this.resultType;
}