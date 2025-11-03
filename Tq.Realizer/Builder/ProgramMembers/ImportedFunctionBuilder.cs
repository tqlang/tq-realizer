using System.IO.Pipes;
using System.Text;

namespace Tq.Realizer.Builder.ProgramMembers;

public class ImportedFunctionBuilder: BaseFunctionBuilder
{
    public string? ImportDomain { get; set; }
    public string? ImportSymbol { get; set; }
   

    internal ImportedFunctionBuilder(INamespaceOrStructureBuilder parent, string name)
        : base(parent, name, false) { }
    
    
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"(func \"{Symbol}\"");
        foreach (var (name, type) in Parameters) sb.Append($" (param \"{name}\" {type})");
        if (ReturnType != null) sb.Append($" (ret {ReturnType})");
        
        if (ImportDomain != null && ImportSymbol != null) sb.Append($" (import \"{ImportDomain}\" \"{ImportSymbol}\")");
        else if (ImportSymbol != null) sb.Append($" (import \"{ImportSymbol}\")");
        else sb.Append($" (import nullptr)");
        
        return sb.ToString();
    }
}
