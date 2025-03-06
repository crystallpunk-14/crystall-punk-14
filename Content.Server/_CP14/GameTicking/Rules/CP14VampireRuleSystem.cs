using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server._CP14.Vampire;
using Content.Server.Body.Systems;
using Content.Server.GameTicking.Rules;
using Content.Shared.Damage;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14VampireRuleSystem : GameRuleSystem<CP14VampireRuleComponent>
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14VampireComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14VampireComponent> ent, ref MapInitEvent args)
    {
        _damageable.SetDamageModifierSetId(ent, "CP14Vampire");
        _bloodstream.ChangeBloodReagent(ent, ent.Comp.NewBloodReagent);
    }
}
