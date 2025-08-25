using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Action;

public abstract partial class CP14ActionSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeAttempts();
        InitializeExamine();
    }
}
