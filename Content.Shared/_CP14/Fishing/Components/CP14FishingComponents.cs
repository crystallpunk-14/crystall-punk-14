using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Fishing.Components
{
    [Serializable, NetSerializable]
    public enum CP14FishingUiKey : byte
    {
        Key,
    }

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
