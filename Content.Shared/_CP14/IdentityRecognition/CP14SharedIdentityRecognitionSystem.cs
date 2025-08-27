using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.IdentityManagement;
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
    [Dependency] private readonly SharedIdentitySystem _identity = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14UnknownIdentityComponent, GetVerbsEvent<Verb>>(OnUnknownIdentityVerb);
        SubscribeLocalEvent<CP14UnknownIdentityComponent, ExaminedEvent>(OnExaminedEvent);

        SubscribeLocalEvent<MindContainerComponent, CP14RememberedNameChangedMessage>(OnRememberedNameChanged);

        SubscribeLocalEvent<CP14RememberedNamesComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14RememberedNamesComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<MindComponent>(ent, out var mind))
            return;

        if (mind.OwnedEntity is null)
            return;

        if (mind.CharacterName is null)
            return;

        RememberCharacter(ent, GetNetEntity(mind.OwnedEntity.Value), mind.CharacterName);
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

        var seeAttemptEv = new SeeIdentityAttemptEvent();
        RaiseLocalEvent(ent.Owner, seeAttemptEv);

        var _args = args;
        var verb = new Verb
        {
            Priority = 2,
            Icon =  new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/sentient.svg.192dpi.png")),
            Text = Loc.GetString("cp14-remember-name-verb"),
            Disabled = seeAttemptEv.Cancelled,
            Act = () =>
            {
                _uiSystem.SetUiState(_args.User, CP14RememberNameUiKey.Key, new CP14RememberNameUiState(GetNetEntity(ent)));
                _uiSystem.TryToggleUi(_args.User, CP14RememberNameUiKey.Key, actor.PlayerSession);
            },
        };
        args.Verbs.Add(verb);
    }

    private void OnExaminedEvent(Entity<CP14UnknownIdentityComponent> ent, ref ExaminedEvent args)
    {
        var ev = new SeeIdentityAttemptEvent();
        RaiseLocalEvent(ent.Owner, ev);

        if (ev.Cancelled)
            return;

        if (!_mind.TryGetMind(args.Examiner, out var mindId, out var mind))
            return;

        if (!TryComp<CP14RememberedNamesComponent>(mindId, out var knownNames))
            return;

        if (knownNames.Names.TryGetValue(GetNetEntity(ent).Id, out var name))
        {
            args.PushMarkup(Loc.GetString("cp14-remember-name-examine", ("name", name)), priority: -1);
        }
    }

    private void OnRememberedNameChanged(Entity<MindContainerComponent> ent, ref CP14RememberedNameChangedMessage args)
    {
        var mindEntity = ent.Comp.Mind;

        if (mindEntity is null)
            return;

        RememberCharacter(mindEntity.Value, args.Target, args.Name);
    }

    private void RememberCharacter(EntityUid mindEntity, NetEntity targetId, string name)
    {
        var knownNames = EnsureComp<CP14RememberedNamesComponent>(mindEntity);

        knownNames.Names[targetId.Id] = name;
        Dirty(mindEntity, knownNames);
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
