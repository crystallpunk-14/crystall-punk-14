using Content.Server.Actions;
using Content.Server.GameTicking;
using Content.Shared.Actions;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Server.Traits;

public sealed class TraitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedHandsSystem _sharedHandsSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private readonly ActionsSystem _action = default!; //CP14
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!; //CP14

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    // When the player is spawned in, add all trait components selected during character creation
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        // Check if player's job allows to apply traits
        if (args.JobId == null ||
            !_prototypeManager.TryIndex<JobPrototype>(args.JobId ?? string.Empty, out var protoJob) ||
            !protoJob.ApplyTraits)
        {
            return;
        }

        foreach (var traitId in args.Profile.TraitPreferences)
        {
            if (!_prototypeManager.TryIndex<TraitPrototype>(traitId, out var traitPrototype))
            {
                Log.Warning($"No trait found with ID {traitId}!");
                return;
            }

            if (_whitelistSystem.IsWhitelistFail(traitPrototype.Whitelist, args.Mob) ||
                _whitelistSystem.IsBlacklistPass(traitPrototype.Blacklist, args.Mob))
                continue;
            
            // CP14 start - add all spells to player mind
            foreach (var spell in traitPrototype.Actions)
                _action.AddAction(args.Mob, spell);
            //CP14 end

            // Add all components required by the prototype
            if (traitPrototype.Components.Count > 0) //CP14 added check
                EntityManager.AddComponents(args.Mob, traitPrototype.Components, false);

            // Add item required by the trait
            if (traitPrototype.TraitGear == null)
                continue;

            if (!TryComp(args.Mob, out HandsComponent? handsComponent))
                continue;

            var coords = Transform(args.Mob).Coordinates;
            var inhandEntity = EntityManager.SpawnEntity(traitPrototype.TraitGear, coords);
            _sharedHandsSystem.TryPickup(args.Mob,
                inhandEntity,
                checkActionBlocker: false,
                handsComp: handsComponent);
        }
    }
}
