namespace Tq.Realizer.Core.Configuration.LangOutput;

public interface ILanguageOutputConfiguration
{
    public bool BakeGenerics { get; init; }
    public bool UnnestMembers { get; init; }
    
    
    public byte MemoryUnit { get; init; }
    public byte NativeIntegerSize { get; init; }
    
    public GenericAllowedFeatures GenericAllowedFeatures { get; init; }
}

[Flags]
public enum GenericAllowedFeatures
{
    None = 0,
    All = UseLdSelfInsteadArg0,
    
    UseLdSelfInsteadArg0 = 1 << 0,
}