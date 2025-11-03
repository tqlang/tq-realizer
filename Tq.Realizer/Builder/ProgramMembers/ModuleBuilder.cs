using System.Text;

namespace Tq.Realizer.Builder.ProgramMembers;

public class ModuleBuilder: NamespaceBuilder
{
    public override ModuleBuilder Module => this;

    internal ModuleBuilder(string name) : base(null!, name) { }
    
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"(module \"{Symbol}\"");
        foreach (var i in namespaces) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in fields) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in functions) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in structures) sb.AppendLine(i.ToString().TabAllLines());
        foreach (var i in typedefs) sb.AppendLine(i.ToString().TabAllLines());
        sb.Length -= Environment.NewLine.Length;
        sb.Append(')');
        
        return sb.ToString();
    }
}
