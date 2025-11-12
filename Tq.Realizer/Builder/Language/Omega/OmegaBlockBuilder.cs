using System.Numerics;
using System.Text;
using Tq.Realizer.Builder.ProgramMembers;
using Tq.Realizer.Builder.References;
using Tq.Realizer.Core.Intermediate.Values;
using ProgramMembers_TypeBuilder = Tq.Realizer.Builder.ProgramMembers.TypeBuilder;
using TypeBuilder = Tq.Realizer.Builder.ProgramMembers.TypeBuilder;

namespace Tq.Realizer.Builder.Language.Omega;

public class OmegaBlockBuilder: BlockBuilder
{
    private List<IOmegaInstruction> _instructions = [];
    public List<IOmegaInstruction> InstructionsList => _instructions;
    public InstructionWriter Writer => new(this);

    internal OmegaBlockBuilder(FunctionBuilder parent, string name, uint idx) : base(parent, name, idx) {}


    public bool IsBlockFinished() => _instructions.LastOrDefault(e => e is IOmegaFinishInstruction) != null;
    public override string DumpInstructionsToString()
    {
        var sb = new StringBuilder();
        var instQueue = new Queue<IOmegaInstruction>(_instructions);
        
        while (instQueue.Count > 0) sb.Append(WriteInstruction(instQueue, false));
        
        // sb.AppendLine();
        // var newline = true;
        // foreach (var i in _instructions)
        // {
        //     if (newline) sb.Append(";; ");
        //     newline = false;
        //     sb.Append(i);
        //    
        //     if (i is IOmegaFlag) continue;
        //     sb.AppendLine();
        //     newline = true;
        // }
        
        if (sb.Length > Environment.NewLine.Length) sb.Length -= Environment.NewLine.Length;
        return sb.ToString();
    }

    
    private string WriteInstruction(Queue<IOmegaInstruction> instQueue, bool recursive)
    {
        var sb = new StringBuilder();
        
        var a = instQueue.Peek();
        switch (a)
        {
            case MacroDefineLocal @dl:
                instQueue.Dequeue();
                sb.Append($"$DEFINE_LOCAL {dl.Type}");
                break;
            
            case InstNop:
                instQueue.Dequeue();
                sb.Append("nop");
                break;

            case InstRet @r:
                instQueue.Dequeue();
                sb.Append("ret");
                if (r.value) sb.Append(" " + WriteInstructionValue(instQueue).TabAllLines()[1..]);
                break;
            
            case InstStLocal @stlocal:
            {
                instQueue.Dequeue();
                if (stlocal.index < 0) sb.Append($"(arg ${(-stlocal.index) - 1}) = ");
                else sb.Append($"(local {stlocal.index}) = ");
                sb.Append(WriteInstructionValue(instQueue).TabAllLines().TrimStart('\t'));
            } break;
            case InstStLocalRef @stlocref:
            {
                instQueue.Dequeue();
                if (stlocref.index < 0) sb.Append($"(arg ${(-stlocref.index) - 1})* = ");
                else sb.Append($"(local {stlocref.index})* = ");
                sb.Append(WriteInstructionValue(instQueue).TabAllLines().TrimStart('\t'));
            } break;
            
            case InstStStaticField @stfld:
            {
                instQueue.Dequeue();
                sb.Append($"{stfld.StaticField.ToReadableReference()} = ");
                sb.Append(WriteInstructionValue(instQueue).TabAllLines().TrimStart('\t'));
            } break;
            case InstStField @stfld:
            {
                instQueue.Dequeue();
                sb.Append($"{stfld.StaticField.ToReadableReference()} = ");
                sb.Append(WriteInstructionValue(instQueue).TabAllLines().TrimStart('\t'));
            } break;
            
            case InstBranch @b: instQueue.Dequeue(); sb.Append($"branch \"{Parent.CodeBlocks[(int)b.To].Name}\""); break;
            case InstBranchIf @b: instQueue.Dequeue(); sb.Append(
                $"(branch {WriteInstructionValue(instQueue)} ? \"{Parent.CodeBlocks[(int)b.IfTrue].Name}\"" +
                $" : \"{Parent.CodeBlocks[(int)b.IfFalse].Name}\"");
                break;

            default:
                sb.Append(WriteInstructionValue(instQueue).TrimStart('\t'));
                break;
        }
        if (!recursive) sb.AppendLine();

        return sb.ToString();
    }
    private string WriteInstructionValue(Queue<IOmegaInstruction> instQueue)
    {
        if (instQueue.Count == 0) return "<eof>";
        var sb = new StringBuilder();
        
        var a = instQueue.Dequeue();
        switch (a)
        {
            case InstInc: sb.Append($"inc {WriteInstructionValue(instQueue)}"); break;
            case InstDec: sb.Append($"dec {WriteInstructionValue(instQueue)}"); break;
            
            case InstNot: sb.Append($"!{WriteInstructionValue(instQueue)}"); break;
            case InstAdd: sb.Append($"{WriteInstructionValue(instQueue)} + " +
                                    $"{WriteInstructionValue(instQueue)})"); break;
            case InstSub: sb.Append($"{WriteInstructionValue(instQueue)} - " +
                                    $"{WriteInstructionValue(instQueue)})"); break;
            case InstMul: sb.Append($"{WriteInstructionValue(instQueue)} * " +
                                    $"{WriteInstructionValue(instQueue)})"); break;
            case InstAnd: sb.Append($"{WriteInstructionValue(instQueue)} & " +
                                    $"{WriteInstructionValue(instQueue)})"); break;
            case InstOr:  sb.Append($"{WriteInstructionValue(instQueue)} | " +
                                    $"{WriteInstructionValue(instQueue)})"); break;
            case InstXor: sb.Append($"{WriteInstructionValue(instQueue)} ^ " +
                                    $"{WriteInstructionValue(instQueue)})"); break;
            
            case InstCmpEq:  sb.Append($"{WriteInstructionValue(instQueue)} == " +
                                       $"{WriteInstructionValue(instQueue)}"); break;
            case InstCmpNeq: sb.Append($"{WriteInstructionValue(instQueue)} != " +
                                       $"{WriteInstructionValue(instQueue)}"); break;
            case InstCmpGr: sb.Append($"{WriteInstructionValue(instQueue)} > " +
                                      $"{WriteInstructionValue(instQueue)})"); break;
            case InstCmpGe: sb.Append($"{WriteInstructionValue(instQueue)} >= " +
                                      $"{WriteInstructionValue(instQueue)}"); break;
            case InstCmpLr: sb.Append($"{WriteInstructionValue(instQueue)} < " +
                                      $"{WriteInstructionValue(instQueue)}"); break;
            case InstCmpLe: sb.Append($"{WriteInstructionValue(instQueue)} <= " +
                                      $"{WriteInstructionValue(instQueue)}"); break;
            
            
            case InstSigcast @s: sb.Append("(sigcast."
                                           + (s.Signed ? 's' : 'u')
                                           + $" {WriteInstructionValue(instQueue)}"); break;
            case InstTrunc @t: sb.Append($"trunc {WriteInstructionValue(instQueue)}"); break;
            case InstExtend @e: sb.Append($"extend {WriteInstructionValue(instQueue)}"); break;
            case InstConv @c: sb.Append($"conv {WriteInstructionValue(instQueue)}"); break;
            
            case InstLdConst @c: sb.Append($"{c.Value}"); break;
            
            case InstLdSlice @slice: sb.Append($"(slice ${slice.Index})"); break;
            
            case InstLdLocal @ldl:
                if (ldl.Local < 0) sb.Append($"(arg {(-ldl.Local)-1})");
                else sb.Append($"(local {ldl.Local})");
                break;
            case InstLdLocalRef @ldr:
                if (ldr.Local < 0) sb.Append($"(arg.ref {(-ldr.Local)-1})");
                else sb.Append($"&(local {ldr.Local})");
                break;
            case InstLdStaticField @ldf: sb.Append($"{ldf.StaticField.ToReadableReference()}"); break;
            case InstLdField @acf: sb.Append($"{acf.StaticField.ToReadableReference()}"); break;
            
            case InstLdTypeRefOf: sb.Append($"(typeof {WriteInstructionValue(instQueue)})"); break;
            
            case InstLdSelf: sb.Append("ld.self"); break;
            
            case InstCall @call:
            {
                sb.Append($"call {call.function.ToReadableReference()}");
                foreach (var i in call.function.Parameters)
                    sb.Append(" " + WriteInstructionValue(instQueue));
            } break;
            
            case InstLdNewObject @newobj:
                sb.Append($"new {newobj.Type.ToReadableReference()}");
                break;
            
            case FlagTypeInt @tint:
                sb.Append("(" + (tint.Signed ? "s" : "u") + (tint.Size.HasValue ? $"{tint.Size}" : "ptr") + ')');
                sb.Append("(" + WriteInstructionValue(instQueue) + ")");
                break;
            case FlagTypeFloat @tflo:
                sb.Append($"f{tflo.Size})");
                sb.Append("(" + WriteInstructionValue(instQueue) + ")");
                break;
            
            default:
                sb.Append($";; uninplemented {a}");
                break;
        }

        if (instQueue.Count > 0 && instQueue.Peek() 
            is InstLdField
            or InstStField)
        {
            sb.Append($"->{WriteInstruction(instQueue, true)}");
        }

        return sb.ToString();
    }
    

