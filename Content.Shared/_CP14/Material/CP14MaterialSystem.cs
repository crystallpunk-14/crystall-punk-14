using System.Text;
using Content.Shared.Examine;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Material;

public sealed partial class CP14MaterialSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MaterialComponent, ExaminedEvent>(OnMaterialExamined);
    }

    private void OnMaterialExamined(Entity<CP14MaterialComponent> ent, ref ExaminedEvent args)
    {
        TryComp<StackComponent>(ent, out var stack);

        var sb = new StringBuilder();

        sb.Append($"{Loc.GetString("cp14-material-examine")}\n");
        foreach (var material in ent.Comp.Materials)
        {
            if (!_proto.TryIndex(material.Key, out var indexedMaterial))
                continue;

            var count = material.Value;
            if (stack is not null)
                count *= stack.Count;

            sb.Append($"[color={indexedMaterial.Color.ToHex()}]{Loc.GetString(indexedMaterial.Name)}[/color] ({count})\n");
        }
        args.PushMarkup(sb.ToString());
    }

}
