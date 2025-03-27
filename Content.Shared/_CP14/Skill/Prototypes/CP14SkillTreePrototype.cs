using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Skill.Prototypes;

/// <summary>
/// A group of skills combined into one “branch”
/// </summary>
[Prototype("cp14SkillTree")]
public sealed partial class CP14SkillTreePrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField(required: true)]
    public LocId Name;

    [DataField]
    public SpriteSpecifier FrameIcon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_CP14/Interface/Skills/default.rsi"), "frame");

    [DataField]
    public SpriteSpecifier HoveredIcon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_CP14/Interface/Skills/default.rsi"), "hovered");

    [DataField]
    public SpriteSpecifier SelectedIcon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_CP14/Interface/Skills/default.rsi"), "selected");

    [DataField]
    public SpriteSpecifier LearnedIcon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_CP14/Interface/Skills/default.rsi"), "learned");

    [DataField]
    public SpriteSpecifier AvailableIcon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_CP14/Interface/Skills/default.rsi"), "available");

    [DataField]
    public string Parallax = "AspidParallax";

    [DataField]
    public LocId? Desc;

    [DataField]
    public Color Color;

    [DataField]
    public SoundSpecifier LearnSound = new SoundCollectionSpecifier("CP14LearnSkill");
}
