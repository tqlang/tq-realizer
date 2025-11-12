using System.Text;
using Tq.Realizer.Builder.References;
using Tq.Realizer.Core.Intermediate.Values;

namespace Tq.Realizer.Builder.ProgramMembers;

public class StaticPropertyBuilder(INamespaceOrStructureOrTypedefBuilder parent, string symbol)
    : PropertyBuilder(parent, symbol)
{
    
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"(prop static \"{Symbol}\"");
        if (Getter != null) sb.Append($"\n\t(getter {Getter.ToReadableReference()})");
        if (Setter != null) sb.Append($"\n\t(setter {Setter.ToReadableReference()})");
        sb.Append(')');
        
        return sb.ToString();
    }
}
