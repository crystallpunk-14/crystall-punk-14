using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.Skill;

public sealed partial class CP14ClientSkillSystem : CP14SharedSkillSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public event Action<SkillMenuData>? OnSkillUpdate;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14SkillStorageComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
    }

    private void OnAfterAutoHandleState(Entity<CP14SkillStorageComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        OnSkillUpdate?.Invoke(
            new SkillMenuData(
                ent.Owner,
                ent.Comp.Skills
            )
        );
    }

    public void RequestSkillData()
    {
        var localPlayer = _playerManager.LocalEntity;

        if (!TryComp<CP14SkillStorageComponent>(localPlayer, out var component))
            return;

        OnSkillUpdate?.Invoke(
            new SkillMenuData(
                localPlayer.Value,
                component.Skills
            )
        );
    }
}

public readonly record struct SkillMenuData(
    EntityUid Entity,
    List<ProtoId<CP14SkillPrototype>> Skills
);
