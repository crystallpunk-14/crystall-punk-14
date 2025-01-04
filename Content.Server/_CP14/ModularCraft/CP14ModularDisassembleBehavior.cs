using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Behaviors;

namespace Content.Server._CP14.ModularCraft;

[Serializable]
[DataDefinition]
public sealed partial class CP14ModularDisassembleBehavior : IThresholdBehavior
{
    public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
    {
        var modular = system.EntityManager.System<CP14ModularCraftSystem>();
        modular.DisassembleModular(owner);
    }
}
