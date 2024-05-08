using Content.Shared.Interaction;

namespace Content.Server._CP14.Magic;

public sealed partial class CPMagicSpellContainerSystem
{
    private void InitializeSpell()
    {
        SubscribeLocalEvent<CPMagicSpellComponent, AfterInteractEvent>(OnInteract);
    }

    private void OnInteract(Entity<CPMagicSpellComponent> spell, ref AfterInteractEvent args)
    {
        var entity = Spawn(BaseSpellEffectEntity, args.ClickLocation.ToMap(EntityManager, _transform));
        foreach (var effect in spell.Comp.Effects)
        {
            EntityManager.AddComponents(entity, effect.Components);
        }

        var ev = new CPMagicCastedEvent(args.User, args.Target, args.ClickLocation);
        RaiseLocalEvent(entity, ev);

        Del(spell);
    }
}
