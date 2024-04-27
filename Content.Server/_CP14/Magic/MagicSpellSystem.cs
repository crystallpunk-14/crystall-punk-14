using System.Runtime.CompilerServices;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Magic;

public sealed class MagicSpellSystem : EntitySystem
{
    [Robust.Shared.IoC.Dependency] private readonly IPrototypeManager _prototype = default!;
    [Robust.Shared.IoC.Dependency] private readonly TransformSystem _transform = default!;

    public IReadOnlyDictionary<ProtoId<MagicSpellPrototype>, MagicSpellPrototype> Spells => _spells;

    private readonly Dictionary<ProtoId<MagicSpellPrototype>, MagicSpellPrototype> _spells = new();

    public override void Initialize()
    {
        base.Initialize();

        Enumerate();
        _prototype.PrototypesReloaded += OnProtoReload;
    }

    public void Cast(EntityUid caster, IReadOnlyList<ProtoId<MagicSpellPrototype>> spells)
    {
        var casterPosition = _transform.GetMapCoordinates(caster);

        var context = new MagicSpellContext
        {
            SourcePoint = casterPosition,
            TargetPoint = casterPosition
        };

        foreach (var spellProto in spells)
        {
            if (!Spells.TryGetValue(spellProto, out var spell))
                continue;

            spell.Action.Modify(context);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Enumerate()
    {
        var prototypes = _prototype.EnumeratePrototypes<MagicSpellPrototype>();
        foreach (var prototype in prototypes)
        {
            _spells.Add(prototype.ID, prototype);
        }
    }

    private void OnProtoReload(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<MagicSpellPrototype>())
            return;

        Enumerate();
    }
}
