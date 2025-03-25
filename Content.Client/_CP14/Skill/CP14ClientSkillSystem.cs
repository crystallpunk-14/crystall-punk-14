using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Robust.Client.Player;

namespace Content.Client._CP14.Skill;

public sealed partial class CP14ClientSkillSystem : CP14SharedSkillSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public event Action<EntityUid>? OnSkillUpdate;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14SkillStorageComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
    }

    private void OnAfterAutoHandleState(Entity<CP14SkillStorageComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        OnSkillUpdate?.Invoke(ent.Owner);
    }

    public void RequestSkillData()
    {
        var localPlayer = _playerManager.LocalEntity;

        if (!TryComp<CP14SkillStorageComponent>(localPlayer, out var component))
            return;

        OnSkillUpdate?.Invoke(localPlayer.Value);
    }

    public void RequestLearnSkill(EntityUid? target, CP14SkillPrototype? skill)
    {
        if (skill == null || target == null)
            return;

        var netEv = new CP14TryLearnSkillMessage(GetNetEntity(target.Value), skill.ID);
        RaiseNetworkEvent(netEv);
    }
}
