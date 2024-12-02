using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.LockKey;

public sealed partial class CP14KeyDistributionSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;


    public override void Initialize()
    {
        base.Initialize();

        
    }
}
