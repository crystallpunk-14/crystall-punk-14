using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.Skill;

public sealed partial class CP14ClientSkillSystem : CP14SharedSkillSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

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

        if (!HasComp<CP14SkillStorageComponent>(localPlayer))
            return;

        OnSkillUpdate?.Invoke(localPlayer.Value);
    }

    public void RequestLearnSkill(EntityUid? target, CP14SkillPrototype? skill)
    {
        if (skill == null || target == null)
            return;

        var netEv = new CP14TryLearnSkillMessage(GetNetEntity(target.Value), skill.ID);
        RaiseNetworkEvent(netEv);

        if (_proto.TryIndex(skill.Tree, out var indexedTree))
        {
            _audio.PlayGlobal(indexedTree.LearnSound, target.Value, AudioParams.Default.WithVolume(6f));
        }
    }
}
