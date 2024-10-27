namespace Content.Shared._CP14.TravelingStoreShip;

public class CP14SharedCargoSystem : EntitySystem
{
}

/// <summary>
/// is called in just before the goods are shipped into town, and before the profit on them is calculated. It allows you to edit the list of sent items.
/// </summary>
public sealed class BeforeSellEntities : EntityEventArgs
{
    public HashSet<EntityUid> Sent;

    public BeforeSellEntities(ref HashSet<EntityUid> sent)
    {
        Sent = sent;
    }
}
