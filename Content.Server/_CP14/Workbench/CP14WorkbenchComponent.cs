/*
 * All right reserved to Crystallpunk.
 *
 * BUT this file is sublicensed under CLA License for next projects:
 * https://github.com/technologists-team/after-rain
 * https://github.com/EscapeD-14/EscapeD-14
 *
 */

using Content.Shared._CP14.Workbench.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Workbench;

[RegisterComponent]
[Access(typeof(CP14WorkbenchSystem))]
public sealed partial class CP14WorkbenchComponent : Component
{
    [DataField]
    public float CraftSpeed = 1f;

    [DataField]
    public List<ProtoId<CP14WorkbenchRecipePrototype>> Recipes = new();

    [DataField]
    public SoundSpecifier CraftSound = new SoundCollectionSpecifier("CP14Hammering");
}
