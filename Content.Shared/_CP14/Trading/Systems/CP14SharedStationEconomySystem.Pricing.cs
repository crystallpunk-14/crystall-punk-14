using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Materials;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Trading.Systems;

//TODO: All of this should be removed when PricingSystem in the upstream moves to Shared.
public abstract partial class CP14SharedStationEconomySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    /// <summary>
    /// Get a rough price for an entityprototype. Does not consider contained entities.
    /// </summary>
    public double GetEstimatedPrice(EntityPrototype prototype)
    {
        var ev = new EstimatedPriceCalculationEvent(prototype);

        RaiseLocalEvent(ref ev);

        if (ev.Handled)
            return ev.Price;

        var price = ev.Price;
        price += GetMaterialsPrice(prototype);
        price += GetSolutionsPrice(prototype);
        // Can't use static price with stackprice
        var oldPrice = price;
        price += GetStackPrice(prototype);

        if (oldPrice.Equals(price))
        {
            price += GetStaticPrice(prototype);
        }

        // TODO: Proper container support.

        return price;
    }

    private double GetStaticPrice(EntityPrototype prototype)
    {
        var price = 0.0;

        if (prototype.Components.TryGetValue(Factory.GetComponentName<StaticPriceComponent>(), out var staticProto))
        {
            var staticPrice = (StaticPriceComponent) staticProto.Component;
            price += staticPrice.Price;
        }

        return price;
    }

    private double GetMaterialsPrice(EntityPrototype prototype)
    {
        double price = 0;

        //CP14 We take materials into account when calculating the price in any case.
        if ((prototype.Components.ContainsKey(Factory.GetComponentName<MaterialComponent>()) || prototype.ID.StartsWith("CP14")) &&
            prototype.Components.TryGetValue(Factory.GetComponentName<PhysicalCompositionComponent>(), out var composition))
        {
            var compositionComp = (PhysicalCompositionComponent) composition.Component;
            var matPrice = GetMaterialPrice(compositionComp);

            if (prototype.Components.TryGetValue(Factory.GetComponentName<StackComponent>(), out var stackProto))
            {
                matPrice *= ((StackComponent) stackProto.Component).Count;
            }

            price += matPrice;
        }

        return price;
    }

    private double GetMaterialPrice(PhysicalCompositionComponent component)
    {
        double price = 0;
        foreach (var (id, quantity) in component.MaterialComposition)
        {
            price += _prototypeManager.Index<MaterialPrototype>(id).Price * quantity;
        }
        return price;
    }

    private double GetSolutionsPrice(EntityPrototype prototype)
    {
        var price = 0.0;

        if (prototype.Components.TryGetValue(Factory.GetComponentName<SolutionContainerManagerComponent>(), out var solManager))
        {
            var solComp = (SolutionContainerManagerComponent) solManager.Component;
            price += GetSolutionPrice(solComp);
        }

        return price;
    }
    private double GetSolutionPrice(SolutionContainerManagerComponent component)
    {
        var price = 0.0;

        foreach (var (_, prototype) in _solutionContainerSystem.EnumerateSolutions(component))
        {
            foreach (var (reagent, quantity) in prototype.Contents)
            {
                if (!_prototypeManager.TryIndex<ReagentPrototype>(reagent.Prototype, out var reagentProto))
                    continue;

                // TODO check ReagentData for price information?
                price += (float) quantity * reagentProto.PricePerUnit;
            }
        }

        return price;
    }

    private double GetStackPrice(EntityPrototype prototype)
    {
        var price = 0.0;

        if (prototype.Components.TryGetValue(Factory.GetComponentName<StackPriceComponent>(), out var stackpriceProto) &&
            prototype.Components.TryGetValue(Factory.GetComponentName<StackComponent>(), out var stackProto) &&
            !prototype.Components.ContainsKey(Factory.GetComponentName<MaterialComponent>()))
        {
            var stackPrice = (StackPriceComponent) stackpriceProto.Component;
            var stack = (StackComponent) stackProto.Component;
            price += stack.Count * stackPrice.Price;
        }

        return price;
    }
}
