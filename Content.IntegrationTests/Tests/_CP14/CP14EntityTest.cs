using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._CP14;

#nullable enable

[TestFixture]
public sealed class CP14EntityTest
{
    [Test]
    public async Task CheckAllCP14EntityHasForkFilteredCategory()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var compFactory = server.ResolveDependency<IComponentFactory>();
        var protoManager = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                if (!protoManager.TryIndex<EntityCategoryPrototype>("ForkFiltered", out var indexedFilter))
                    return;

                foreach (var proto in protoManager.EnumeratePrototypes<EntityPrototype>())
                {
                    if (!proto.ID.StartsWith("CP14"))
                        continue;

                    if (proto.Abstract || proto.HideSpawnMenu)
                        continue;

                    Assert.That(proto.Categories.Contains(indexedFilter), $"{proto} does not have a ForkFiltered category");
                }
            });
        });
        await pair.CleanReturnAsync();
    }
}
