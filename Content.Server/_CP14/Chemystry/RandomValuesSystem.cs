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

    private Dictionary<string, Dictionary<string, FixedPoint2>> _CP14RandomProducts = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
    }

    private void AddProductsList(ReactionPrototype reaction, Dictionary<string, FixedPoint2> products)
    {
        foreach (var product in products)
        {
            if (reaction.Products.ContainsKey(product.Key))
            {
                reaction.Products[product.Key] += product.Value;
            }
            else
            {
                reaction.Products[product.Key] = product.Value;
            }
        }
    }

    private void RemoveProductsList(ReactionPrototype reaction, Dictionary<string, FixedPoint2> products)
    {
        foreach (var product in products)
        {
            reaction.Products[product.Key] -= product.Value;
            if (reaction.Products[product.Key] == 0)
            {
                reaction.Products.Remove(product.Key);
            }
        }
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        foreach (var reaction in _prototypeManager.EnumeratePrototypes<ReactionPrototype>())
        {
            if (reaction.Cp14RandomProducts.Count == 0)
                continue;
            if (_CP14RandomProducts.ContainsKey(reaction.ID))
                RemoveProductsList(reaction, _CP14RandomProducts[reaction.ID]);
            _CP14RandomProducts[reaction.ID] = _random.Pick(reaction.Cp14RandomProducts);
            AddProductsList(reaction, _CP14RandomProducts[reaction.ID]);
        }
    }
}
