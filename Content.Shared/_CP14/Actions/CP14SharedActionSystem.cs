using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Actions;

public abstract partial class CP14SharedActionSystem : EntitySystem
{
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeAttempts();
        InitializeExamine();
    }
}
