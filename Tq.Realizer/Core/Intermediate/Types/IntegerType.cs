namespace Tq.Realizer.Core.Intermediate.Types;

internal class IntegerType(bool signed, byte? size) : RealizerType
{
    public readonly bool Signed = signed;
    public readonly byte? Size = size;
    
    public override string ToString() => (Signed ? "s" : "u") + (Size.HasValue ? $"{Size}" : "");
}
