using Content.Shared.IdentityManagement.Components;

namespace Content.Shared._CP14.IdentityRecognition;

public abstract partial class CP14SharedIdentityRecognitionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14UnknownIdentityComponent, SeeIdentityAttemptEvent>(OnSeeIdentityAttempt);
    }

    private void OnSeeIdentityAttempt(Entity<CP14UnknownIdentityComponent> ent, ref SeeIdentityAttemptEvent args)
    {
        args.Cancel();
    }

    private void RememberName(EntityUid rememberer, EntityUid remembered, string name)
    {
        var knownNames = EnsureComp<CP14KnownNamesComponent>(rememberer);

        knownNames.Names[remembered] = name;
        Dirty(rememberer, knownNames);
    }
}
