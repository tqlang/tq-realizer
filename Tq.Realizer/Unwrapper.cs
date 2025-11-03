using System.Diagnostics;
using System.Text;
using Tq.Realizer.Builder.References;
using Tq.Realizer.Core.Configuration.LangOutput;
using Tq.Realizer.Core.Intermediate.Values;
using Tq.Realizer.Builder.Language;
using Tq.Realizer.Builder.Language.Omega;
using Tq.Realizer.Builder.ProgramMembers;
using Tq.Realizer.Core.Intermediate;
using Tq.Realizer.Core.Intermediate.Language;
using Tq.Realizer.Core.Intermediate.Types;

namespace Tq.Realizer;

internal static class Unwrapper
{
    // To be able to understand the code at high level,
    // it must unwrap the instruction into an lisp-like
    // intermediate representation.
    
    public static void UnwerapFunction(FunctionBuilder function)
    {
        List<IntermediateBlockBuilder> newblocks = [];
        newblocks.AddRange(function.CodeBlocks.Select(builder
            => new IntermediateBlockBuilder(function, builder.Name, builder.Index)));

        foreach (var (i, builder) in function.CodeBlocks.ToArray().Index())
        {
            var irblock = newblocks[i].Root;
        
            var queue = builder switch
            {
                OmegaBlockBuilder @omega => new Queue<IOmegaInstruction>(omega.InstructionsList),
                _ => throw new ArgumentException("function do not has a valid input bytecode!")
            };

            while (queue.Count > 0) irblock.content.Add(UnwrapInstruction(function, queue, newblocks));
            function.CodeBlocks[i] = newblocks[i];
        }
    }

    private static IrNode UnwrapInstruction(
        FunctionBuilder function,
        Queue<IOmegaInstruction> instructions,
        List<IntermediateBlockBuilder> newblocks)
    {
        var a = instructions.Peek();
        switch (a)
        {
            case MacroDefineLocal @mdl:
                instructions.Dequeue();
                return new IrMacroDefineLocal(mdl.Type);
            
            case InstStLocal stLocal:
                instructions.Dequeue();
                return new IrAssign(new IrLocal(stLocal.index), UnwrapValue(function, instructions, newblocks));
            
            case InstStField stField:
                instructions.Dequeue();
                return new IrAssign(new IrField(stField.StaticField), UnwrapValue(function, instructions, newblocks));
            
            case InstStStaticField stStaticField:
                instructions.Dequeue();
                return new IrAssign(new IrField(stStaticField.StaticField), UnwrapValue(function, instructions, newblocks));
            
            case InstRet @r:
                instructions.Dequeue();
                return new IrRet(r.value ? UnwrapValue(function, instructions, newblocks) : null);
                
            case InstBranch @b:
                instructions.Dequeue();
                return new IrBranch(newblocks[(int)b.To]);
            
            case InstBranchIf @b:
                instructions.Dequeue();
                return new IrBranchIf(UnwrapValue(function, instructions, newblocks),
                    newblocks[(int)b.IfTrue], newblocks[(int)b.IfFalse]);
            
            default:
            {
                IrNode r = UnwrapValue(function, instructions, newblocks);

                while (instructions.Count > 0 && instructions.Peek() is InstStField)
                {
                    var b = (IrAssign)UnwrapInstruction(function, instructions, newblocks);
                    var c = new IrAccess((IrValue)r, (IrValue)b.to);
                    r = new IrAssign(c, b.value);
                }

                return r;
            }
        }
    }

    private static IrValue UnwrapValue(FunctionBuilder function,
        Queue<IOmegaInstruction> instructions,
        List<IntermediateBlockBuilder> newblocks)
    {
        var a = instructions.Dequeue();
        var r = a switch
        {
            IOmegaRequiresTypePrefix => throw new Exception($"instruction \"{a}\" expects type prefix"),
            
            InstLdLocal @ldlocal => new IrLocal(ldlocal.Local),
            InstLdNewObject @newobj => new IrNewObj(newobj.Type),
            
            InstLdLocalRef @ldlocalref => new IrRefOf(new IrLocal(ldlocalref.Local)),
            
            InstLdTypeRefOf @ldtyperefof => new IrTypeOf(UnwrapValue(function, instructions, newblocks)),
            
            InstLdField @ldField => new IrField(ldField.StaticField),
            InstLdStaticField @ldStaticField => new IrField(ldStaticField.StaticField),
            
            InstLdConst @ldc1 => new IrConstValue(ldc1.Value),
            
            InstLdSlice @slice => new IrConstValue(function.DataBlocks[slice.Index]),
            
            InstLdSelf => new IrSelf(),
            
            InstCall @cal => new IrCall(cal.function, UnwrapValues(function, instructions, cal.function.Parameters.Count, newblocks)),
            
            IOmegaTypePrefix @tprefix => UnwrapValueFlagged(function, [tprefix], instructions, newblocks),
            
            _ => throw new UnreachableException(),
        };
                
        while (instructions.Count > 0 && instructions.Peek() is InstLdField)
        {
            var b = UnwrapValue(function, instructions, newblocks);
            r = new IrAccess(r, b);
            
        }

        return r;
    }

