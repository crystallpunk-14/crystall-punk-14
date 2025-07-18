using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Systems;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellSendMessageToGod : CP14SpellEffect
{
    [DataField]
    public LocId? Message;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (!entManager.TryGetComponent<CP14ReligionFollowerComponent>(args.User, out var follower))
            return;
        if (!entManager.TryGetComponent<MetaDataComponent>(args.User, out var metaData))
            return;

        if (follower.Religion is null)
            return;

        var religionSys = entManager.System<CP14SharedReligionGodSystem>();

        religionSys.SendMessageToGods(follower.Religion.Value, Loc.GetString("cp14-call-follower-message", ("name", metaData.EntityName)) + " " + Loc.GetString(Message?? ""), args.User.Value);
    }
}
