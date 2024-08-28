
namespace Content.Server._CP14.Alchemy;

[RegisterComponent, Access(typeof(CP14AlchemyExtractionSystem))]
public sealed partial class CP14MortarComponent : Component
{
    [DataField(required: true)]
    public string Solution = string.Empty;

    [DataField(required: true)]
    public string ContainerId = string.Empty;
}

