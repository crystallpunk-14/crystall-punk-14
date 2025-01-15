using Content.Server.Popups;
using Content.Shared._CP14.Knowledge;
using Content.Shared.Damage;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Throwing;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Knowledge;

public sealed partial class CP14KnowledgeSystem : SharedCP14KnowledgeSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
    }
}
