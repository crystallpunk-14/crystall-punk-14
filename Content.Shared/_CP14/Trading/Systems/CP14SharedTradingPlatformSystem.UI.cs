using Content.Shared._CP14.Trading.Components;
using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared.UserInterface;

namespace Content.Shared._CP14.Trading.Systems;

public abstract partial class CP14SharedTradingPlatformSystem
{
    private void InitializeUI()
    {
        SubscribeLocalEvent<CP14TradingPlatformComponent, BeforeActivatableUIOpenEvent>(OnBeforeTradingUIOpen);
    }

    private void OnBeforeTradingUIOpen(Entity<CP14TradingPlatformComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateTradingUIState(ent, args.User);
    }

    protected void UpdateTradingUIState(Entity<CP14TradingPlatformComponent> ent, EntityUid user)
    {
        _userInterface.SetUiState(ent.Owner, CP14TradingUiKey.Buy, new CP14TradingPlatformUiState(GetNetEntity(ent)));
    }

    public string GetTradeDescription(CP14TradingPositionPrototype position)
    {
        if (position.Desc != null)
            return Loc.GetString(position.Desc);

        if (position.Service is null)
            return string.Empty;

        return position.Service.GetDesc(Proto);
    }

    public string GetTradeName(CP14TradingPositionPrototype position)
    {
        if (position.Name != null)
            return Loc.GetString(position.Name);

        if (position.Service is null)
            return string.Empty;

        return position.Service.GetName(Proto);
    }
}
