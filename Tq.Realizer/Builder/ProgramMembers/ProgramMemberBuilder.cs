namespace Tq.Realizer.Builder.ProgramMembers;

public abstract class ProgramMemberBuilder
{
    internal string _symbol;
    public virtual ModuleBuilder Module => Parent?.Module!;
    public INamespaceOrStructureBuilder? Parent { get; internal set; }
    public readonly bool Annonymous;
    
    public string Symbol => (Annonymous ? "__annon__" : "") + _symbol;
    
    public string[] GlobalIdentifier => Parent == null
        ? [Symbol]
        : [..Parent.GlobalIdentifier.Where(e=>!string.IsNullOrEmpty(e)), Symbol];
    
    internal ProgramMemberBuilder(INamespaceOrStructureBuilder parent, string symbol, bool annonymous)
    {
        Parent = parent;
        _symbol = symbol;
        Annonymous = annonymous;
    }

    public abstract string ToReadableReference();
}
