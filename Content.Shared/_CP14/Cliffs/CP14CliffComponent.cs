using System.Numerics;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Shared._CP14.Cliffs;


[RegisterComponent]
public sealed partial class CP14CliffComponent : Component
{
    [DataField(required: true)]
    public string TriggerFallFixtureId = string.Empty;

    [DataField]
    public Angle fallDirection = Angle.Zero;

    [DataField]
    public float LaunchForwardsMultiplier = 2f;

    [DataField]
    public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(3f);

    [DataField]
    public DamageSpecifier FallDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 15.00 },
        },
    };

    [DataField]
    public SoundSpecifier FallSound = new SoundPathSpecifier("/Audio/Effects/slip.ogg");
}
