using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Fishing.Components
{
    /// <summary>
    /// Key for CP14FishingBoundUserInterface
    /// </summary>
    [Serializable, NetSerializable]
    public enum CP14FishingUiKey : byte
    {
        Key,
    }

    /// <summary>
    /// Event for sending reeling key status
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class CP14FishingReelKeyMessage : EntityEventArgs
    {
        public bool Reeling { get; }

        public CP14FishingReelKeyMessage(bool reeling)
        {
            Reeling = reeling;
        }
    }
}
