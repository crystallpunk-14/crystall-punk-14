using System.Diagnostics.CodeAnalysis;
using Content.Shared._CP14.Sponsor;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Roles;

/// <summary>
/// Requires a character to have, or not have, certain traits
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CP14SponsorFeatureRequired : JobRequirement
{
    [DataField(required: true)]
    public ProtoId<CP14SponsorFeaturePrototype> Feature = string.Empty;

    public override bool Check(NetUserId? userId,
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        if (userId is null)
            return false;

        var sponsorship = IoCManager.Resolve<ICP14SponsorManager>();

        var haveFeature = sponsorship.UserHasFeature(userId.Value, Feature);

        if (haveFeature)
            return true;

        var prototypeMan = IoCManager.Resolve<IPrototypeManager>();

        var indexedFeature = prototypeMan.Index(Feature);
        var lowestRole = GetLowestPriorityRole(indexedFeature.MinPriority, prototypeMan);
        prototypeMan.TryIndex(lowestRole, out var indexedRole);

        if (indexedRole == null)
            return false;
        reason = FormattedMessage.FromMarkupPermissive(Loc.GetString("cp14-role-req-sponsor-feature-req", ("role", indexedRole.Name)));

        return false;
    }

    public ProtoId<CP14SponsorRolePrototype>? GetLowestPriorityRole(float priority, IPrototypeManager protoMan)
    {
        ProtoId<CP14SponsorRolePrototype>? lowestRole = null;
        float lowestPriority = float.MaxValue;

        foreach (var role in protoMan.EnumeratePrototypes<CP14SponsorRolePrototype>())
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
