using Content.Shared.Tools;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.DestroyedByTool;

/// <summary>
/// abstract ability to destroy objects by using the right kind of tool on them
/// </summary>
[RegisterComponent, Access(typeof(CP14DestroyedByToolSystem))]
public sealed partial class CP14DestroyedByToolComponent : Component
{
    [DataField]
    public ProtoId<ToolQualityPrototype>? Tool;

    [DataField]
    public TimeSpan RemoveTime = TimeSpan.FromSeconds(1f);
}
