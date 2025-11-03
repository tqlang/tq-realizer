namespace Abstract.Realizer.Core.Intermediate;

public enum BinaryOperation
{
    Add, AddWarp, AddSaturate,
    Sub, SubWarp, SubSaturate,
    Mul,
    Div,
    Rem,
    
    BitAnd,
    BitOr,
    BitXor,
}

public enum CompareOperation
{
    Equals,
    NotEquals,
    Greater,
    Lesser,
    GreatherEquals,
    LesserEquals,
}