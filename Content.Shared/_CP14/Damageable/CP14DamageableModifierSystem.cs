using Content.Shared.Damage;

namespace Content.Shared._CP14.Damageable;

public sealed class CP14DamageableModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14DamageableModifierComponent, DamageModifyEvent>(OnDamageModify);
    }

    private void OnDamageModify(Entity<CP14DamageableModifierComponent> ent, ref DamageModifyEvent args)
    {
        args.Damage *= ent.Comp.Modifier;
    }
}
