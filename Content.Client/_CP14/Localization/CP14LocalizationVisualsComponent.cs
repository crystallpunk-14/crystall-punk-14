namespace Content.Client._CP14.Localization;

/// <summary>
/// Controls the visual of the sprite, depending on the localization. Useful for drawn lettering
/// </summary>
[RegisterComponent]
public sealed partial class CP14LocalizationVisualsComponent : Component
{
    /// <summary>
    /// map(map,(lang, state))
    /// in yml:
    ///
    /// - type: Sprite
    ///   layers:
    ///   - state: stateName0
    ///     map: ["map1"]
    ///   - state: stateName0
    ///     map: ["map2"]
    /// - type: CP14LocalizationVisuals
    ///   mapStates:
    ///     map1:
    ///       ru-RU: stateName1
    ///       en-US: stateName2
    ///     map2:
    ///       ru-RU: stateName3
    ///       en-US: stateName4
    ///
    /// </summary>
    [DataField]
    public Dictionary<string, Dictionary<string, string>> MapStates = new();
}
