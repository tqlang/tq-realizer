using System.Text;

namespace Tq.Realizer.Builder.ProgramMembers;

public class NamespaceBuilder: ProgramMemberBuilder, INamespaceOrStructureBuilder
{
    
    internal List<NamespaceBuilder> namespaces = [];
    internal List<StaticFieldBuilder> fields = [];
    internal List<BaseFunctionBuilder> functions = [];
    internal List<StructureBuilder> structures = [];
    internal List<TypeDefinitionBuilder> typedefs = [];

    public NamespaceBuilder[] Namespaces => [..namespaces];
    public StaticFieldBuilder[] Fields => [..fields]; 
    public BaseFunctionBuilder[] Functions => [..functions];
    public StructureBuilder[] Structures => [..structures];
    public TypeDefinitionBuilder[] TypeDefinitions => [..typedefs];
    
    internal NamespaceBuilder(INamespaceOrStructureBuilder parent, string name)
        : base(null!, name, false) { }
    
    public NamespaceBuilder AddNamespace(string symbol)
    {
        var newNamespace = new NamespaceBuilder(this, symbol);
        namespaces.Add(newNamespace);
        return newNamespace;
    }
    public FunctionBuilder AddFunction(string symbol)
    {
        var newFunction = new FunctionBuilder(this, symbol, false);
        functions.Add(newFunction);
        return newFunction;
    }
    public StructureBuilder AddStructure(string symbol)
    {
        var newStructure = new StructureBuilder(this, symbol, false);
        structures.Add(newStructure);
        return newStructure;
    }
    public TypeDefinitionBuilder AddTypedef(string symbol)
    {
        var newTypedef = new TypeDefinitionBuilder(this, symbol, false);
        typedefs.Add(newTypedef);
        return newTypedef;
    }
    public StaticFieldBuilder AddStaticField(string symbol)
    {
        var newField = new StaticFieldBuilder(this, symbol, false);
        fields.Add(newField);
        return newField;
    }


    public FunctionBuilder AddAnnonymousFunction(string symbol)
    {
        var newFunction = new FunctionBuilder(this, symbol, true);
        functions.Add(newFunction);
        return newFunction;
    }
    public StructureBuilder AddAnnonymousStructure(string symbol)
    {
        var newStructure = new StructureBuilder(this, symbol, true);
        structures.Add(newStructure);
        return newStructure;
    }
    public TypeDefinitionBuilder AddAnnonymousTypedef(string symbol)
    {
        var newTypedef = new TypeDefinitionBuilder(this, symbol, true);
        typedefs.Add(newTypedef);
        return newTypedef;
    }
    
    
    public ImportedFunctionBuilder AddExternImportedFunction(string fn)
    {
        var newFunction = new ImportedFunctionBuilder(this, fn);
        functions.Add(newFunction);
        return newFunction;
    }
    
    
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"(namespace \"{Symbol}\"");
        foreach (var i in namespaces) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in fields) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in functions) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in structures) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in typedefs) sb.AppendLine(i.ToString().TabAllLines());
        sb.Length -= Environment.NewLine.Length;
        sb.Append(')');
        
        return sb.ToString();
    }
    
    public override string ToReadableReference() => '"' + string.Join('.', GlobalIdentifier) + '"';
    
}
