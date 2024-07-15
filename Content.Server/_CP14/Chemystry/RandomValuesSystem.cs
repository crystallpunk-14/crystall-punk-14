using Content.Server.GameTicking.Events;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.FixedPoint;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Chemystry;

public sealed class ChemicalReactionSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        foreach (var reaction in _prototypeManager.EnumeratePrototypes<ReactionPrototype>())
        {
            reaction.Cp14RandomProductIndex = _random.Next(reaction.Cp14RandomProducts.Count);
        }
    }
}
