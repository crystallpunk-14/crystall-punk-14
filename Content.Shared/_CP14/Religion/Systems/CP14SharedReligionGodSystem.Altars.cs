using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Religion.Systems;

public abstract partial class CP14SharedReligionGodSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    private void InitializeAltars()
    {
        SubscribeLocalEvent<CP14ReligionAltarComponent, GetVerbsEvent<AlternativeVerb>>(GetAltVerb);
    }

    private void GetAltVerb(Entity<CP14ReligionAltarComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (ent.Comp.Religion is null)
            return;

        var disabled = !CanBecomeFollower(args.User, ent.Comp.Religion.Value);

        if (!disabled && TryComp<CP14ReligionPendingFollowerComponent>(args.User, out var pendingFollower))
        {
            if (pendingFollower.Religion is not null)
                disabled = true;
        }

        if (disabled)
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb()
        {
            Text = Loc.GetString("cp14-altar-become-follower"),
            Message = Loc.GetString("cp14-altar-become-follower-desc"),
            Act = () =>
            {
                var doAfterArgs = new DoAfterArgs(EntityManager, user, 5f, new CP14AltarOfferDoAfter(), ent, used: ent)
                {
                    BreakOnDamage = true,
                    BreakOnMove = true,
                };
                _doAfter.TryStartDoAfter(doAfterArgs);
            },
        });
    }
}

[Serializable, NetSerializable]
public sealed partial class CP14AltarOfferDoAfter : SimpleDoAfterEvent
{
}
