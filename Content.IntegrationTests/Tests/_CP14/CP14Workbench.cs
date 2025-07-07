using Content.Server.Cargo.Systems;
using Content.Shared._CP14.Workbench.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._CP14;

#nullable enable

[TestFixture]
public sealed class CP14Workbench
{
    /// <summary>
    /// Check that the price of all resources to craft the item on the workbench is lower than the price of the result.
    /// </summary>
    [Test]
    public async Task CheckRecipePricingReduction()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var entManager = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();

        var pricingSystem = entManager.System<PricingSystem>();

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                foreach (var recipe in protoMan.EnumeratePrototypes<CP14WorkbenchRecipePrototype>())
                {
                    double resourcePrice = 0;
                    foreach (var req in recipe.Requirements)
                    {
                        resourcePrice += req.GetPrice(entManager, protoMan);
                    }

                    var result = entManager.Spawn(recipe.Result);
                    var resultPrice = pricingSystem.GetPrice(result) * recipe.ResultCount;

                    Assert.That(resourcePrice <= resultPrice, $"The ingredients to craft the [{recipe.ID}] cost more than the result of the crafting. Crafting: [{recipe.Result.Id} x{recipe.ResultCount}]. Expected result price is {resourcePrice}+, but it is {resultPrice}.");
                    entManager.DeleteEntity(result);
                }
            });
        });

        await pair.CleanReturnAsync();
    }
}
