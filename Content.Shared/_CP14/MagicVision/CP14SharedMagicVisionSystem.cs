using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Mobs;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.MagicVision;

public abstract class CP14SharedMagicVisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

    public readonly EntProtoId MagicTraceProto = "CP14MagicVisionMarker";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicVisionMarkerComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CP14MagicEnergyContainerComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(Entity<CP14MagicEnergyContainerComponent> ent, ref MobStateChangedEvent args)
    {
        switch (args.NewMobState)
        {
            case MobState.Critical:
            {
                SpawnMagicVision(
                    Transform(ent).Coordinates,
                    new SpriteSpecifier.Rsi(new ResPath("_CP14/Actions/Spells/misc.rsi"), "skull"),
                    Loc.GetString("cp14-magic-vision-crit"),
                    TimeSpan.FromMinutes(10));
                break;
            }
            case MobState.Dead:
            {
                SpawnMagicVision(
                    Transform(ent).Coordinates,
                    new SpriteSpecifier.Rsi(new ResPath("_CP14/Actions/Spells/misc.rsi"), "skull_red"),
                    Loc.GetString("cp14-magic-vision-dead"),
                    TimeSpan.FromMinutes(10));
                break;
            }
        }
    }

    protected virtual void OnExamined(Entity<CP14MagicVisionMarkerComponent> ent, ref ExaminedEvent args)
    {

    }

    /// <summary>
    /// Creates an invisible “magical trace” entity that can be seen with magical vision.
    /// </summary>
    /// <param name="position">Coordinates where the magic trail will be created</param>
    /// <param name="icon">Magic trace icon</param>
    /// <param name="description">Description that can be obtained when examining the magical trace</param>
    /// <param name="duration">Duration of the magical trace</param>
    /// <param name="target">Optional: The direction in which this trace “faces.” When studying the trace,
    /// this direction can be seen in order to understand, for example, in which direction the spell was used.</param>
    public void SpawnMagicVision(EntityCoordinates position, SpriteSpecifier? icon, string description, TimeSpan duration, EntityCoordinates? target = null)
    {
        var ent = PredictedSpawnAtPosition(MagicTraceProto, position);
        var markerComp = EnsureComp<CP14MagicVisionMarkerComponent>(ent);

        markerComp.SpawnTime = _timing.CurTime;
        markerComp.EndTime = _timing.CurTime + duration;
        markerComp.TargetCoordinates = target;

        markerComp.Icon = icon;

        _meta.SetEntityDescription(ent, description);

        Dirty(ent, markerComp);
    }
}

public sealed partial class CP14MagicVisionToggleActionEvent : InstantActionEvent
{
}
