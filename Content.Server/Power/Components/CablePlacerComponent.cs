using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Power;

namespace Content.Server.Power.Components
{
    [RegisterComponent]
    public sealed partial class CablePlacerComponent : Component
    {
        [DataField("cablePrototypeID", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? CablePrototypeId = "CableHV";

        [DataField("blockingWireType")]
        public CableType BlockingCableType = CableType.HighVoltage;

        /// <summary>
        /// in the main game we can't put wires ON tiles, but here some wires can lie on top of tiles.
        /// </summary>
        [DataField]
        public bool CP14OnlySubfloor = true;
    }
}
