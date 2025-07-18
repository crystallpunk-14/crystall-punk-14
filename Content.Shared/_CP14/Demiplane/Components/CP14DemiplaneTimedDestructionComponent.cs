using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Demiplane.Components;

/// <summary>
/// Destroy demiplane after time
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause, Access(typeof(CP14SharedDemiplaneSystem))]
public sealed partial class CP14DemiplaneTimedDestructionComponent : Component
{
    [DataField]
    public TimeSpan TimeToDestruction = TimeSpan.FromSeconds(240f);

    [DataField, AutoPausedField]
    public TimeSpan EndTime = TimeSpan.Zero;

    /// <summary>
    /// Countdown audio stream.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Stream = null;

    /// <summary>
    /// Sound that plays when the demiplane start to collapse.
    /// </summary>
    [DataField]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("ExpeditionEnd")
    {
        Params = AudioParams.Default.WithVolume(-5),
    };

    /// <summary>
    /// Song selected on MapInit so we can predict the audio countdown properly.
    /// </summary>
    [DataField]
    public SoundPathSpecifier? SelectedSong;
}
