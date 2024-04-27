using Content.Shared.Mobs.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._CP14.Magic.Spells;

[Serializable, DataDefinition]
public abstract partial class MagicSpellPointRandomMob : MagicSpell
{
    private readonly EntityLookupSystem _entityLookupSystem;
    private readonly IRobustRandom _random;
    private readonly TransformSystem _transform;

    [DataField]
    public virtual float Range { get; set; } = 5f;

    [DataField]
    public override int BaseCost { get; set; } = 50;

    public MagicSpellPointRandomMob()
    {
        _entityLookupSystem = EntityManager.System<EntityLookupSystem>();
        _transform = EntityManager.System<TransformSystem>();

        _random = IoCManager.Resolve<IRobustRandom>();
    }

    public override void Modify(MagicSpellContext context)
    {
        base.Modify(context);

        var targets = _entityLookupSystem.GetEntitiesInRange<MobStateComponent>(context.SourcePoint, Range);
        if (targets.Count == 0)
            return;

        var target = _random.Pick(targets);
        var coordinates = _transform.GetMapCoordinates(target);

        ApplyCoordinates(context, coordinates);
    }

    public abstract void ApplyCoordinates(MagicSpellContext context, MapCoordinates coordinates);
}
