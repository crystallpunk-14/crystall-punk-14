using Content.Server._CP14.Magic.Events;

namespace Content.Server._CP14.Magic;

public sealed partial class CPMagicSpellContainerSystem
{
    private void InitializeHash()
    {
        SubscribeLocalEvent<CPMagicSpellContainerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CPMagicSpellContainerComponent, CPMagicSpellContainerListUpdatedEvent>(OnListUpdated);
    }

    private void OnStartup(Entity<CPMagicSpellContainerComponent> container, ref ComponentStartup args)
    {
        RaiseListUpdated(container);
    }

    private void OnListUpdated(Entity<CPMagicSpellContainerComponent> container, ref CPMagicSpellContainerListUpdatedEvent args)
    {
        container.Comp.EffectPrototypes = new List<CPMagicEffectPrototype>();
        container.Comp.TotalCompleteness = 0f;
        container.Comp.TotalCastTime = TimeSpan.Zero;

        foreach (var effectId in container.Comp.Effects)
        {
            if (!_prototype.TryIndex(effectId, out var prototype))
            {
                _sawmill.Error($"Impossible to find a prototype for {effectId}");
                return;
            }

            container.Comp.EffectPrototypes.Add(prototype);
            container.Comp.TotalCompleteness += prototype.Complexity;
            container.Comp.TotalCastTime += prototype.CastTime;
        }
    }

    private void RaiseListUpdated(Entity<CPMagicSpellContainerComponent> container)
    {
        var ev = new CPMagicSpellContainerListUpdatedEvent();
        RaiseLocalEvent(container, ev);
    }
}
