using Content.Shared._CP14.MagicVision;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._CP14.MagicVision;

public sealed class CP14MagicVisionOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    private readonly ShaderInstance _drowsinessShader;

    public float CurrentPower = 10.0f;
    public TimeSpan StartOverlay = TimeSpan.Zero; // when the overlay started

    private const float PowerDivisor = 250.0f;
    private const float Intensity = 0.2f; // for adjusting the visual scale
    private float _visualScale = 0; // between 0 and 1

    public CP14MagicVisionOverlay()
    {
        IoCManager.InjectDependencies(this);
        _drowsinessShader = _prototypeManager.Index<ShaderPrototype>("Drowsiness").InstanceUnique();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        var playerEntity = _playerManager.LocalEntity;

        if (playerEntity == null)
            return;

        if (!_entityManager.HasComponent<CP14MagicVisionComponent>(playerEntity))
            return;

        var curTime = _timing.CurTime;
        var timeLeft = (float)(curTime - StartOverlay).TotalSeconds;

        CurrentPower = Math.Max(50f, 200f - (150f * Math.Min((float)(timeLeft / 3.0), 1.0f)));
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_entityManager.TryGetComponent(_playerManager.LocalEntity, out EyeComponent? eyeComp))
            return false;

        if (args.Viewport.Eye != eyeComp.Eye)
            return false;

        _visualScale = Math.Clamp(CurrentPower / PowerDivisor, 0.0f, 1.0f);
        return _visualScale > 0;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        _drowsinessShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _drowsinessShader.SetParameter("Strength", _visualScale * Intensity);
        handle.UseShader(_drowsinessShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
