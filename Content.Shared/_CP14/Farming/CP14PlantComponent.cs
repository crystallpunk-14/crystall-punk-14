namespace Content.Shared._CP14.Farming;

/// <summary>
/// The backbone of any plant. Provides common variables for the plant to other components, and a link to the soil
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedFarmingSystem))]
public sealed partial class CP14PlantComponent : Component
{
    /// <summary>
    /// Soil link. May be null, as not all plants in the world grow on entity soil (e.g. wild shrubs)
    /// </summary>
    public EntityUid? SoilUid;

    /// <summary>
    /// The ability to consume a resource for growing
    /// </summary>
    [DataField]
    public float Energy = 0f;

    /// <summary>
    /// resource consumed for growth
    /// </summary>
    [DataField]
    public float Resource = 0f;

    /// <summary>
    /// Plant growth status, 0 to 1
    /// </summary>
    [DataField]
    public float GrowthLevel = 0f;
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
