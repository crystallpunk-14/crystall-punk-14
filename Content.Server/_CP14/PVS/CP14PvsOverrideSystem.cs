using Robust.Server.GameStates;

namespace Content.Server._CP14.PVS;

public sealed partial class CP14PvsOverrideSystem : EntitySystem
{
    [Dependency] private readonly PvsOverrideSystem _pvs = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<CP14PvsOverrideComponent, ComponentStartup>(OnLighthouseStartup);
        SubscribeLocalEvent<CP14PvsOverrideComponent, ComponentShutdown>(OnLighthouseShutdown);
    }

    private void OnLighthouseShutdown(Entity<CP14PvsOverrideComponent> ent, ref ComponentShutdown args)
    {
        _pvs.RemoveGlobalOverride(ent);
    }

    private void OnLighthouseStartup(Entity<CP14PvsOverrideComponent> ent, ref ComponentStartup args)
    {
        _pvs.AddGlobalOverride(ent);
    }
}
