namespace Abstract.Realizer.Builder.References;

public class NoreturnTypeReference : TypeReference
{
    public override uint? Alignment { get => null; init {} }
    public override string ToString() => "noreturn";
}