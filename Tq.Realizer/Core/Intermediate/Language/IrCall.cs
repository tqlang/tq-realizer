using System.Text;
using Tq.Realizer.Builder.ProgramMembers;

namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrCall(BaseFunctionBuilder f, IrValue[] arguments) : IrValue
{
    public readonly BaseFunctionBuilder Function = f;
    public readonly IrValue[] Arguments = arguments;

    public override string ToString()
    {
        var sb = new StringBuilder();
        
        sb.Append($"(call {Function.ToReadableReference()}");
        foreach (var i in Arguments) sb.Append($"\n\t{i}");
        sb.Append(')');

        return sb.ToString();
    }
}