    public struct InstructionWriter
    {
        private OmegaBlockBuilder _parentBuilder;
        internal InstructionWriter(OmegaBlockBuilder builder) => _parentBuilder = builder;

        private InstructionWriter AddAndReturn(IOmegaInstruction value)
        {
            if (value is IOmegaRequiresTypePrefix)
            {
                var c = _parentBuilder._instructions.Count - 1;
                while (c >= 0 && _parentBuilder._instructions[c] is IOmegaCompoundPrefix) c--;
                
                if (_parentBuilder._instructions[c] is not IOmegaTypePrefix)
                    throw new Exception($"Instruction '{value}' expects type prefix");
            }

            _parentBuilder._instructions.Add(value);
            return this;
        }

        public InstructionWriter Nop() => AddAndReturn(new InstNop());
        public InstructionWriter Invalid() => AddAndReturn(new InstInvalid());
        public InstructionWriter Call(BaseFunctionBuilder r) => AddAndReturn(new InstCall(r));
        public InstructionWriter CallVirt() => AddAndReturn(new InstCallvirt());
        public InstructionWriter Ret(bool value) => AddAndReturn(new InstRet(value));
        
        public InstructionWriter Inc() => AddAndReturn(new InstDec());
        public InstructionWriter Dec() => AddAndReturn(new InstDec());
        public InstructionWriter Add() => AddAndReturn(new InstAdd());
        public InstructionWriter Sub() => AddAndReturn(new InstSub());
        public InstructionWriter Mul() => AddAndReturn(new InstMul());
        public InstructionWriter Div() => AddAndReturn(new InstDiv());
        public InstructionWriter Rem() => AddAndReturn(new InstRem());
        public InstructionWriter Neg() => AddAndReturn(new InstNeg());
        public InstructionWriter Not() => AddAndReturn(new InstNot());
        public InstructionWriter And() => AddAndReturn(new InstAnd());
        public InstructionWriter Or() => AddAndReturn(new InstOr());
        public InstructionWriter Xor() => AddAndReturn(new InstXor());
        public InstructionWriter Shr() => AddAndReturn(new InstShr());
        public InstructionWriter Shl() => AddAndReturn(new InstShl());
        public InstructionWriter Ror() => AddAndReturn(new InstRor());
        public InstructionWriter Rol() => AddAndReturn(new InstRol());
        
