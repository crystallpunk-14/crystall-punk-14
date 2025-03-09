using Content.Shared.CCVar;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Client._CP14.Wave;

public sealed class CP14WaveShaderSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private ShaderInstance _shader = default!;
    private bool _enabled;

    public override void Initialize()
    {
        base.Initialize();

        _shader = _protoMan.Index<ShaderPrototype>("Wave").InstanceUnique();
        _enabled = _configuration.GetCVar(CCVars.WaveShaderEnabled);

        SubscribeLocalEvent<CP14WaveShaderComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CP14WaveShaderComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CP14WaveShaderComponent, BeforePostShaderRenderEvent>(OnBeforeShaderPost);
    }

    private void OnStartup(Entity<CP14WaveShaderComponent> entity, ref ComponentStartup args)
    {
        entity.Comp.Offset = _random.NextFloat(0, 1000);
        SetShader(entity.Owner, _shader);
    }

    private void OnShutdown(Entity<CP14WaveShaderComponent> entity, ref ComponentShutdown args)
    {
        SetShader(entity.Owner, null);
    }

    private void SetShader(Entity<SpriteComponent?> entity, ShaderInstance? instance)
    {
        if (!Resolve(entity, ref entity.Comp, false) || !_enabled)
            return;

        entity.Comp.PostShader = instance;
        entity.Comp.GetScreenTexture = instance is not null;
        entity.Comp.RaiseShaderEvent = instance is not null;
    }

    private void OnBeforeShaderPost(Entity<CP14WaveShaderComponent> entity, ref BeforePostShaderRenderEvent args)
    {
        if (!_enabled)
            return;

        _shader.SetParameter("Speed", entity.Comp.Speed);
        _shader.SetParameter("Dis", entity.Comp.Dis);
        _shader.SetParameter("Offset", entity.Comp.Offset);
    }
}
