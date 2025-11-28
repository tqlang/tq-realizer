using Tq.Realizeer.Core.Program;
using Tq.Realizeer.Core.Program.Builder;
using Tq.Realizeer.Core.Program.Member;
using Tq.Realizer.Compiler;
using Tq.Realizer.Core.Configuration.LangOutput;
using Tq.Realizer.Passes;

namespace Tq.Realizer;

public class RealizerProcessor
{

    private string? _debugDumpPath = null;
    public string? DebugDumpPath
    {
        get => _debugDumpPath;
        set => _debugDumpPath = value == null ? null : Path.GetFullPath(value);
    }
    public bool Verbose { get; set; } = false;

    private RealizerProgram program;
    private IOutputConfiguration configuration;
    private bool processRunning = false;
    private int stage = 0;
    
    private List<RealizerFunction> functions = [];
    private List<RealizerField> fields = [];
    private List<RealizerStructure> structs = [];
    private List<RealizerTypedef> typedefs = [];

    public void SelectProgram(RealizerProgram program)
    {
        if (processRunning) throw new InvalidOperationException("Already running");
        this.program = program;
    }
    public void SelectConfiguration(IOutputConfiguration config)
    {
        if (processRunning) throw new InvalidOperationException("Already running");
        configuration = config;
    }

    public void Start()
    {
        if (processRunning) throw new Exception("Already running");
        processRunning = true;

        TryDumpProgram("setup");

        stage++;
        Format.Pass(program, configuration);
        
        TryDumpProgram("format");

    }
    public RealizerProgram Compile()
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

        // foreach (var function in functions)
        // {
        //     foreach (var (i, b) in function.CodeBlocks.ToArray().Index())
        //     {
        //         if (b is not IntermediateBlockBuilder @block) throw new Exception("Invalid block type");
        //         
        //         function.CodeBlocks[i] = compileTo switch
        //         {
        //             LanguageOutput.Omega => OmegaCompiler.CompileBlock(block, (OmegaOutputConfiguration)configuration),
        //             
        //             LanguageOutput.Alpha or 
        //                 LanguageOutput.Beta or
        //             _ => throw new NotImplementedException()
        //         };
        //     }=
        // }
        
        TryDumpProgram("compile");
        return program;
    }

    private void TryDumpProgram(string op)
    {
        if (DebugDumpPath != null)
            File.WriteAllTextAsync(Path.Combine(DebugDumpPath, $"Stage{stage}-{op}.txt"), program.ToString());
    }
    
    private enum LanguageOutput
    {
        Alpha,
        Beta,
        Omega,
    }
    
}