        public InstructionWriter Branch(uint to) => AddAndReturn(new InstBranch(to));
        public InstructionWriter BranchIf(uint iftrue, uint iffalse) => AddAndReturn(new InstBranchIf(iftrue, iffalse));
        
        public InstructionWriter LdConst(RealizerConstantValue value) => AddAndReturn(new InstLdConst(value));
        public InstructionWriter LdNewSlice() => AddAndReturn(new InstLdNewSlice());
        public InstructionWriter LdSlice(int index) => AddAndReturn(new InstLdSlice(index));
        public InstructionWriter LdNewObject(StructureBuilder r) => AddAndReturn(new InstLdNewObject(r));
        
        public InstructionWriter LdLocal(short index) => AddAndReturn(new InstLdLocal(index));
        public InstructionWriter LdLocalRef(short index) => AddAndReturn(new InstLdLocalRef(index));
        
        public InstructionWriter LdField(StaticFieldBuilder r) => AddAndReturn(new InstLdStaticField(r));
        public InstructionWriter LdField(InstanceFieldBuilder r) => AddAndReturn(new InstLdField(r));
        public InstructionWriter LdFieldRef() => AddAndReturn(new InstLdFieldRef());
        public InstructionWriter LdIndex() => AddAndReturn(new InstLdIndex());
        public InstructionWriter LdFuncRef(FunctionBuilder funcref) => AddAndReturn(new InstLdFuncRef(funcref));
        public InstructionWriter LdTypeRef(ProgramMembers_TypeBuilder typeref) => AddAndReturn(new InstLdTypeRef(typeref));
        public InstructionWriter LdTypeRefOf() => AddAndReturn(new InstLdTypeRefOf());
        public InstructionWriter LdSelf() => AddAndReturn(new InstLdSelf());
        public InstructionWriter LdMeta(OmegaMetadataKind kind) => AddAndReturn(new InstLdMeta(kind));
        
