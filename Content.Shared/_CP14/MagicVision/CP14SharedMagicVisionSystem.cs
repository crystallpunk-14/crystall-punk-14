using System.Linq;
using System.Text;
using Content.Shared._CP14.AuraDNA;
using Content.Shared._CP14.MagicVision.Components;
using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Mobs;
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
        SubscribeLocalEvent<CP14MagicVisionStatusEffectComponent, StatusEffectRelayedEvent<MobStateChangedEvent>>(OnMobStateChange);
    }

    private void OnMobStateChange(Entity<CP14MagicVisionStatusEffectComponent> ent, ref StatusEffectRelayedEvent<MobStateChangedEvent> args)
    {
        if (args.Args.NewMobState == MobState.Alive) return;

        //Removes MagicVisionStatusEffect entity when MobState is Crit,Dead or Invalid
        TryQueueDel(ent);
    }

    protected virtual void OnExamined(Entity<CP14MagicVisionMarkerComponent> ent, ref ExaminedEvent args)
    {
        var sb = new StringBuilder();

        var timePassed = _timing.CurTime - ent.Comp.SpawnTime;
        var timeRemaining = ent.Comp.EndTime - _timing.CurTime;
        var totalDuration = ent.Comp.EndTime - ent.Comp.SpawnTime;

        sb.Append($"{Loc.GetString("cp14-magic-vision-timed-past")} {timePassed.Minutes}:{timePassed.Seconds:D2}\n");

        if (string.IsNullOrEmpty(ent.Comp.AuraImprint))
        {
            args.AddMarkup(sb.ToString());
            return;
        }

        var imprint = ent.Comp.AuraImprint;

        // Try to extract the content between [color=...] and [/color]
        var startTag = imprint.IndexOf(']') + 1;
        var endTag = imprint.LastIndexOf('[');
        if (startTag <= 0 || endTag <= startTag)
        {
            sb.Append($"{Loc.GetString("cp14-magic-vision-aura")} {imprint}");
            args.AddMarkup(sb.ToString());
            return;
        }

        var content = imprint[startTag..endTag];
        var obscuredContent = new StringBuilder(content.Length);

        // Progress goes from 0 (fresh) to 1 (completely faded)
        var progress = Math.Clamp(1.0 - (timeRemaining.TotalSeconds / totalDuration.TotalSeconds), 0.0, 1.0);

        // Number of characters to obscure
        var charsToObscure = (int)Math.Round(content.Length * progress);

        // Deterministic pseudo-random based on content + entity id
        var hash = (content + ent.Owner.Id).GetHashCode();

        var obscuredCount = 0;
        for (var i = 0; i < content.Length; i++)
        {
            // Pick bits from hash + index to ensure distribution
            var mask = ((hash >> (i % 32)) ^ i) & 1;
            var shouldObscure = obscuredCount < charsToObscure && mask == 1;

            obscuredContent.Append(shouldObscure ? '~' : content[i]);
            if (shouldObscure)
                obscuredCount++;
        }

        // If we still didn't obscure enough (due to unlucky bit pattern), obscure remaining from the start
        while (obscuredCount < charsToObscure && obscuredCount < obscuredContent.Length)
        {
            for (var i = 0; i < obscuredContent.Length && obscuredCount < charsToObscure; i++)
            {
                if (obscuredContent[i] != '~')
                {
                    obscuredContent[i] = '~';
                    obscuredCount++;
                }
            }
        }

        var obscuredImprint = imprint.Substring(0, startTag) + obscuredContent + imprint.Substring(endTag);
        sb.Append($"{Loc.GetString("cp14-magic-vision-aura")} {obscuredImprint}");

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
    public void SpawnMagicTrace(EntityCoordinates position,
        SpriteSpecifier? icon,
        string description,
        TimeSpan duration,
        EntityUid? aura = null,
        EntityCoordinates? target = null)
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
