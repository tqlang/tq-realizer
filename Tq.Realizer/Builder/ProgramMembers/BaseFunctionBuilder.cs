using System.Text;
using Tq.Realizer.Builder.References;

namespace Tq.Realizer.Builder.ProgramMembers;

public abstract class BaseFunctionBuilder: ProgramMemberBuilder
{
    public List<(string name, TypeReference type)> Parameters = [];
    public TypeReference? ReturnType = null;
    
    public readonly bool IsStatic;

    internal BaseFunctionBuilder(INamespaceOrStructureOrTypedefBuilder parent, string name, bool isStatic)
        : base(parent, name)
    {
        IsStatic = isStatic;
    }
    
    
    public int AddParameter(string name, TypeReference typeReference)
    {
        Parameters.Add((name, typeReference));
        return Parameters.Count - 1;
    }
    
    public override string ToReadableReference()
    {
        var sb = new StringBuilder();

        sb.Append('"');
        sb.AppendJoin('.', GlobalIdentifier);
        sb.Append('"');
        sb.Append(" (");
        sb.AppendJoin(", ", Parameters.Select(e => e.type.ToString()));
        sb.Append(')');
        sb.Append(ReturnType == null ? " void" : $" {ReturnType}");
        
        return sb.ToString();
    }
}