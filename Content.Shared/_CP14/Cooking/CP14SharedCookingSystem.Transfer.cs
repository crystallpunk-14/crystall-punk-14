/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Cooking.Components;
using Content.Shared.Interaction;
using Robust.Shared.Containers;

namespace Content.Shared._CP14.Cooking;

public abstract partial class CP14SharedCookingSystem
{
    private void InitTransfer()
    {
        SubscribeLocalEvent<CP14FoodHolderComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<CP14FoodHolderComponent, InteractUsingEvent>(OnInteractUsing);

        SubscribeLocalEvent<CP14FoodCookerComponent, ContainerIsInsertingAttemptEvent>(OnInsertAttempt);
    }

    private void OnInteractUsing(Entity<CP14FoodHolderComponent> target, ref InteractUsingEvent args)
    {
        if (!TryComp<CP14FoodHolderComponent>(args.Used, out var used))
            return;

        TryTransferFood(target, (args.Used, used));
    }

    private void OnAfterInteract(Entity<CP14FoodHolderComponent> ent, ref AfterInteractEvent args)
    {
        if (!TryComp<CP14FoodHolderComponent>(args.Target, out var target))
            return;

        TryTransferFood(ent, (args.Target.Value, target));
    }

    private void OnInsertAttempt(Entity<CP14FoodCookerComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<CP14FoodHolderComponent>(ent, out var holder))
            return;

        if (holder.FoodData is not null)
        {
            _popup.PopupEntity(Loc.GetString("cp14-cooking-popup-not-empty", ("name", MetaData(ent).EntityName)), ent);
            args.Cancel();
        }
    }
}
