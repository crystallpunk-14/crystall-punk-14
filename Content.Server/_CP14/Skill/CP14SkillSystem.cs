using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Components;

namespace Content.Server._CP14.Skill;

public sealed partial class CP14ClientSkillSystem : CP14SharedSkillSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<CP14TryLearnSkillMessage>(OnClientRequestLearnSkill);
    }

    private void OnClientRequestLearnSkill(CP14TryLearnSkillMessage ev, EntitySessionEventArgs args)
    {
        var entity = GetEntity(ev.Entity);

        if (args.SenderSession.AttachedEntity != entity)
            return;

        if (!TryLearnSkill(entity, ev.Skill))
            return;
    }
}
