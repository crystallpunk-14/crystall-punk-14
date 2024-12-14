namespace Content.Shared._CP14.Fishing.Events;

[ByRefEvent]
public readonly struct CP14FishingFinishedEvent
{
    public readonly bool Success;

    public CP14FishingFinishedEvent(bool success)
    {
        Success = success;
    }
}
