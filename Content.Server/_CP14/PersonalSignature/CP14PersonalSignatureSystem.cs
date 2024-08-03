using System.Diagnostics.CodeAnalysis;
using Content.Server.Mind;
using Content.Server.Paper;
using Content.Shared.Hands.Components;
using Content.Shared.Paper;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server._CP14.PersonalSignature;

public sealed class CP14PersonalSignatureSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
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

        if (!CanSign(args.Using, out var signature))
            return;

        if (HasSign(entity, mind.CharacterName))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("cp-sign-verb"),
            Act = () =>
            {
                Sign(entity, mind.CharacterName, signature.SignSound);
            },
        });
    }

    private bool CanSign(EntityUid? item, [NotNullWhen(true)] out CP14PersonalSignatureComponent? personalSignature)
    {
        personalSignature = null;
        return item is not null && TryComp(item, out personalSignature);
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

    private void Sign(Entity<PaperComponent> target, string name, SoundSpecifier? sound)
    {
        var info = new StampDisplayInfo
        {
            StampedName = name,
            StampedColor = Color.Gray,
        };

        if (sound is not null)
            _audio.PlayEntity(sound, Filter.Pvs(target), target, true);

        target.Comp.StampedBy.Add(info);
    }
}
