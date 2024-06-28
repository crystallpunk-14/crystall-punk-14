using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Client._CP14.Wave;

public sealed class CP14WaveShaderSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private ShaderInstance _shader = default!;

    public override void Initialize()
    {
        base.Initialize();

        _shader = _protoMan.Index<ShaderPrototype>("Wave").InstanceUnique();
        _shader.SetParameter("Offset", _random.NextFloat(0, 1000));

        SubscribeLocalEvent<CP14WaveShaderComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CP14WaveShaderComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<CP14WaveShaderComponent> entity, ref ComponentStartup args)
    {
        SetShader(entity.Owner, _shader);
    }

    private void OnShutdown(Entity<CP14WaveShaderComponent> entity, ref ComponentShutdown args)
    {
        SetShader(entity.Owner, null);
    }

    private void SetShader(Entity<SpriteComponent?> entity, ShaderInstance? instance)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        entity.Comp.PostShader = instance;
    }
}
