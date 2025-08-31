using Content.Server.Body.Systems;
using Content.Server.Kitchen.Components;
using Content.Shared._CP14.Butchering;
using Content.Shared.DoAfter;
using Content.Shared.Kitchen;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._CP14.Butchering;

/// <summary>
/// Server-side system that implements staged butchering.
/// It listens for vanilla DoAfter completions (knife/spike) and, if the target
/// has CP14ButcherableStagesComponent, replaces vanilla "dump-all" with staged logic.
/// </summary>
public sealed class CP14ButcheringSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Handle DoAfter completion on the TARGET entity (the corpse/creature being butchered).
        SubscribeLocalEvent<CP14ButcherableStagesComponent, SharpDoAfterEvent>(OnKnifeDoAfter);
        SubscribeLocalEvent<CP14ButcherableStagesComponent, SpikeDoAfterEvent>(OnSpikeDoAfter);
    }

    private void OnKnifeDoAfter(Entity<CP14ButcherableStagesComponent> ent, ref SharpDoAfterEvent args)
    {
        // This handler executes on the TARGET; vanilla SharpSystem handles on the KNIFE.
        // We must fully consume the event here and clean up SharpSystem state.
        if (args.Cancelled || args.Args.Target == null || args.Handled)
            return;

        var target = args.Args.Target.Value;

        // Only proceed if this event actually belongs to our target.
        if (target != ent)
            return;

        // Mark as handled so vanilla SharpSystem won't do default butchering.
        args.Handled = true;

        // --- Clean up vanilla SharpSystem bookkeeping so nothing gets stuck ---
        // Remove target from the knife's SharpComponent.Butchering set
        if (args.Args.Used != null && TryComp<SharpComponent>(args.Args.Used.Value, out var sharp))
            sharp.Butchering.Remove(target);

        // Clear BeingButchered flag if present (parity with spike flow)
        if (TryComp<ButcherableComponent>(target, out var butcherComp))
            butcherComp.BeingButchered = false;

        DoStage(ent, args.Args.User, isSpike: false, used: args.Args.Used);
    }

    private void OnSpikeDoAfter(Entity<CP14ButcherableStagesComponent> ent, ref SpikeDoAfterEvent args)
    {
        // This handler executes on the TARGET; vanilla KitchenSpikeSystem handles on the SPIKE.
        if (args.Cancelled || args.Args.Target == null || args.Handled)
            return;

        var target = args.Args.Target.Value;

        if (target != ent)
            return;

        // Mark as handled so KitchenSpikeSystem won't spike/gib/delete.
        args.Handled = true;

        // --- Bring KitchenSpike state back to idle to avoid "stuck in use" ---
        if (args.Args.Used != null && TryComp<KitchenSpikeComponent>(args.Args.Used.Value, out var spike))
            spike.InUse = false;

        // KitchenSpikeSystem clears BeingButchered before checking Handled,
        // but if we ran first we should ensure it's false.
        if (TryComp<ButcherableComponent>(target, out var butcherComp))
            butcherComp.BeingButchered = false;

        DoStage(ent, args.Args.User, isSpike: true, used: args.Args.Used);
    }

    /// <summary>
    /// Execute a staged step: spawn entries for the current stage and
    /// either advance to the next stage or finish by deleting the entity.
    /// </summary>
    private void DoStage(Entity<CP14ButcherableStagesComponent> ent, EntityUid user, bool isSpike, EntityUid? used)
    {
        ref var comp = ref ent.Comp;

        if (comp.CurrentStage < 0 || comp.CurrentStage >= comp.Stages.Count)
            return;

        var stage = comp.Stages[comp.CurrentStage];
        var xform = Transform(ent);
        var coords = xform.Coordinates;

        // Spawn loot for this stage
        foreach (var entry in stage.Spawned)
        {
            // Use vanilla collection util to respect amount/prob/maxAmount.
            var list = EntitySpawnCollection.GetSpawns(new List<EntitySpawnEntry> { entry }, _random);
            foreach (var proto in list)
                Spawn(proto, coords);
        }

        if (stage.FinalStage)
        {
            // Optional gibbing before deletion (if body exists)
            if (stage.GibOnFinal && TryComp<MobStateComponent>(ent, out var mobState) && !_mobState.IsAlive(ent, mobState))
            {
                // Only gib corpses / living mobs with bodies. If no body, skip.
                _body.GibBody(ent, deleteItems: false);
            }

            // Final popup (configurable)
            if (comp.FinalStagePopup is { } finalMsg)
                _popup.PopupEntity(finalMsg, ent, user, PopupType.LargeCaution);
            else
                _popup.PopupEntity("The body has been fully butchered.", ent, user, PopupType.LargeCaution);

            // Delete the entity to finish butchering.
            Del(ent);
            return;
        }

        // Mid-stage: advance and inform user.
        comp.CurrentStage++;

        if (comp.MidStagePopup is { } midMsg)
            _popup.PopupEntity(midMsg, ent, user, PopupType.MediumCaution);
        else
            _popup.PopupEntity("You slice off a part...", ent, user, PopupType.MediumCaution);

        // Optional: tiny visual/audio hooks could be added here later if desired.
    }
}
