namespace Tq.Realizer.Core.Configuration.LangOutput;

public class OmegaOutputConfiguration : ILanguageOutputConfiguration
{
    public bool BakeGenerics { get; init; }
    public bool UnnestMembers { get; init; }
    
    public byte MemoryUnit { get; init; }
    public byte NativeIntegerSize { get; init; }
    
    public GenericAllowedFeatures GenericAllowedFeatures { get; init; }
    public OmegaAllowedFeatures OmegaAllowedFeatures { get; init; } 
}


[Flags]
public enum OmegaAllowedFeatures
{
    None = 0,
    All = 0,
}
