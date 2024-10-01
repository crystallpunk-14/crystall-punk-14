namespace Content.Shared._CP14.MagicRitual;

public partial class CP14SharedRitualSystem : EntitySystem
{
    public void ChangeRitualStability(Entity<CP14MagicRitualComponent> ritual, float dStab)
    {
        var newS = MathHelper.Clamp01(ritual.Comp.Stability + dStab);

        var ev = new CP14RitualStabilityChangedEvent(ritual.Comp.Stability, newS);
        RaiseLocalEvent(ritual, ev);

        ritual.Comp.Stability = newS;
    }
}
