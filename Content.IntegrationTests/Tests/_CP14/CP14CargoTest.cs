using System.Collections.Generic;
using Content.Shared._CP14.Trading.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._CP14;

#nullable enable

[TestFixture]
public sealed class CP14CargoTest
{
    [Test]
    public async Task CheckAllBuyPositionsUniqueCode()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var protoManager = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                HashSet<string> existedCodes = new();

                foreach (var proto in protoManager.EnumeratePrototypes<CP14TradingPositionPrototype>())
                {

                }
            });
        });
        await pair.CleanReturnAsync();
    }
}
