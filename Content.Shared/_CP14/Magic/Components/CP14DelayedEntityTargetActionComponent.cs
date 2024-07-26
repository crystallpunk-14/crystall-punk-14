using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.EntityEffects;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Magic.Components;

/// <summary>
/// Stores a list of effects for delayed actions.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14DelayedEntityTargetActionComponent : Component
{
    [DataField(required: true, serverOnly: true)]
    public List<EntityEffect> Effects = new();
}
