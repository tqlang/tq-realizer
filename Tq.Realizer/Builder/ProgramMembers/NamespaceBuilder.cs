using System.Text;
using Tq.Realizer.Core.Exceptions;

namespace Tq.Realizer.Builder.ProgramMembers;

public class NamespaceBuilder: ProgramMemberBuilder, INamespaceOrStructureBuilder
{
    
    internal List<NamespaceBuilder> _namespaces = [];
    internal List<StaticFieldBuilder> _fields = [];
    internal List<StaticPropertyBuilder> _props = [];
    internal List<BaseFunctionBuilder> _functions = [];
    internal List<StructureBuilder> _structures = [];
    internal List<TypedefBuilder> _typedefs = [];

    public NamespaceBuilder[] Namespaces => [.. _namespaces];
    public StaticFieldBuilder[] Fields => [.. _fields]; 
    public StaticPropertyBuilder[] Properties => [.. _props]; 
    public BaseFunctionBuilder[] Functions => [.. _functions];
    public StructureBuilder[] Structures => [.. _structures];
    public TypedefBuilder[] TypeDefinitions => [.. _typedefs];
    public ProgramMemberBuilder[] GetMembers()
        => [ .._namespaces, .._props, .._fields, .._functions, .._structures, .._typedefs ];
    
    
    internal NamespaceBuilder(INamespaceOrStructureBuilder parent, string name) : base(null!, name) { }
    
    public NamespaceBuilder AddNamespace(string symbol)
    {
        var newNamespace = new NamespaceBuilder(this, symbol);
        _namespaces.Add(newNamespace);
        return newNamespace;
    }
    
    
    public FunctionBuilder AddFunction(string symbol, bool isStatic)
    {
        var newFunction = new FunctionBuilder(this, symbol, isStatic);
        _functions.Add(newFunction);
        return newFunction;
    }
    public FieldBuilder AddField(string symbol, bool isStatic)
    {
        if (!isStatic) throw new TqInvalidMemberFlagException("Namespace cannot have non-static fields");
;            
        var newField = new StaticFieldBuilder(this, symbol);
        _fields.Add(newField);
        return newField;
    }

    public PropertyBuilder AddProperty(string symbol, bool isStatic)
    {
        if (!isStatic) throw new TqInvalidMemberFlagException("Namespace cannot have non-static properties");
        
        var newProperty = new StaticPropertyBuilder(this, symbol);
        _props.Add(newProperty);
        return newProperty;
    }

    public StructureBuilder AddStructure(string symbol)
    {
        var newStructure = new StructureBuilder(this, symbol);
        _structures.Add(newStructure);
        return newStructure;
    }
    public TypedefBuilder AddTypedef(string symbol)
    {
        var newTypedef = new TypedefBuilder(this, symbol);
        _typedefs.Add(newTypedef);
        return newTypedef;
    }


    
    public ImportedFunctionBuilder AddExternImportedFunction(string fn)
    {
        var newFunction = new ImportedFunctionBuilder(this, fn);
        _functions.Add(newFunction);
        return newFunction;
    }
    
    
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"(namespace \"{Symbol}\"");
        foreach (var i in _namespaces) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in _fields) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in _props) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in _functions) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in _structures) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in _typedefs) sb.AppendLine(i.ToString().TabAllLines());
        sb.Length -= Environment.NewLine.Length;
        sb.Append(')');
        
        return sb.ToString();
    }
    
    public override string ToReadableReference() => '"' + string.Join('.', GlobalIdentifier) + '"';
    
}
