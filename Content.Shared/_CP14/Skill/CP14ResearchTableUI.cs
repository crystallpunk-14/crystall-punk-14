using Content.Shared._CP14.Skill.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Skill;

[Serializable, NetSerializable]
public enum CP14ResearchTableUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14ResearchMessage(ProtoId<CP14SkillPrototype> skill) : BoundUserInterfaceMessage
{
    public readonly ProtoId<CP14SkillPrototype> Skill = skill;
}


[Serializable, NetSerializable]
public sealed class CP14ResearchTableUiState(List<CP14ResearchUiEntry> skills) : BoundUserInterfaceState
{
    public readonly List<CP14ResearchUiEntry> Skills = skills;
}

[Serializable, NetSerializable]
public readonly struct CP14ResearchUiEntry(ProtoId<CP14SkillPrototype> protoId, bool craftable) : IEquatable<CP14ResearchUiEntry>
{
    public readonly ProtoId<CP14SkillPrototype> ProtoId = protoId;
    public readonly bool Craftable = craftable;

    public int CompareTo(CP14ResearchUiEntry other)
    {
        return Craftable.CompareTo(other.Craftable);
    }

    public override bool Equals(object? obj)
    {
        return obj is CP14ResearchUiEntry other && Equals(other);
    }

    public bool Equals(CP14ResearchUiEntry other)
    {
        return ProtoId.Id == other.ProtoId.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ProtoId, Craftable);
    }

    public override string ToString()
    {
        return $"{ProtoId} ({Craftable})";
    }

    public static int CompareTo(CP14ResearchUiEntry left, CP14ResearchUiEntry right)
    {
        return right.CompareTo(left);
    }
}
