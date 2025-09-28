using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicVision;
using Content.Shared._CP14.Religion.Systems;
using Content.Shared._CP14.Skill;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Actions;

public abstract partial class CP14SharedActionSystem : EntitySystem
{
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedHandsSystem _hand = default!;
    [Dependency] private readonly CP14SharedMagicEnergySystem _magicEnergy = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly CP14SharedReligionGodSystem _god = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly CP14SharedSkillSystem _skill = default!;
    [Dependency] private readonly CP14SharedMagicVisionSystem _magicVision = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;

    private EntityQuery<ActionComponent> _actionQuery;

    public override void Initialize()
    {
        base.Initialize();

        _actionQuery = GetEntityQuery<ActionComponent>();

        InitializeAttempts();
        InitializeExamine();
        InitializePerformed();
        InitializeModularEffects();
        InitializeDoAfter();
    }
}

/// <summary>
/// Called on an action when an attempt to start doAfter using this action begins.
/// </summary>
public sealed class CP14ActionStartDoAfterEvent(NetEntity performer, RequestPerformActionEvent input) : EntityEventArgs
{
    public NetEntity Performer = performer;
    public readonly RequestPerformActionEvent Input = input;
}
