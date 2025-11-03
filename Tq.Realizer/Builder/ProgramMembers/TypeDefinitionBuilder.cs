namespace Tq.Realizer.Builder.ProgramMembers;

public class TypeDefinitionBuilder: TypeBuilder
{
    
    internal TypeDefinitionBuilder(INamespaceOrStructureBuilder parent, string name, bool annonymous)
        : base(parent, name, annonymous) {}
    
    
    public override string ToString() => $"(typedef \"{Symbol}\")";
    public override string ToReadableReference() => '"' + string.Join('.', GlobalIdentifier) + '"';
}
