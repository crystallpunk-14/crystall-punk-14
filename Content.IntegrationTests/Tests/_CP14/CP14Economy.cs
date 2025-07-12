using Content.Server.Cargo.Systems;
using Content.Shared._CP14.Workbench.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._CP14;

#nullable enable

[TestFixture]
public sealed class CP14Economy
{
    /// <summary>
    /// Checks that StaticPrice is not used together with PhysicalComposition to evaluate an item.
    /// </summary>
    [Test]
    public async Task CheckStaticPriceUnique()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var protoMan = server.ResolveDependency<IPrototypeManager>();


        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                foreach (var proto in protoMan.EnumeratePrototypes<EntityPrototype>())
                {
                    //We dont check vanilla entities
                    if (!proto.ID.StartsWith("CP14"))
                        continue;

                    if (!proto.Components.ContainsKey("StaticPrice"))
                        continue;

                    if (proto.Components.ContainsKey("PhysicalComposition"))
                    {
                        Assert.Fail($"The prototype [{proto.ID}] contains both StaticPrice and PhysicalComposition components. Only one method of evaluating the entity should be used.");
                    }
                }
            });
        });
    }

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

                    resourcePrice = Math.Round(resourcePrice, 3);
                    resultPrice = Math.Round(resultPrice, 3);

                    if (resourcePrice == 0 && resultPrice == 0)
                        continue;

                    var minLimit = resourcePrice;
                    var maxLimit = resourcePrice * 2;
                    if (resultPrice < minLimit)
                    {
                        Assert.Fail($"The ingredients to craft the [{recipe.ID}] cost more than the result of the crafting. \nCrafting: [{recipe.Result.Id} x{recipe.ResultCount}]. \nExpected price range: from {minLimit} to {maxLimit}. \nCurrent price: {resultPrice}");
                    }

                    if (resultPrice > maxLimit)
                    {
                        Assert.Fail($"The result of crafting [{recipe.ID}] is too expensive! After crafting, the final cost exceeds the cost of all resources more than 2 times! \nCrafting: [{recipe.Result.Id} x{recipe.ResultCount}]. \nExpected price range: from {minLimit} to {maxLimit}. \nCurrent price: {resultPrice}");
                    }
                    entManager.DeleteEntity(result);
                }
            });
        });

        await pair.CleanReturnAsync();
    }
}
