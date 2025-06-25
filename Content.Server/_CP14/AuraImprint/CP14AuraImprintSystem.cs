using Content.Shared._CP14.AuraDNA;
using Robust.Shared.Random;

namespace Content.Server._CP14.AuraImprint;

/// <summary>
/// This system handles the basic mechanics of spell use, such as doAfter, event invocation, and energy spending.
/// </summary>
public sealed partial class CP14AuraImprintSystem : CP14SharedAuraImprintSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14AuraImprintComponent, MapInitEvent>(OnMapInit);
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
}