    private static IrValue UnwrapValueFlagged(FunctionBuilder function,
        IOmegaFlag[] flags,
        Queue<IOmegaInstruction> instructions,
        List<IntermediateBlockBuilder> newblocks)
    {

        while (instructions.Count > 0 && instructions.Peek() is IOmegaCompoundPrefix)
            flags = [..flags, (IOmegaCompoundPrefix)instructions.Dequeue()];

        RealizerType typeref = flags[0] switch
        {
            FlagTypeInt @typei => new IntegerType(typei.Signed, typei.Size),
            FlagTypeReference @typer => new ReferenceType(),
            _ => throw new UnreachableException(),
        };
        
        var a = instructions.Dequeue();
        return a switch
        {
            InstAdd => new IrBinaryOp(typeref,
                      flags switch
                      {
                          [_, FlagAllowOvf] => BinaryOperation.AddWarp,
                          [_, FlagSaturated] => BinaryOperation.AddSaturate,
                          _ => BinaryOperation.Add
                      },
                UnwrapValue(function, instructions, newblocks),
                UnwrapValue(function, instructions, newblocks)),

            InstSub => new IrBinaryOp(typeref,
                      flags switch
                      {
                          [_, FlagAllowOvf] => BinaryOperation.SubWarp,
                          [_, FlagSaturated] => BinaryOperation.SubSaturate,
                          _ => BinaryOperation.Sub
                      },
                UnwrapValue(function, instructions, newblocks),
                UnwrapValue(function, instructions, newblocks)),

            InstMul => new IrBinaryOp(typeref, BinaryOperation.Mul,
                UnwrapValue(function, instructions, newblocks),
                UnwrapValue(function, instructions, newblocks)),


            InstAnd => new IrBinaryOp(typeref, BinaryOperation.BitAnd,
                UnwrapValue(function, instructions, newblocks),
                UnwrapValue(function, instructions, newblocks)),

            InstOr => new IrBinaryOp(typeref, BinaryOperation.BitOr,
                UnwrapValue(function, instructions, newblocks),
                UnwrapValue(function, instructions, newblocks)),

            InstXor => new IrBinaryOp(typeref, BinaryOperation.BitXor,
                UnwrapValue(function, instructions, newblocks),
                UnwrapValue(function, instructions, newblocks)),


            InstCmpEq => new IrCmp(((IntegerType)typeref).Signed, CompareOperation.Equals,
                UnwrapValue(function, instructions, newblocks),
                UnwrapValue(function, instructions, newblocks)),
            
            InstCmpGr => new IrCmp(((IntegerType)typeref).Signed, CompareOperation.Greater,
                UnwrapValue(function, instructions, newblocks),
                UnwrapValue(function, instructions, newblocks)),
            
            InstCmpGe => new IrCmp(((IntegerType)typeref).Signed, CompareOperation.GreatherEquals,
                UnwrapValue(function, instructions, newblocks),
                UnwrapValue(function, instructions, newblocks)),
            
            InstCmpLr => new IrCmp(((IntegerType)typeref).Signed, CompareOperation.Lesser,
                UnwrapValue(function, instructions, newblocks),
                UnwrapValue(function, instructions, newblocks)),
            
            InstCmpLe => new IrCmp(((IntegerType)typeref).Signed, CompareOperation.LesserEquals,
                UnwrapValue(function, instructions, newblocks),
                UnwrapValue(function, instructions, newblocks)),


            InstConv => new IrConv(typeref, UnwrapValue(function, instructions, newblocks)),
            InstExtend => new IrExtend((IntegerType)typeref, UnwrapValue(function, instructions, newblocks)),
            InstTrunc => new IrTrunc((IntegerType)typeref, UnwrapValue(function, instructions, newblocks)),

            _ => throw new Exception($"Instruction \"{a}\" does not allows type prefix"),
        };
    }
    
    private static IrValue[] UnwrapValues(FunctionBuilder function,
        Queue<IOmegaInstruction> instructions,
        int count,
        List<IntermediateBlockBuilder> newblocks)
    {
        List<IrValue> values = [];
        for (var c = 0; c < count; c++) values.Add(UnwrapValue(function, instructions, newblocks));
        return [..values];
    }
}
