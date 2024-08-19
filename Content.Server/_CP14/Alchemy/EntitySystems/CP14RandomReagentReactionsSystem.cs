using Content.Server.GameTicking.Events;
using Content.Shared.Chemistry.Reaction;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Alchemy.EntitySystems;

public sealed class CP14RandomReagentReactionsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        foreach (var reaction in _proto.EnumeratePrototypes<ReactionPrototype>())
        {
            reaction.Cp14RandomProductIndex = _random.Next(reaction.Cp14RandomProducts.Count);
        }
    }
}
