using System.Linq;
using System.Text;
using Content.Shared._CP14.AuraDNA;
using Content.Shared._CP14.MagicVision.Components;
using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.MagicVision;

public abstract class CP14SharedMagicVisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public readonly EntProtoId MagicTraceProto = "CP14MagicVisionMarker";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicVisionMarkerComponent, ExaminedEvent>(OnExamined);
    }

    protected virtual void OnExamined(Entity<CP14MagicVisionMarkerComponent> ent, ref ExaminedEvent args)
    {
        var sb = new StringBuilder();

        var timePassed = _timing.CurTime - ent.Comp.SpawnTime;
        sb.Append($"{Loc.GetString("cp14-magic-vision-timed-past")} {timePassed.Minutes}:{(timePassed.Seconds < 10 ? "0" : "")}{timePassed.Seconds}\n");

        if (ent.Comp.AuraImprint is not null)
        {
            sb.Append($"{Loc.GetString("cp14-magic-vision-aura")} {ent.Comp.AuraImprint}");
        }

        args.AddMarkup(sb.ToString());
    }

    public string? GetAuraImprint(Entity<CP14AuraImprintComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return null;

        if (_statusEffects.TryEffectsWithComp<CP14HideMagicAuraStatusEffectComponent>(ent, out var hideComps))
        {
            return hideComps.First().Comp1.Imprint;
        }

        return ent.Comp.Imprint;
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
    public void SpawnMagicTrace(EntityCoordinates position, SpriteSpecifier? icon, string description, TimeSpan duration, EntityUid? aura = null, EntityCoordinates? target = null)
    {
        if (_net.IsClient)
            return;

        var ent = SpawnAtPosition(MagicTraceProto, position);
        var markerComp = EnsureComp<CP14MagicVisionMarkerComponent>(ent);

        markerComp.SpawnTime = _timing.CurTime;
        markerComp.EndTime = _timing.CurTime + duration;
        markerComp.TargetCoordinates = target;
        markerComp.Icon = icon;

        if (aura is not null)
            markerComp.AuraImprint = GetAuraImprint(aura.Value);

        _meta.SetEntityDescription(ent, description);

        Dirty(ent, markerComp);
    }
}

public sealed partial class CP14MagicVisionToggleActionEvent : InstantActionEvent
{
}
