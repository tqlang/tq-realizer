using Tq.Realizer.Builder.References;

namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrMacroDefineLocal(TypeReference tref): IrMacro
{
    public readonly TypeReference Type = tref;
    public override string ToString() => $"$DEFINE_LOCAL {Type}";
}
