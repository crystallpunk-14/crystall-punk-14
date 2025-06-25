using Content.Shared._CP14.AuraDNA;
using Content.Shared._CP14.MagicVision;
using Content.Shared.Mobs;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._CP14.AuraImprint;

/// <summary>
/// This system handles the basic mechanics of spell use, such as doAfter, event invocation, and energy spending.
/// </summary>
public sealed partial class CP14AuraImprintSystem : CP14SharedAuraImprintSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly CP14SharedMagicVisionSystem _vision = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14AuraImprintComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14AuraImprintComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMapInit(Entity<CP14AuraImprintComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Imprint = GenerateAuraImprint(ent);
        Dirty(ent);
    }

    public string GenerateAuraImprint(Entity<CP14AuraImprintComponent> ent)
    {
        var letters = new[] { "ä", "ã", "ç", "ø", "ђ", "œ", "Ї", "Ћ", "ў", "ž", "Ћ", "ö", "є", "þ"};
        var imprint = string.Empty;

        for (var i = 0; i < ent.Comp.ImprintLength; i++)
        {
            imprint += letters[_random.Next(letters.Length)];
        }

        return $"[color={ent.Comp.ImprintColor.ToHex()}]{imprint}[/color]";
    }

    private void OnMobStateChanged(Entity<CP14AuraImprintComponent> ent, ref MobStateChangedEvent args)
    {
        switch (args.NewMobState)
        {
            case MobState.Critical:
            {
                _vision.SpawnMagicVision(
                    Transform(ent).Coordinates,
                    new SpriteSpecifier.Rsi(new ResPath("_CP14/Actions/Spells/misc.rsi"), "skull"),
                    Loc.GetString("cp14-magic-vision-crit"),
                    TimeSpan.FromMinutes(10),
                    ent);
                break;
            }
            case MobState.Dead:
            {
                _vision.SpawnMagicVision(
                    Transform(ent).Coordinates,
                    new SpriteSpecifier.Rsi(new ResPath("_CP14/Actions/Spells/misc.rsi"), "skull_red"),
                    Loc.GetString("cp14-magic-vision-dead"),
                    TimeSpan.FromMinutes(10),
                    ent);
                break;
            }
        }
    }
}
