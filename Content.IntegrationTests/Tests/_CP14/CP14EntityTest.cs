using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
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

                    Assert.That(proto.Categories.Contains(indexedFilter), $"CP14 fork proto: {proto} does not marked abstract, or have a HideSpawnMenu or ForkFiltered category");
                }
            });
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task CheckAllCP14ForkFilteredEntitiesUseForkDamageModifierSet()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var protoManager = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                if (!protoManager.TryIndex<EntityCategoryPrototype>("ForkFiltered", out var indexedFilter))
                    return;

                foreach (var proto in protoManager.EnumeratePrototypes<EntityPrototype>())
                {
                    // Skip non-CP14 entities
                    if (!proto.ID.StartsWith("CP14"))
                        continue;

                    // Skip entities that don't have ForkFiltered category
                    if (!proto.Categories.Contains(indexedFilter))
                        continue;

                    // Check if entity has DamageableComponent
                    if (!proto.TryGetComponent<DamageableComponent>("Damageable", out var damageable))
                        continue;
                    
                    // Check if it has a damage modifier set
                    if (damageable.DamageModifierSetId == null)
                        continue;

                    // Validate that the damage modifier set ID starts with "CP14"
                    Assert.That(damageable.DamageModifierSetId.Value.StartsWith("CP14"), 
                        $"CP14 fork entity '{proto.ID}' with ForkFiltered category and DamageableComponent uses damage modifier set '{damageable.DamageModifierSetId}' that doesn't start with 'CP14' prefix. All CP14 entities must use CP14-specific damage modifiers.");
                }
            });
        });
        await pair.CleanReturnAsync();
    }
}
