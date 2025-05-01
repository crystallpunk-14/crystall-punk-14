using System.Numerics;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Client._CP14.UserInterface.Systems.NodeTree;

[Serializable, NetSerializable]
public sealed class CP14NodeTreeElement(string nodeKey, bool active, Vector2 uiPosition, HashSet<string> childrens, SpriteSpecifier icon)
{
    public string NodeKey = nodeKey;
    public bool Active = active;

    public Vector2 UiPosition = uiPosition;
    public HashSet<string> Childrens = childrens;
    public SpriteSpecifier Icon = icon;
}

[Serializable, NetSerializable]
public sealed class CP14NodeTreeUiState(HashSet<CP14NodeTreeElement> nodes)
{
    public HashSet<CP14NodeTreeElement> Nodes = nodes;

    //TODO: customazible
    public SpriteSpecifier FrameIcon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_CP14/Interface/Skills/default.rsi"), "frame");
    public SpriteSpecifier HoveredIcon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_CP14/Interface/Skills/default.rsi"), "hovered");
    public SpriteSpecifier SelectedIcon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_CP14/Interface/Skills/default.rsi"), "selected");
    public SpriteSpecifier LearnedIcon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_CP14/Interface/Skills/default.rsi"), "learned");
}
