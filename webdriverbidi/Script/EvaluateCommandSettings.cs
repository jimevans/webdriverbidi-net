namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class EvaluateCommandSettings : CommandSettings
{
    private string expression;
    private ScriptTarget scriptTarget;
    private bool awaitPromise;
    private OwnershipModel? ownershipModel;

    public EvaluateCommandSettings(string expression, ScriptTarget scriptTarget, bool awaitPromise)
    {
        this.expression = expression;
        this.scriptTarget = scriptTarget;
        this.awaitPromise = awaitPromise;
    }

    public override string MethodName => "script.evaluate";

    public override Type ResultType => typeof(ScriptEvaluateResult);

    [JsonProperty("expression")]
    public string Expression { get => this.expression; set => this.expression = value; }

    [JsonProperty("target")]
    public ScriptTarget ScriptTarget { get => this.scriptTarget; set => this.scriptTarget = value; }

    [JsonProperty("awaitPromise")]
    public bool AwaitPromise { get => this.awaitPromise; set => this.awaitPromise = value; }

    public OwnershipModel? OwnershipModel { get => this.ownershipModel; set => this.ownershipModel = value;}

    [JsonProperty("resultOwnership", NullValueHandling = NullValueHandling.Ignore)]
    internal string? SerializableOwnershipModel
    {
        get
        {
            if (this.ownershipModel is null)
            {
                return null;
            }

            return this.ownershipModel.Value.ToString().ToLowerInvariant();
        }
    }
}