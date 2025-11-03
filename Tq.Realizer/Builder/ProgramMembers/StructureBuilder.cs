using System.Text;

namespace Tq.Realizer.Builder.ProgramMembers;

public class StructureBuilder: TypeBuilder, INamespaceOrStructureBuilder
{
    public StructureBuilder? Extends = null;
    public List<InstanceFieldBuilder> Fields = [];
    
    public List<StaticFieldBuilder> StaticFields = [];
    public List<BaseFunctionBuilder> Functions = [];
    
    public uint? Length = null;
    public uint? Alignment = null;
    public uint? VTableSize = null;
    
    internal StructureBuilder(INamespaceOrStructureBuilder parent, string name, bool annonymouns)
        : base(parent, name, annonymouns) {}
    
    
    public InstanceFieldBuilder AddField(string symbol)
    {
        var newField = new InstanceFieldBuilder(this, symbol, false);
        Fields.Add(newField);
        return newField;
    }
    public StaticFieldBuilder AddStaticField(string symbol)
    {
        var newField = new StaticFieldBuilder(this, symbol, false);
        StaticFields.Add(newField);
        return newField;
    }
    public FunctionBuilder AddFunction(string symbol)
    {
        var newFunction = new FunctionBuilder(this, symbol, false);
        Functions.Add(newFunction);
        return newFunction;
    }
    public VirtualFunctionBuilder AddVirtualFunction(string symbol, uint index)
    {
        var newFunction = new VirtualFunctionBuilder(this, symbol, index, false);
        Functions.Add(newFunction);
        return newFunction;
    }
    
    public InstanceFieldBuilder AddAnnonymousField(string symbol)
    {
        var newField = new InstanceFieldBuilder(this, symbol, true);
        Fields.Add(newField);
        return newField;
    }
    public FunctionBuilder AddAnnonymousFunction(string symbol)
    {
        var newFunction = new FunctionBuilder(this, symbol, true);
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
        if (VTableSize != null) sb.AppendLine($"\t(vtablelength {VTableSize.Value})");
        
        foreach (var i in StaticFields) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in Fields) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in Functions) sb.AppendLine(i.ToString().TabAllLines());
        sb.Length -= Environment.NewLine.Length;
        sb.Append(')');
        
        return sb.ToString();
    }
    public override string ToReadableReference() => '"' + string.Join('.', GlobalIdentifier) + '"';
}
