namespace Tq.Realizer.Core.Intermediate.Types;

internal class SliceType(RealizerType elementT) : RealizerType
{
    public readonly RealizerType ElementType = elementT;
    public override string ToString() => $"[]{ElementType}";
}