        public InstructionWriter StLocal(short index) => AddAndReturn(new InstStLocal(index));
        public InstructionWriter StLocalRef(short index) => AddAndReturn(new InstStLocalRef(index));
        public InstructionWriter StField(StaticFieldBuilder r) => AddAndReturn(new InstStStaticField(r));
        public InstructionWriter StField(InstanceFieldBuilder r) => AddAndReturn(new InstStField(r));
        public InstructionWriter StIndex() => AddAndReturn(new InstStIndex());
        
        public InstructionWriter Conv() => AddAndReturn(new InstConv());
        public InstructionWriter Extend() => AddAndReturn(new InstExtend());
        public InstructionWriter Trunc() => AddAndReturn(new InstTrunc());
        public InstructionWriter Sigcast(bool signess) => AddAndReturn(new InstSigcast(signess));
        public InstructionWriter Bitcast() => AddAndReturn(new InstBitcast());

        public InstructionWriter CmpEq() => AddAndReturn(new InstCmpEq());
        public InstructionWriter CmpNeq() => AddAndReturn(new InstCmpNeq());
        public InstructionWriter CmpGr() => AddAndReturn(new InstCmpGr());
        public InstructionWriter CmpLr() => AddAndReturn(new InstCmpLr());
        public InstructionWriter CmpGe() => AddAndReturn(new InstCmpGe());
        public InstructionWriter CmpLe() => AddAndReturn(new InstCmpLe());
        
        public InstructionWriter MemCopy() => AddAndReturn(new InstMemCopy());
        public InstructionWriter MemFill() => AddAndReturn(new InstMemFill());
        public InstructionWriter MemEq() => AddAndReturn(new InstMemEq());
        
        public InstructionWriter AllowOvf() => AddAndReturn(new FlagAllowOvf());
        public InstructionWriter Saturated() => AddAndReturn(new FlagSaturated());
        public InstructionWriter AllowNil() => AddAndReturn(new FlagAllowNil());
        
        public InstructionWriter TypeInt(bool signed, byte? size) => AddAndReturn(new FlagTypeInt(signed, size));
        public InstructionWriter TypeFloat(byte size) => AddAndReturn(new FlagTypeFloat(size));
        public InstructionWriter TypeObj() => AddAndReturn(new FlagTypeObject());
        public InstructionWriter TypeAny() => AddAndReturn(new FlagTypeAny());
        public InstructionWriter TypeReference() => AddAndReturn(new FlagTypeReference());
        
        public InstructionWriter MacroDefineLocal(TypeReference typer) => AddAndReturn(new MacroDefineLocal(typer));
    }
}
