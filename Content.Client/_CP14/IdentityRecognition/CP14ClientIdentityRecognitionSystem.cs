using Content.Shared._CP14.IdentityRecognition;
using Content.Shared.IdentityManagement;

namespace Content.Client._CP14.IdentityRecognition;

public sealed partial class CP14ClientIdentityRecognitionSystem : CP14SharedIdentityRecognitionSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14KnownNamesComponent, CP14ClientTransformNameEvent>(OnTransformSpeakerName);
    }

    private void OnTransformSpeakerName(Entity<CP14KnownNamesComponent> ent, ref CP14ClientTransformNameEvent args)
    {
        if (args.Handled)
            return;

        var speaker = GetEntity(args.Speaker);

        if (speaker == ent.Owner)
            return;

        if (ent.Comp.Names.TryGetValue(args.Speaker.Id, out var name))
        {
            args.Name = name;
        }
        else
        {
            args.Name = Identity.Name(speaker, EntityManager, ent);
        }

        args.Handled = true;
    }
}

public sealed class CP14ClientTransformNameEvent(NetEntity speaker) : EntityEventArgs
{
    public NetEntity Speaker = speaker;

    public string Name = string.Empty;

    public bool Handled { get; set; }
}
