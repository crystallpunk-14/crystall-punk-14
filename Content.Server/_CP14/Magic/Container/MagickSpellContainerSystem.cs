namespace Content.Server._CP14.Magic.Container;

public sealed class MagickSpellContainerSystem : EntitySystem
{
    [Dependency] private readonly MagicSpellSystem _magicSpell = default!;

    public void CastSpell(Entity<MagicSpellContainerComponent> container)
    {
        _magicSpell.Cast(container, container.Comp.Spells);
    }
}
