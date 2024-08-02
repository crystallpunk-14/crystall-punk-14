using Content.Server.Mind;
using Content.Server.Paper;
using Content.Shared.Hands.Components;
using Content.Shared.Paper;
using Content.Shared.Verbs;

namespace Content.Server._CP14.PersonalSignature;

public sealed class CP14PersonalSignatureSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaperComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerb);
    }

    private void OnGetVerb(Entity<PaperComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!_mind.TryGetMind(args.User, out _, out var mind))
            return;

        if (mind.CharacterName is null)
            return;

        if (!CanSign(args.Using))
            return;

        if (HasSign(entity, mind.CharacterName))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = "Sign",
            Act = () =>
            {
                Sign(entity, mind.CharacterName);
            },
        });
    }

    private bool CanSign(EntityUid? item)
    {
        if (item is null)
            return false;

        if (HasComp<CP14PersonalSignatureComponent>(item))
            return true;

        return false;
    }

    private bool HasSign(Entity<PaperComponent> entity, string sign)
    {
        foreach (var info in entity.Comp.StampedBy)
        {
            if (info.StampedName == sign)
                return true;
        }

        return false;
    }

    private void Sign(Entity<PaperComponent> target, string name)
    {
        var info = new StampDisplayInfo
        {
            StampedName = name,
            StampedColor = Color.Gray,
        };

        target.Comp.StampedBy.Add(info);
    }
}
