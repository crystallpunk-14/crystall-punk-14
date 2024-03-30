
using Content.Server.GameTicking.Events;
using Content.Shared.CrystallPunk.LockKey;
using Content.Shared.Examine;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server.CrystallPunk.LockKey;


public sealed partial class CPLockKeySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private Dictionary<ProtoId<CPLockCategoryPrototype>, List<int>> _roundKeyData = new();

    private const int DepthCompexity = 2;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);

        SubscribeLocalEvent<CPLockComponent, MapInitEvent>(OnLockInit);
        SubscribeLocalEvent<CPKeyComponent, ExaminedEvent>(OnKeyExamine);
        SubscribeLocalEvent<CPKeyComponent, MapInitEvent>(OnKeyInit);

    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        _roundKeyData = new();
    }

    private void OnKeyExamine(Entity<CPKeyComponent> key, ref ExaminedEvent args)
    {
        if (key.Comp.LockShape == null)
            return;

        foreach (var item in key.Comp.LockShape)
        {
            args.PushMarkup($"{item}");
        }
    }

    private void OnKeyInit(Entity<CPKeyComponent> keyEnt, ref MapInitEvent args)
    {
        if (keyEnt.Comp.AutoGenerateKey != null)
        {
            keyEnt.Comp.LockShape = GetKeyLockData(keyEnt.Comp.AutoGenerateKey.Value);
        }
    }

    private void OnLockInit(Entity<CPLockComponent> lockEnt, ref MapInitEvent args)
    {
       
    }

    private List<int> GetKeyLockData(ProtoId<CPLockCategoryPrototype> category)
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

    private List<int> GenerateNewUniqueLockData(ProtoId<CPLockCategoryPrototype> category)
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

    private List<int> InvertLockData(List<int> input)
    {
        var newKeyData = input;
        for (int i = 0; i < input.Count; i++)
        {
            newKeyData[i] = input[i] * -1;
        }
        return newKeyData;
    }
}
