using Content.Shared.IdentityManagement.Components;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.IdentityRecognition;

public abstract class CP14SharedIdentityRecognitionSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14UnknownIdentityComponent, SeeIdentityAttemptEvent>(OnSeeIdentityAttempt);
        SubscribeLocalEvent<CP14KnownNamesComponent, CP14RememberedNameChangedMessage>(OnRememberedNameChanged);
        SubscribeLocalEvent<CP14UnknownIdentityComponent, GetVerbsEvent<Verb>>(OnUnknownIdentityVerb);
    }

    private void OnUnknownIdentityVerb(Entity<CP14UnknownIdentityComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!TryComp<CP14KnownNamesComponent>(args.User, out var knownNames))
            return;

        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        if (args.User == ent.Owner)
            return;

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

    private void OnRememberedNameChanged(Entity<CP14KnownNamesComponent> ent, ref CP14RememberedNameChangedMessage args)
    {
        ent.Comp.Names[args.Target.Id] = args.Name;
        Dirty(ent);
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
