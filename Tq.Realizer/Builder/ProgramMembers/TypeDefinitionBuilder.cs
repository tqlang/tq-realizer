using System.Text;
using Tq.Realizer.Core.Intermediate.Values;

namespace Tq.Realizer.Builder.ProgramMembers;

public class TypeDefinitionBuilder: TypeBuilder, INamespaceOrStructureOrTypedefBuilder
{
    
    internal TypeDefinitionBuilder(INamespaceOrStructureBuilder parent, string name) : base(parent, name) {}
    
    internal List<(string key, RealizerConstantValue? value)> _namedEntries = [];
    internal List<BaseFunctionBuilder> _functions = [];

    public (string key, RealizerConstantValue? value)[] NamedEntries => [.. _namedEntries];
    public BaseFunctionBuilder[] Functions => [.. _functions];
    public ProgramMemberBuilder[] GetMembers() => [.. _functions];
    
    public void AddNamedEntry(string key, RealizerConstantValue? value = null) => _namedEntries.Add((key, value));
    public FunctionBuilder AddFunction(string symbol, bool isStatic)
    {
        var newFunction = new FunctionBuilder(this, symbol, isStatic);
        _functions.Add(newFunction);
        return newFunction;
    }
    
    
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"(typedef \"{Symbol}\"");
        foreach (var i in NamedEntries)
        {
            if (i.value != null) sb.Append($"\n\t(\"{i.key}\" {i.value})");
            else sb.Append($"\n\t(\"{i.key}\")");
        }
        sb.Append(')');
        
        return sb.ToString();
    }
    public override string ToReadableReference() => '"' + string.Join('.', GlobalIdentifier) + '"';
}
