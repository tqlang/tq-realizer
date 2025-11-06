namespace Tq.Realizer.Builder.ProgramMembers;

public abstract class ProgramMemberBuilder
{
    internal string _symbol;
    public string Symbol => _symbol;
    public virtual ModuleBuilder Module => Parent?.Module!;
    public INamespaceOrStructureOrTypedefBuilder? Parent { get; internal set; }
    
    
    public string[] GlobalIdentifier => Parent == null
        ? [_symbol]
        : [..Parent.GlobalIdentifier.Where(e=>!string.IsNullOrEmpty(e)), _symbol];
    
    internal ProgramMemberBuilder(INamespaceOrStructureOrTypedefBuilder parent, string symbol)
    {
        Parent = parent;
        _symbol = symbol;
    }

    public abstract string ToReadableReference();
}
