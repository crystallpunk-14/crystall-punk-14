using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Vampire.Components;

[RegisterComponent]
[NetworkedComponent]
[Access(typeof(CP14SharedVampireSystem))]
public sealed partial class CP14VampireTreeCollectableComponent : Component
{
    [DataField]
    public FixedPoint2 Essence = 1f;

    [DataField]
    public SoundSpecifier CollectSound = new SoundPathSpecifier("/Audio/_CP14/Effects/essence_consume.ogg");
}
