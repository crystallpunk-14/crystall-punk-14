using Robust.Shared.Prototypes;
using Content.Shared._CP14.MagicWand;
using Content.Shared.FixedPoint;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.MagicWand.Components;

[RegisterComponent, Access(typeof(CP14MagicWandSystem))]
public sealed partial class CP14MagicWandComponent : Component
{
    [DataField]
    public FixedPoint2 ManaCost = 0f;

    [DataField]
    public TimeSpan RuneActivationDelay = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan WandShootDelay = TimeSpan.FromSeconds(10);

    [DataField]
    public int Spread = 45;
}
