namespace Content.Shared.Beam;

public abstract class SharedBeamSystem : EntitySystem
{
    //CP14 Shared ability to create beams.
    public virtual void TryCreateBeam(EntityUid user,
        EntityUid target,
        string bodyPrototype,
        string? bodyState = null,
        string shader = "unshaded",
        EntityUid? controller = null)
    {

    }
}
