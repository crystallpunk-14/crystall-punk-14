namespace Content.Server._CP14.Magic;

[Serializable, DataDefinition]
public abstract partial class MagicSpell
{
    protected readonly IEntityManager EntityManager;

    public MagicSpell()
    {
        EntityManager = IoCManager.Resolve<IEntityManager>();
    }

    [DataField]
    public virtual int BaseCost { get; set; } = 10;

    public virtual void Modify(MagicSpellContext context)
    {
        context.Cost += BaseCost;
    }
}
