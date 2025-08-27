using Content.Shared.Ghost;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.IdentityRecognition;

public abstract class CP14SharedIdentityRecognitionSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14UnknownIdentityComponent, SeeIdentityAttemptEvent>(OnSeeIdentityAttempt);
        SubscribeLocalEvent<CP14UnknownIdentityComponent, GetVerbsEvent<Verb>>(OnUnknownIdentityVerb);

        SubscribeLocalEvent<MindContainerComponent, CP14RememberedNameChangedMessage>(OnRememberedNameChanged);
    }

    private void OnUnknownIdentityVerb(Entity<CP14UnknownIdentityComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (HasComp<GhostComponent>(args.User))
            return;

        if(!_mind.TryGetMind(args.User, out var mindId, out var mind))
            return;

        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        if (args.User == ent.Owner)
            return;

        EnsureComp<CP14RememberedNamesComponent>(mindId);

        var _args = args;
        var verb = new Verb
        {
            Priority = 2,
            Icon =  new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/sentient.svg.192dpi.png")),
            Text = Loc.GetString("cp14-remember-name-verb"),
            Act = () =>
            {
                _uiSystem.SetUiState(_args.User, CP14RememberNameUiKey.Key, new CP14RememberNameUiState(GetNetEntity(ent)));
                _uiSystem.TryToggleUi(_args.User, CP14RememberNameUiKey.Key, actor.PlayerSession);
            },
        };
        args.Verbs.Add(verb);
    }

    private void OnRememberedNameChanged(Entity<MindContainerComponent> ent, ref CP14RememberedNameChangedMessage args)
    {
        var mindEntity = ent.Comp.Mind;

        if (mindEntity is null)
            return;

        var knownNames = EnsureComp<CP14RememberedNamesComponent>(mindEntity.Value);

        knownNames.Names[args.Target.Id] = args.Name;
        Dirty(mindEntity.Value, knownNames);
    }

    private void OnSeeIdentityAttempt(Entity<CP14UnknownIdentityComponent> ent, ref SeeIdentityAttemptEvent args)
    {
        args.Cancel();
    }
}

[Serializable, NetSerializable]
public sealed class CP14RememberedNameChangedMessage(string name, NetEntity target) : BoundUserInterfaceMessage
{
    public string Name { get; } = name;
    public NetEntity Target { get; } = target;
}

[Serializable, NetSerializable]
public enum CP14RememberNameUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14RememberNameUiState(NetEntity target) : BoundUserInterfaceState
{
    public NetEntity Target = target;
}
