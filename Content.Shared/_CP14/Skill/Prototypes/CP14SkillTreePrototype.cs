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
    public SpriteSpecifier FrameIcon = new SpriteSpecifier.Texture(new("/Textures/_CP14/Interface/Skills/default.rsi/frame.png"));

    [DataField]
    public SpriteSpecifier HoveredIcon = new SpriteSpecifier.Texture(new("/Textures/_CP14/Interface/Skills/default.rsi/hovered.png"));

    [DataField]
    public SpriteSpecifier SelectedIcon = new SpriteSpecifier.Texture(new("/Textures/_CP14/Interface/Skills/default.rsi/selected.png"));

    [DataField]
    public SpriteSpecifier LearnedIcon = new SpriteSpecifier.Texture(new("/Textures/_CP14/Interface/Skills/default.rsi/learned.png"));

    [DataField]
    public SpriteSpecifier AvailableIcon = new SpriteSpecifier.Texture(new("/Textures/_CP14/Interface/Skills/default.rsi/available.png"));

    [DataField]
    public string Parallax = "AspidParallax";

    [DataField]
    public LocId? Desc;

    [DataField]
    public Color Color;
}
