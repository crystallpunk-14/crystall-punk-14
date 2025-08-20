
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.Vampire.Components;
using Content.Shared.Examine;
using Content.Shared.Mobs.Systems;
using Content.Shared.SSDIndicator;

namespace Content.Shared._CP14.Vampire;

public abstract partial class CP14SharedVampireSystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    private void InitializeSpell()
    {
        SubscribeLocalEvent<CP14MagicEffectAllVampireClanComponent, CP14CastMagicEffectAttemptEvent>(OnVampireClanCastAttempt);
        SubscribeLocalEvent<CP14MagicEffectAllVampireClanComponent, ExaminedEvent>(OnVampireClanCastExamine);

        SubscribeLocalEvent<CP14MagicEffectVampireComponent, CP14CastMagicEffectAttemptEvent>(OnVampireCastAttempt);
        SubscribeLocalEvent<CP14MagicEffectVampireComponent, ExaminedEvent>(OnVampireCastExamine);
    }

    private void OnVampireClanCastAttempt(Entity<CP14MagicEffectAllVampireClanComponent> ent, ref CP14CastMagicEffectAttemptEvent args)
    {
        //If we are not vampires in principle, we certainly should not have this ability,
        //but then we will not limit its use to a valid vampire form that is unavailable to us.

        if (!TryComp<CP14VampireComponent>(args.Performer, out var performerVampire))
            return;

        var query = EntityQueryEnumerator<CP14VampireComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var vampire, out var xform))
        {
            if (uid == ent.Owner)
                continue;

            if (vampire.Faction != performerVampire.Faction)
                continue;

            if (_mobState.IsDead(uid))
                continue;

            if (TryComp<SSDIndicatorComponent>(uid, out var ssd) && ssd.IsSSD)
                continue;

            //Check distance to the vampire
            if (!xform.Coordinates.TryDistance(EntityManager, Transform(args.Performer).Coordinates, out var distance) || distance > ent.Comp.Range)
            {
                args.PushReason("cp14-magic-spell-need-all-vampires");
                args.Cancel();
                return;
            }
        }
    }

    private void OnVampireClanCastExamine(Entity<CP14MagicEffectAllVampireClanComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup($"{Loc.GetString("cp14-magic-spell-need-all-vampires")}");
    }

    private void OnVampireCastAttempt(Entity<CP14MagicEffectVampireComponent> ent, ref CP14CastMagicEffectAttemptEvent args)
    {
        //If we are not vampires in principle, we certainly should not have this ability,
        //but then we will not limit its use to a valid vampire form that is unavailable to us.

        if (!HasComp<CP14VampireComponent>(args.Performer))
            return;

        if (!HasComp<CP14VampireVisualsComponent>(args.Performer))
        {
            args.PushReason(Loc.GetString("cp14-magic-spell-need-vampire-valid"));
            args.Cancel();
        }
    }

    private void OnVampireCastExamine(Entity<CP14MagicEffectVampireComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup($"{Loc.GetString("cp14-magic-spell-need-vampire-valid")}");
    }
}
