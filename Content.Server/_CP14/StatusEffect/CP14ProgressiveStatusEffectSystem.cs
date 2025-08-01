using Content.Shared._CP14.StatusEffect;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.StatusEffect;

public sealed partial class CP14ProgressiveStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<CP14ProgressiveStatusEffectComponent, StatusEffectComponent>();

        while (query.MoveNext(out var uid, out var progressive, out var effect))
        {
            if (_timing.CurTime < progressive.NextUpdateTime)
                continue;

            var meta = MetaData(uid).EntityPrototype;
            if (effect.AppliedTo is null || meta is null)
            {
                QueueDel(uid);
                continue;
            }

            progressive.NextUpdateTime = _timing.CurTime + TimeSpan.FromSeconds(1f);

            progressive.WoundHealth = Math.Clamp(progressive.WoundHealth + progressive.WoundHealthRegen, 0, progressive.WoundMaxHealth);
            DirtyField(uid, progressive, nameof(CP14ProgressiveStatusEffectComponent.WoundHealth));

            if (progressive.WoundHealth >= 0)
            {
                if (progressive.Complications.Count > 0)
                {
                    var nextEffect = _random.Pick(progressive.Complications);
                    _statusEffects.TrySetStatusEffectDuration(effect.AppliedTo.Value, nextEffect);
                }
                _statusEffects.TryRemoveStatusEffect(effect.AppliedTo.Value, meta);
            }

            if (progressive.WoundHealth <= progressive.WoundMaxHealth)
            {
                if (progressive.Restorations.Count > 0)
                {
                    var nextEffect = _random.Pick(progressive.Restorations);
                    _statusEffects.TrySetStatusEffectDuration(effect.AppliedTo.Value, nextEffect);
                    _statusEffects.TryRemoveStatusEffect(effect.AppliedTo.Value, meta);
                }
            }
        }
    }
}
