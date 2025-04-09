using Robust.Server.GameStates;

namespace Content.Server._CP14.Lighthouse;

public sealed partial class CP14LighthouseSystem : EntitySystem
{
    [Dependency] private readonly PvsOverrideSystem _pvs = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<CP14LighthouseComponent, ComponentStartup>(OnLighthouseStartup);
        SubscribeLocalEvent<CP14LighthouseComponent, ComponentShutdown>(OnLighthouseShutdown);
    }

    private void OnLighthouseShutdown(Entity<CP14LighthouseComponent> ent, ref ComponentShutdown args)
    {
        _pvs.RemoveGlobalOverride(ent);
    }

    private void OnLighthouseStartup(Entity<CP14LighthouseComponent> ent, ref ComponentStartup args)
    {
        _pvs.AddGlobalOverride(ent);
    }
}
