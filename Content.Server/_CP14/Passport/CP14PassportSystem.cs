using System.Text;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.Paper;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Passport;

public sealed partial class CP14PassportSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public readonly EntProtoId PassportProto = "CP14Passport";
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawning);
    }

    private void OnPlayerSpawning(PlayerSpawnCompleteEvent ev)
    {
        if (!TryComp<InventoryComponent>(ev.Mob, out var inventory))
            return;

        var passport = Spawn(PassportProto, Transform(ev.Mob).Coordinates);

        if (!TryComp<PaperComponent>(passport, out var paper))
            return;

        var text = GeneratePassportText(ev);
        _paper.SetContent((passport, paper), text);
        _paper.TryStamp((passport, paper),
            new StampDisplayInfo
            {
                StampedColor = Color.FromHex("#0a332a"),
                StampedName = Loc.GetString("cp14-passport-stamp")
            },
            "");
        _inventory.TryEquip(ev.Mob, passport, "pocket1", inventory: inventory);
    }

    private string GeneratePassportText(PlayerSpawnCompleteEvent ev)
    {
        var sb = new StringBuilder();

        //Name
        sb.AppendLine(Loc.GetString("cp14-passport-name", ("name",  ev.Profile.Name)));
        //Species
        if (_proto.TryIndex(ev.Profile.Species, out var indexedSpecies))
            sb.AppendLine(Loc.GetString("cp14-passport-species", ("species", Loc.GetString(indexedSpecies.Name))));
        //Birthday
        var birthday = $"{_random.Next(40)}.{_random.Next(12)}.{225 - ev.Profile.Age}";
        sb.AppendLine(Loc.GetString("cp14-passport-birth-date", ("birthday",  birthday)));
        //Job
        if (ev.JobId is not null && _proto.TryIndex<JobPrototype>(ev.JobId, out var indexedJob))
            sb.AppendLine(Loc.GetString("cp14-passport-job", ("job",  Loc.GetString(indexedJob.Name))));

        return sb.ToString();
    }
}
