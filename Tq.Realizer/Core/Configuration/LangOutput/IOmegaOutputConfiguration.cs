namespace Tq.Realizer.Core.Configuration.LangOutput;

public class OmegaOutputConfiguration : ILanguageOutputConfiguration
{
    public bool BakeGenerics { get; init; }
    public bool UnnestMembers { get; init; }
    
    public byte MemoryUnit { get; init; }
    public byte NativeIntegerSize { get; init; }
    
    
    public OmegaInstructions AllowedInstructions { get; init; } 
}


[Flags]
public enum OmegaInstructions
{
    None = 0,
    All = Int32.MaxValue,
    
    LdSelf = 1 << 0,
}