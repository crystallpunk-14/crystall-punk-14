using Content.Server.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Magic;

public sealed class CPMagicSpellContainerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CPMagicSpellContainerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerb);
    }

    private void OnGetVerb(Entity<CPMagicSpellContainerComponent> container, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = "cast",
            Disabled = container.Comp.Effects.Count == 0,
            Priority = 10,
            Act = () =>
            {
                Cast(container, user);
            }
        });
    }

    public void Cast(Entity<CPMagicSpellContainerComponent> container, EntityUid caster)
    {
        var effectPrototypes = new List<CPMagicEffectPrototype>();
        var complexity = 0f;

        foreach (var effectId in container.Comp.Effects)
        {
            if (!_prototype.TryIndex(effectId, out var prototype))
            {
                _popup.PopupEntity("Fuck!", container);
                return;
            }

            complexity += prototype.Complexity;
            effectPrototypes.Add(prototype);
        }

        if (complexity > container.Comp.MaximumCompleteness)
        {
            _popup.PopupEntity("Too much complicated", container);
            return;
        }

        var entity = Spawn(container.Comp.BaseSpellEffectEntity, Transform(container).Coordinates);
        foreach (var effect in effectPrototypes)
        {
            EntityManager.AddComponents(entity, effect.Components);
        }
    }
}
