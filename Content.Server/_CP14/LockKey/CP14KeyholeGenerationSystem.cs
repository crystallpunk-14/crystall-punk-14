using System.Linq;
using Content.Shared._CP14.LockKey;
using Content.Shared._CP14.LockKey.Components;
using Content.Shared.Examine;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.LockKey;

public sealed partial class CP14KeyholeGenerationSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private Dictionary<ProtoId<CP14LockCategoryPrototype>, List<int>> _roundKeyData = new();

    private const int DepthCompexity = 2;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnd);

        SubscribeLocalEvent<CP14LockComponent, MapInitEvent>(OnLockInit);
        SubscribeLocalEvent<CP14KeyComponent, MapInitEvent>(OnKeyInit);

        SubscribeLocalEvent<CP14KeyComponent, ExaminedEvent>(OnKeyExamine);
    }

    #region Init
    private void OnRoundEnd(RoundRestartCleanupEvent ev)
    {
        _roundKeyData = new();
    }

    private void OnKeyInit(Entity<CP14KeyComponent> keyEnt, ref MapInitEvent args)
    {
        if (keyEnt.Comp.AutoGenerateShape != null)
        {
            keyEnt.Comp.LockShape = GetKeyLockData(keyEnt.Comp.AutoGenerateShape.Value);
        }
    }

    private void OnLockInit(Entity<CP14LockComponent> lockEnt, ref MapInitEvent args)
    {
        if (lockEnt.Comp.AutoGenerateShape != null)
        {
            lockEnt.Comp.LockShape = GetKeyLockData(lockEnt.Comp.AutoGenerateShape.Value);
        }
    }
    #endregion

    private void OnKeyExamine(Entity<CP14KeyComponent> key, ref ExaminedEvent args)
    {
        var parent = Transform(key).ParentUid;
        if (parent != args.Examiner)
            return;

        if (key.Comp.LockShape == null)
            return;

        var markup = Loc.GetString("cp14-lock-examine-key", ("item", MetaData(key).EntityName));
        markup += " (";
        foreach (var item in key.Comp.LockShape)
        {
            markup += $"{item} ";
        }
        markup += ")";
        args.PushMarkup(markup);
    }

    private List<int> GetKeyLockData(ProtoId<CP14LockCategoryPrototype> category)
    {
        if (_roundKeyData.ContainsKey(category))
            return _roundKeyData[category];
        else
        {
            var newData = GenerateNewUniqueLockData(category);
            _roundKeyData[category] = newData;
            return newData;
        }
    }

    private List<int> GenerateNewUniqueLockData(ProtoId<CP14LockCategoryPrototype> category)
    {
        List<int> newKeyData = new List<int>();
        var categoryData = _proto.Index(category);
        var ready = false;
        var iteration = 0;

        while (!ready)
        {
            //Generate try
            newKeyData = new List<int>();
            for (int i = 0; i < categoryData.Complexity; i++)
            {
                newKeyData.Add(_random.Next(-DepthCompexity, DepthCompexity));
            }

            //Identity Check shitcode
            // На текущий момент он пытается сгенерировать уникальный код. Если он 100 раз не смог сгенерировать уникальный код, он выдаст последний сгенерированный неуникальный.
            var unique = true;
            foreach (var pair in _roundKeyData)
            {
                if (newKeyData.SequenceEqual(pair.Value))
                {
                    unique = false;
                    break;
                }
            }
            if (unique)
                return newKeyData;
            else
                iteration++;

            if (iteration > 100)
            {
                break;
            }
        }
        Log.Error("The unique key for CPLockSystem could not be generated!");
        return newKeyData; //FUCK
    }
}
