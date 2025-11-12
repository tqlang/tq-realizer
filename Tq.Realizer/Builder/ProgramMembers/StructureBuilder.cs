using System.Text;

namespace Tq.Realizer.Builder.ProgramMembers;

public class StructureBuilder: TypeBuilder, INamespaceOrStructureBuilder
{
    public StructureBuilder? Extends = null;
    
    public List<InstanceFieldBuilder> Fields = [];
    
    public List<PropertyBuilder> Properties = [];
    public List<StaticPropertyBuilder> StaticProperties = [];
    public List<StaticFieldBuilder> StaticFields = [];
    
    public List<StructureBuilder> InnerStructures = [];
    public List<TypeDefinitionBuilder> InnerTypedefs = [];
    public List<NamespaceBuilder> InnerNamespaces = [];
    public List<BaseFunctionBuilder> Functions = [];
    
    public ProgramMemberBuilder[] GetMembers()
        => [
            ..Properties,
            ..StaticProperties,
            ..StaticFields,
            ..InnerStructures,
            ..InnerTypedefs,
            ..InnerNamespaces,
            ..Functions
        ];
    
    public uint? Length = null;
    public uint? Alignment = null;
    
    internal StructureBuilder(INamespaceOrStructureBuilder parent, string name)
        : base(parent, name) {}


    public NamespaceBuilder AddNamespace(string symbol)
    {
        var ns = new NamespaceBuilder(this, symbol);
        InnerNamespaces.Add(ns);
        return ns;
    }
    public StructureBuilder AddStructure(string symbol)
    {
        var st = new StructureBuilder(this, symbol);
        InnerStructures.Add(st);
        return st;
    }
    public TypeDefinitionBuilder AddTypedef(string symbol)
    {
        var td = new TypeDefinitionBuilder(this, symbol);
        InnerTypedefs.Add(td);
        return td;
    }
    
    public FieldBuilder AddField(string symbol, bool isStatic)
    {
        if (isStatic)
        {
            var newField = new StaticFieldBuilder(this, symbol);
            StaticFields.Add(newField);
            return newField;
        }
        else
        {
            var newField = new InstanceFieldBuilder(this, symbol);
            Fields.Add(newField);
            return newField;
        }
    }

    public PropertyBuilder AddProperty(string symbol, bool isStatic)
    {
        if (isStatic)
        {
            var newProperty = new StaticPropertyBuilder(this, symbol);
            StaticProperties.Add(newProperty);
            return newProperty;
        }
        else
        {
            var  newProperty = new InstancePropertyBuilder(this, symbol);
            Properties.Add(newProperty);
            return newProperty;
        }
    }

    public FunctionBuilder AddFunction(string symbol, bool isStatice)
    {
        var newFunction = new FunctionBuilder(this, symbol, isStatice);
        Functions.Add(newFunction);
        return newFunction;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"(struct \"{Symbol}\"");
        if (Extends != null) sb.Append($" (extends {Extends.ToReadableReference()}");
        sb.AppendLine();
        
        if (Length != null || Alignment != null) sb.Append('\t');
        if (Length != null) sb.Append($"(length {Length}) ");
        if (Alignment != null) sb.Append($"(alignment {Alignment})");
        if (Length != null || Alignment != null) sb.AppendLine();

        foreach (var i in StaticFields) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in StaticProperties) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in Fields) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in Properties) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in Functions) sb.AppendLine(i.ToString().TabAllLines());
        sb.Length -= Environment.NewLine.Length;
        sb.Append(')');
        
        return sb.ToString();
    }
    public override string ToReadableReference() => '"' + string.Join('.', GlobalIdentifier) + '"';
}
