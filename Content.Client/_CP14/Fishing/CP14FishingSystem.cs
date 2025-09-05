using System.Diagnostics;
using System.Numerics;
using Content.Shared._CP14.Fishing;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared.Interaction;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.Fishing;

public sealed class CP14FishingSystem : CP14SharedFishingSystem
{
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private Popup? _fishingPopup;
    private const int FishingMinigameSizeX = 41;
    private const int FishingMinigameSizeY = 149;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FishingRodComponent, AfterInteractEvent>(OnInteract);
    }

    private void OnInteract(EntityUid uid, CP14FishingRodComponent component, AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!args.CanReach || args.Target is not { Valid: true })
            return;

        if (!TryComp<CP14FishingPondComponent>(args.Target, out var pond))
            return;

        if (component.FishingProcess)
            return;

        OpenFishingPopup(component);
    }

    private void OpenFishingPopup(CP14FishingRodComponent component)
    {
        _fishingPopup = new Popup
        {
            CloseOnClick = false,
            CloseOnEscape = false,
            MinSize = new Vector2(FishingMinigameSizeX, FishingMinigameSizeY),
            MaxSize = new Vector2(FishingMinigameSizeX, FishingMinigameSizeY),
        };
        _userInterfaceManager.ModalRoot.AddChild(_fishingPopup);

        var backgroundTexture = RequestTexture(CP14FishingMinigameElement.Background, component);
        if (backgroundTexture is null)
            return;

        var panel = new PanelContainer
        {
            PanelOverride = new StyleBoxTexture
            {
                Texture = backgroundTexture,
            },
        };
    }

    private TextureResource? RequestTexture(CP14FishingMinigameElement element, CP14FishingRodComponent fishingRodComponent)
    {
        var minigamePrototype = _prototypeManager.Index(fishingRodComponent.FishingMinigame);

        if (minigamePrototype.Texture is null || !minigamePrototype.Texture.TryGetValue(element, out var data))
        {
            Debug.Fail($"Tried to request a fishing minigame element {element} without a texture.");
            return null;
        }

        var texture = _resourceCache.GetResource<TextureResource>(data.Texture);
        return texture;
    }
}
