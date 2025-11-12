using System.Text;
using Tq.Realizer.Builder.References;
using Tq.Realizer.Core.Intermediate.Values;

namespace Tq.Realizer.Builder.ProgramMembers;

public abstract class PropertyBuilder: ProgramMemberBuilder
{
    internal PropertyBuilder(INamespaceOrStructureOrTypedefBuilder parent, string symbol) : base(parent, symbol) { }
    
    public TypeReference? Type = null;
    public RealizerConstantValue? Initializer = null;
    
    public FunctionBuilder? Getter = null;
    public FunctionBuilder? Setter = null;
    
    public override string ToReadableReference() => $"\"{string.Join('.', GlobalIdentifier)}\"";
}
