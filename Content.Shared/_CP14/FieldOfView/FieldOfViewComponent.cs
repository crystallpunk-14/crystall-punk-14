using Robust.Shared.GameStates;

namespace Content.Shared._Finster.FieldOfView;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FieldOfViewComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Enabled { get; set; } = true;

    [DataField, AutoNetworkedField]
    public float Angle { get; set; } = 270.0f;

    [DataField, AutoNetworkedField]
    public float MinDistance { get; set; } = 0.65f;

    [DataField, AutoNetworkedField]
    public float MaxDistance { get; set; } = 128.0f;

    [DataField, AutoNetworkedField]
    public BlockedVisionDirection Direction = BlockedVisionDirection.Backward;

    [DataField, AutoNetworkedField]
    public bool Simple4DirMode = false;

    public float GetRotation(BlockedVisionDirection dir)
    {
        switch (dir)
        {
            case BlockedVisionDirection.Backward:
                return -90.0f;
            case BlockedVisionDirection.Toward:
                return 90.0f;
        }

        return 0.0f;
    }
}

public enum BlockedVisionDirection : ushort
{
    Backward,
    Toward
}
