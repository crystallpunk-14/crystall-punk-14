using Content.Server.DoAfter;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Magic;

public sealed partial class CPMagicSpellContainerSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public readonly EntProtoId BaseSpellItemEntity = "CPBSpellItemEntity";
    public readonly EntProtoId BaseSpellEffectEntity = "CPBaseSpellEntity";

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logManager.GetSawmill("cp14_magic_spell_container");

        InitializeHash();
        InitializeCast();
        InitializeSpell();
    }
}
