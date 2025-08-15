using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Components;

/// <summary>
/// Allows you to see what skills the creature possesses
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14SkillPointConsumableComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<CP14SkillPointPrototype> PointType = "Memory";

    /// <summary>
    /// How much skill points this consumable gives when consumed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 Volume = 1f;

    /// <summary>
    /// The visual effect that appears on the client when the player consumes this skill point.
    /// </summary>
    [DataField]
    public EntProtoId? ConsumeEffect;

    [DataField]
    public SoundSpecifier ConsumeSound = new SoundPathSpecifier("/Audio/_CP14/Effects/essence_consume.ogg")
    {
        Params = AudioParams.Default.WithVolume(-2f).WithVariation(0.2f),
    };

    /// <summary>
    /// White list of who can absorb this skill point
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;
}
