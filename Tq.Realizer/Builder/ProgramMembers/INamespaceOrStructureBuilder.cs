namespace Tq.Realizer.Builder.ProgramMembers;

public interface INamespaceOrStructureBuilder: INamespaceOrStructureOrTypedefBuilder
{
    public NamespaceBuilder AddNamespace(string symbol);
    public StructureBuilder AddStructure(string symbol);
    public TypeDefinitionBuilder AddTypedef(string symbol);
    public FieldBuilder AddField(string symbol, bool isStatic);
    public PropertyBuilder AddProperty(string symbol, bool isStatic);
}
