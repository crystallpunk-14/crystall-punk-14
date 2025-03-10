using Content.Shared.Examine;
using Content.Shared.Humanoid;

namespace Content.Shared._CP14.Vampire;

public abstract class CP14SharedVampireVisualsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14VampireVisualsComponent, ExaminedEvent>(OnVampireExamine);

        SubscribeLocalEvent<CP14VampireVisualsComponent, ComponentInit>(OnVampireVisualsInit);
        SubscribeLocalEvent<CP14VampireVisualsComponent, ComponentShutdown>(OnVampireVisualsShutdown);
    }

    protected virtual void OnVampireVisualsShutdown(Entity<CP14VampireVisualsComponent> vampire, ref ComponentShutdown args)
    {
        if (!EntityManager.TryGetComponent(vampire, out HumanoidAppearanceComponent? humanoidAppearance))
            return;

        humanoidAppearance.EyeColor = vampire.Comp.OriginalEyesColor;

        Dirty(vampire, humanoidAppearance);
    }

    protected virtual void OnVampireVisualsInit(Entity<CP14VampireVisualsComponent> vampire, ref ComponentInit args)
    {
        if (!EntityManager.TryGetComponent(vampire, out HumanoidAppearanceComponent? humanoidAppearance))
            return;

        vampire.Comp.OriginalEyesColor = humanoidAppearance.EyeColor;
        humanoidAppearance.EyeColor = vampire.Comp.EyesColor;

        Dirty(vampire, humanoidAppearance);
    }

    private void OnVampireExamine(Entity<CP14VampireVisualsComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("cp14-vampire-examine"));
    }
}
