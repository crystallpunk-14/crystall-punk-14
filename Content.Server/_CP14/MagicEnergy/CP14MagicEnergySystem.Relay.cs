using System.Linq;
using System.Numerics;
using Content.Server._CP14.MagicEnergy.Components;
using Content.Server.Beam;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Interaction;
using CP14PowerlineGauntletComponent = Content.Server._CP14.MagicEnergy.Components.CP14PowerlineGauntletComponent;

namespace Content.Server._CP14.MagicEnergy;

public partial class CP14MagicEnergySystem
{
    [Dependency] private readonly BeamSystem _beam = default!;

    private static readonly float UpdateFrequency = 3f;
    private float UpdateTime = 0f;
    private void InitializeRelay()
    {
        SubscribeLocalEvent<CP14PowerlineGauntletComponent, AfterInteractEvent>(OnAfterGauntletInteract);
    }

    private void UpdateRelay(float frameTime)
    {
        UpdateTime += frameTime;
        if (UpdateTime < UpdateFrequency)
            return;
        else
            UpdateTime = 0;

        var query = EntityQueryEnumerator<CP14MagicEnergyRelayComponent>(); //TODO burn this shit
        while (query.MoveNext(out var uid, out var relay))
        {
            foreach (var target in relay.Targets)
            {
                _beam.TryCreateBeam(target, uid, relay.BeamProto);
            }
        }
    }

    private void OnAfterGauntletInteract(Entity<CP14PowerlineGauntletComponent> gauntlet, ref AfterInteractEvent args)
    {
        if (args.Target is not { } target)
            return;
        if (!args.CanReach)
            return;
        if (!TryComp<CP14MagicEnergyRelayComponent>(target, out var otherRelay))
            return;

        if (gauntlet.Comp.LinkedRelay == null) //if not bound - add a link relay -> gauntlet
        {
            gauntlet.Comp.LinkedRelay = (target, otherRelay);
        }
        else //if bound - transfer the link from the glove to the second object.
        {
            TryAddRelayTarget(gauntlet.Comp.LinkedRelay, target, args.User);
            gauntlet.Comp.LinkedRelay = null;
        }
    }

    private bool TryAddRelayTarget(Entity<CP14MagicEnergyRelayComponent>? relay, EntityUid target, EntityUid? user)
    {
        if (relay == null)
            return false;

        if (!EntityManager.EntityExists(relay))
            return false;

        if (!HasComp<CP14MagicEnergyRelayTargetComponent>(target))
            return false;

        if (Vector2.Distance(_transform.GetWorldPosition(relay.Value), _transform.GetWorldPosition(target)) > relay.Value.Comp.Radius)
            return false;

        if (target == relay.Value.Owner)
            return false;

        if (relay.Value.Comp.Targets.Contains(target))
            return false; //Popups to user

        if (TryComp<CP14MagicEnergyRelayComponent>(target, out var targetRelay))
        {
            if (targetRelay.Targets.Contains(relay.Value)) //Loop, remove both
            {
                RemoveRelayTarget(relay.Value, target, user);
                RemoveRelayTarget((target, targetRelay), relay.Value, user);
                return false;
            }
        }

        relay.Value.Comp.Targets.Add(target);
        return true;
    }

    private void RemoveRelayTarget(Entity<CP14MagicEnergyRelayComponent> relay,
        EntityUid target,
        EntityUid? user)
    {
        if (relay.Comp.Targets.Contains(target))
            relay.Comp.Targets.Remove(target);
    }
}
