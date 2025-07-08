using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Salary;

/// <summary>
/// Pays out the salary upon interaction, if it has accumulated for the player.
/// </summary>
[RegisterComponent, Access(typeof(CP14SalarySystem))]
public sealed partial class CP14SalaryPairollComponent : Component
{
    [DataField]
    public SoundSpecifier BuySound = new SoundPathSpecifier("/Audio/_CP14/Effects/cash.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.1f),
    };

    [DataField]
    public EntProtoId BuyVisual = "CP14CashImpact";
}
