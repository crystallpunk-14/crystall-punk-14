using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared.Hands.Components;
using Content.Shared.Speech.Muting;

namespace Content.Shared._CP14.MagicSpell;

public partial class CP14SharedMagicSystem
{
    private void InitializeAspects()
    {
        SubscribeLocalEvent<CP14MagicEffectSomaticAspectComponent, CP14BeforeCastMagicEffectEvent>(OnSomaticAspectBeforeCast);

        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14BeforeCastMagicEffectEvent>(OnVerbalAspectBeforeCast);
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14StartCastMagicEffectEvent>(OnVerbalAspectStartCast);
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14AfterCastMagicEffectEvent>(OnVerbalAspectAfterCast);
    }

    private void OnSomaticAspectBeforeCast(Entity<CP14MagicEffectSomaticAspectComponent> ent, ref CP14BeforeCastMagicEffectEvent args)
    {
        if (TryComp<HandsComponent>(args.Performer, out var hands) || hands is not null)
        {
            var freeHand = 0;
            foreach (var hand in hands.Hands)
            {
                if (hand.Value.IsEmpty)
                    freeHand++;
            }
            if (freeHand >= ent.Comp.FreeHandRequired)
                return;
        }
        args.PushReason(Loc.GetString("cp14-magic-spell-need-somatic-component"));
        args.Cancel();
    }

    private void OnVerbalAspectBeforeCast(Entity<CP14MagicEffectVerbalAspectComponent> ent, ref CP14BeforeCastMagicEffectEvent args)
    {
        if (HasComp<MutedComponent>(args.Performer))
        {
            args.PushReason(Loc.GetString("cp14-magic-spell-need-verbal-component"));
            args.Cancel();
        }
    }

    private void OnVerbalAspectStartCast(Entity<CP14MagicEffectVerbalAspectComponent> ent, ref CP14StartCastMagicEffectEvent args)
    {
        var ev = new CP14VerbalAspectSpeechEvent
        {
            Performer = args.Caster,
            Speech = ent.Comp.StartSpeech,
        };
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnVerbalAspectAfterCast(Entity<CP14MagicEffectVerbalAspectComponent> ent, ref CP14AfterCastMagicEffectEvent args)
    {
        if (_net.IsClient)
            return;

        var ev = new CP14VerbalAspectSpeechEvent
        {
            Performer = args.Caster,
            Speech = ent.Comp.EndSpeech,
        };
        RaiseLocalEvent(ent, ref ev);
    }

}
