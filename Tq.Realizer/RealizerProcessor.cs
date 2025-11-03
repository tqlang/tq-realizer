using Tq.Realizer.Builder;
using Tq.Realizer.Builder.Language;
using Tq.Realizer.Builder.ProgramMembers;
using Tq.Realizer.Compiler;
using Tq.Realizer.Core.Configuration.LangOutput;
using Tq.Realizer.Optimization;

namespace Tq.Realizer;

public class RealizerProcessor
{

    public string? DebugDumpPath { get; set; } = null;
    public bool Verbose { get; set; } = false;

    private ProgramBuilder program;
    private ILanguageOutputConfiguration configuration;
    private bool processRunning = false;
    private int stage = 0;
    
    private List<FunctionBuilder> functions = [];
    private List<StaticFieldBuilder> fields = [];
    private List<StructureBuilder> structs = [];
    private List<TypeDefinitionBuilder> typedefs = [];

    public void SelectProgram(ProgramBuilder program) => this.program = program;
    public void SelectConfiguration(ILanguageOutputConfiguration config) => configuration = config;

    public void Start()
    {
        if (processRunning) throw new Exception("Already running");
        processRunning = true;
        
        if (DebugDumpPath != null)
            File.WriteAllTextAsync(Path.Combine(DebugDumpPath, "Stage0.txt"), program.ToString());
        
        UnwrapRecursive(program);
        
        Optimize(OptimizationOption.ExpandMacros);
        Optimize(OptimizationOption.Unnest);
        Optimize(OptimizationOption.PackStructures);
    }
    public ProgramBuilder Compile()
    {
        stage++;
        
        LanguageOutput compileTo = configuration switch
        {
            AlphaOutputConfiguration => LanguageOutput.Alpha,
            BetaOutputConfiguration => LanguageOutput.Beta,
            OmegaOutputConfiguration => LanguageOutput.Omega,
            _ => throw new ArgumentOutOfRangeException(),
        };
        if (Verbose) Console.WriteLine($"Realizer: Compiling to {compileTo}...");

        foreach (var function in functions)
        {
            foreach (var (i, b) in function.CodeBlocks.ToArray().Index())
            {
                if (b is not IntermediateBlockBuilder @block) throw new Exception("Invalid block type");
                
                function.CodeBlocks[i] = compileTo switch
                {
                    LanguageOutput.Omega => OmegaCompiler.CompileBlock(block, (OmegaOutputConfiguration)configuration),
                    
                    LanguageOutput.Alpha or 
                        LanguageOutput.Beta or
                    _ => throw new NotImplementedException()
                };
            }
        }

        if (DebugDumpPath != null)
            File.WriteAllTextAsync(Path.Combine(DebugDumpPath, $"Stage{stage}.txt"), program.ToString());
        
        return program;
    }
    
    private void Optimize(OptimizationOption optimization)
    {
        stage++;
        if (Verbose) Console.WriteLine($"Realizer: Optimizing ({optimization})...");
        
        switch (optimization)
        {
            case OptimizationOption.PackStructures:
                ObjectBaker.BakeTypeMetadata(configuration, [.. structs], [.. typedefs]);
                break;
            
            case OptimizationOption.ExpandMacros:
                foreach (var function in functions) MacroExpander.ExpandFunctionMacros(function, configuration);
                break;
            
            case OptimizationOption.Unnest:
                Unnester.UnnestProgram(program);
                break;
            
            default: throw new ArgumentOutOfRangeException(nameof(optimization), optimization, null);
        }
        
        if (DebugDumpPath != null)
            File.WriteAllTextAsync(Path.Combine(DebugDumpPath, $"Stage{stage}.txt"), program.ToString());
    }
    
    
    private void UnwrapRecursive(ProgramBuilder program)
    {
        foreach (var module in program.Modules) UnwrapRecursive(module);
    }
    private void UnwrapRecursive(ProgramMemberBuilder member)
    {
        switch (member)
        {
            case ImportedFunctionBuilder: break;
            
            case NamespaceBuilder @m:
                foreach (var i in m.Namespaces) UnwrapRecursive(i);
                foreach (var i in m.Functions) UnwrapRecursive(i);
                foreach (var i in m.Fields) UnwrapRecursive(i);
                foreach (var i in m.TypeDefinitions) UnwrapRecursive(i);
                foreach (var i in m.Structures) UnwrapRecursive(i);
                break;
            
            case FunctionBuilder @f:
                if (f.CodeBlocks.Count == 0) break;
                Unwrapper.UnwerapFunction(f);
                functions.Add(f);
                break;
            
            case StructureBuilder @s:
                structs.Add(@s);
                foreach (var f in s.Functions) UnwrapRecursive(f);
                break;
            
            case TypeDefinitionBuilder @t:
                typedefs.Add(@t);
                //TODO
                break;
            
            case StaticFieldBuilder @f: 
                fields.Add(f);
                break;
            
            default: throw new NotImplementedException();
        }
    }
    
    

    private enum LanguageOutput
    {
        Alpha,
        Beta,
        Omega,
    }
    public enum OptimizationOption
    {
        NormalizeMemoryStack,
        ExpandMacros,
        Unnest,
        PackStructures,
    }
    
}
