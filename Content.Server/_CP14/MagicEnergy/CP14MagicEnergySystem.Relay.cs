using System.Linq;
using System.Numerics;
using Content.Server._CP14.MagicEnergy.Components;
using Content.Server.Beam;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Interaction;

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

        var query = EntityQueryEnumerator<CP14MagicEnergyRelayTargetComponent>(); //TODO burn this shit
        while (query.MoveNext(out var uid, out var relayTarget))
        {
            foreach (var source in relayTarget.Source)
            {
                if (!TryComp<CP14MagicEnergyRelayComponent>(source, out var relay))
                    continue;
                _beam.TryCreateBeam(uid, source, relay.BeamProto);
            }
        }
    }

    private void OnAfterGauntletInteract(Entity<CP14PowerlineGauntletComponent> ent, ref AfterInteractEvent args) //TODO reverse logic - save targets on source, not source on targets
    {
        if (args.Target is not { } target)
            return;
        if (!args.CanReach)
            return;
        if (!TryComp<CP14MagicEnergyRelayTargetComponent>(target, out var relayTarget))
            return;
        if (!TryComp<CP14MagicEnergyRelayTargetComponent>(ent, out var gauntletRelayTarget))
            return;

        if (gauntletRelayTarget.Source.Count == 0) //if not bound - add a link relay -> gauntlet
        {
            TryAddRelaySource((ent, gauntletRelayTarget), target, args.User);
        }
        else //if bound - transfer the link from the glove to the second object.
        {
            var source = gauntletRelayTarget.Source.FirstOrDefault();
            RemoveRelaySource((ent, gauntletRelayTarget), source, args.User);
            TryAddRelaySource((target, relayTarget), source, args.User);
        }
    }

    private bool TryAddRelaySource(Entity<CP14MagicEnergyRelayTargetComponent> target, EntityUid source, EntityUid? user) //TODO - block ability connect item to self
    {
        if (!TryComp<CP14MagicEnergyRelayComponent>(source, out var sourceRelay))
            return false;

        if (Vector2.Distance(_transform.GetWorldPosition(target), _transform.GetWorldPosition(source)) > sourceRelay.Radius)
            return false;

        if (target.Comp.Source.Contains(source))
            return false; //Popups to user

        if (TryComp<CP14MagicEnergyRelayTargetComponent>(source, out var sourceRelayTarget))
        {
            if (sourceRelayTarget.Source.Contains(target)) //Loop, remove both
            {
                RemoveRelaySource(target, source, user);
                RemoveRelaySource((source, sourceRelayTarget), target, user);
                return false;
            }
        }

        target.Comp.Source.Add(source);
        _beam.TryCreateBeam(target, source, sourceRelay.BeamProto);
        return true;
    }

    private void RemoveRelaySource(Entity<CP14MagicEnergyRelayTargetComponent> target, EntityUid source, EntityUid? user)
    {
        if (target.Comp.Source.Contains(source))
            target.Comp.Source.Remove(source);
    }
}
