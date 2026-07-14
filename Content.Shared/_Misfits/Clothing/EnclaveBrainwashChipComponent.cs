using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Misfits.Clothing;

/// <summary>
/// Marks an Enclave implanter device as lock-restricted equipment.
/// Enclave officers (Senior, Junior, Commander) can remove it via AccessReader. Others may pry it out
/// with wire cutters after a 20-second DoAfter.
/// Crafted chips produce a unique paired key that also unlocks them if re-applied.
/// </summary>
[RegisterComponent] // #Misfits Fix - Removed NetworkedComponent: no AutoGenerateComponentState → MissingMetadataException
public sealed partial class EnclaveBrainwashChipComponent : Component
{
    /// <summary>
    /// Crafted chips spawn this key prototype, then stamp it with a unique access tag.
    /// </summary>
    [DataField]
    public EntProtoId KeyPrototype = "N14IDKeyEnclaveBrainwashChipKey";

    /// <summary>
    /// Tool quality required to forcibly cut open a locked bracelet.
    /// </summary>
    [DataField("cutToolQuality")]
    public string CutToolQuality = "Cutting";

    /// <summary>
    /// Duration in seconds for the rescue-cut DoAfter (20 seconds per design spec).
    /// </summary>
    [DataField]
    public float CutUnlockTime = 20f;

    /// <summary>
    /// Prefix used when generating unique runtime access tags for crafted bracelets.
    /// </summary>
    [DataField]
    public string RandomAccessPrefix = "EnclaveBrainwashChip";

    [DataField]
    public int RandomKeyMin = 1000;

    [DataField]
    public int RandomKeyMax = 9999;

    /// <summary>
    /// Set to true once a unique key has been generated so it is not generated again.
    /// </summary>
    [DataField]
    public bool GeneratedKey;
}

/// <summary>
/// Fired when a rescue tool finishes cutting a locked Enclave brainwash chip.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class EnclaveBrainwashChipCutDoAfterEvent : DoAfterEvent
{
    public override DoAfterEvent Clone()
    {
        return this;
    }
}
