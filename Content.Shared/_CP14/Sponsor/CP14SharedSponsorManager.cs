using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Sponsor;

public abstract class SharedSponsorManager : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager Proto = default!;

    public abstract bool UserHasFeature(NetUserId userId,
        ProtoId<CP14SponsorFeaturePrototype> feature,
        bool ifDisabledSponsorhip = true);

    public ProtoId<CP14SponsorRolePrototype>? GetLowestPriorityRole(float priority)
    {
        ProtoId<CP14SponsorRolePrototype>? lowestRole = null;
        float lowestPriority = float.MaxValue;

        foreach (var role in Proto.EnumeratePrototypes<CP14SponsorRolePrototype>())
        {
            if (!role.Examinable)
                continue;

            if (role.Priority >= priority && role.Priority < lowestPriority)
            {
                lowestPriority = role.Priority;
                lowestRole = role.ID;
            }
        }

        return lowestRole;
    }
}

[Serializable, NetSerializable]
public sealed class CP14SponsorRolesEvent : EntityEventArgs
{
    public HashSet<ProtoId<CP14SponsorRolePrototype>> Roles = new();
    public CP14SponsorRolesEvent( HashSet<ProtoId<CP14SponsorRolePrototype>> roles)
    {
        Roles = roles;
    }
}
