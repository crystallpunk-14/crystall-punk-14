using Content.Server.Movement.Components;
using Content.Shared._CP14.Eye;

namespace Content.Server._CP14.Eye;

public sealed class CP14ToggleableEyeOffsetSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<EyeComponent, CP14EyeOffsetToggleActionEvent>(OnToggleEyeOffset);
    }

    private void OnToggleEyeOffset(Entity<EyeComponent> ent, ref CP14EyeOffsetToggleActionEvent args)
    {
        if (!HasComp<EyeCursorOffsetComponent>(ent))
            AddComp<EyeCursorOffsetComponent>(ent);
        else
            RemComp<EyeCursorOffsetComponent>(ent);
    }
}
