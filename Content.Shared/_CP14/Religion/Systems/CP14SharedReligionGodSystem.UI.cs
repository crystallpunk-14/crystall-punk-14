using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Religion.Systems;

[Serializable, NetSerializable]
public enum CP14ReligionEntityUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14ReligionEntityUiState(Dictionary<NetEntity, string> altars, Dictionary<NetEntity, string> followers) : BoundUserInterfaceState
{
    public Dictionary<NetEntity, string> Altars = altars;
    public Dictionary<NetEntity, string> Followers = followers;
}

[Serializable, NetSerializable]
public sealed class CP14ReligionEntityTeleportAttempt(NetEntity entity) : BoundUserInterfaceMessage
{
    public readonly NetEntity Entity = entity;
}
