namespace Tq.Realizer.Core.Configuration.LangOutput;

public interface ILanguageOutputConfiguration
{
    public bool BakeGenerics { get; init; }
    public bool UnnestMembers { get; init; }
    
    
    public byte MemoryUnit { get; init; }
    public byte NativeIntegerSize { get; init; }
}