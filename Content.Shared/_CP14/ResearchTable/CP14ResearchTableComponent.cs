using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.ResearchTable;

[RegisterComponent, NetworkedComponent]
public sealed partial class CP14ResearchTableComponent : Component
{
    [DataField]
    public float ResearchSpeed = 3f;

    [DataField]
    public float ResearchRadius = 0.5f;

    [DataField]
    public SoundSpecifier ResearchSound = new SoundCollectionSpecifier("PaperScribbles");
}
