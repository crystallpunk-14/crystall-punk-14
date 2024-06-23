using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Farming;

/// <summary>
/// The backbone of any plant. Provides common variables for the plant to other components, and a link to the soil
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class CP14PlantComponent : Component
{
    private float _resource;
    private float _health;
    private float _energy;
    private float _growth;

    /// <summary>
    /// Soil link. May be null, as not all plants in the world grow on entity soil (e.g. wild shrubs)
    /// </summary>
    public EntityUid? SoilUid;

    /// <summary>
    /// The ability to consume a resource for growing
    /// </summary>
    [DataField]
    public float Energy
    {
        set => _energy = MathHelper.Clamp(value, 0, MaxEnergy);
        get => _energy;
    }

    [DataField]
    public float MaxEnergy = 100f;


    /// <summary>
    /// resource consumed for growth
    /// </summary>
    [DataField]
    public float Resource
    {
        set => _resource = MathHelper.Clamp(value, 0, MaxResource);
        get => _resource;
    }

    [DataField]
    public float MaxResource = 100f;


    [DataField]
    public float Health
    {
        set => _health = MathHelper.Clamp(value, 0, MaxHealth);
        get => _health;
    }

    /// <summary>
    ///
    /// </summary>
    [DataField]
    public float MaxHealth = 10f;

    /// <summary>
    /// Plant growth status, 0 to 1
    /// </summary>
    [DataField, AutoNetworkedField]
    public float GrowthLevel
    {
        set => _growth = MathHelper.Clamp01(value);
        get => _growth;
    }

    [DataField(serverOnly: true)]
    public float UpdateFrequency = 90f;

    [DataField(serverOnly: true)]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;
}

/// <summary>
/// Is called periodically at random intervals on the plant.
/// </summary>
public sealed class CP14AfterPlantUpdateEvent : EntityEventArgs
{
    public readonly CP14PlantComponent Plant;

    public CP14AfterPlantUpdateEvent(CP14PlantComponent comp)
    {
        Plant = comp;
    }
}

public sealed class CP14PlantEnergyUpdateEvent : EntityEventArgs
{
    public float Energy = 0f;
    public bool Canceled = false;
}

public sealed class CP14PlantResourceUpdateEvent : EntityEventArgs
{
    public float Resource = 0f;
    public bool Canceled = false;
}

/// Задачи:
/// Для роста требуется ресурсы. Вода. Для некоторых растений может требоваться уникальные ресурсы.
/// Значит, у растения есть реагент, которое оно потребляет для роста. Раз в минуту например оно пытается получить из почвы реагенты.
/// Реагенты преобразуются в ресурс роста. Ресурс роста раз в минуту тратится, выращивая растение.
///
/// Какие цели вообще ставятся перед фермерством?
/// Первое - Производство большого количества еды. Я хочу видеть поля засеянные пшеницой.
/// Второе - производство алхимических реагентов.
/// Третье - Куски этой системы должны использоваться в дикой природе (Сбор ягод с кустов, их восстановление)
///
/// Поливание - попросту канонично.
/// Уход (убирание сорняков или вредителей)
///
/// 1) Как можно облажаться?
/// Если не собрать урожай вовремя, он сгниет
/// Урожай может
/// 2) Как все может пойти ужасно и нелепо?
/// Некоторые растения могут представлять опасность если не уследить (выбежавшие из земли корнеплоды-убийцы)
///
///Фермерство - побочная активность, которая не требует сильного внимания. Она просто раз в несколько дней дает много ресурсов.
/// Учитывая большие промежутки времени между получением этих ресурсов, у игроков должен быть их запас раундстартом, и возможность их находить в мире.
