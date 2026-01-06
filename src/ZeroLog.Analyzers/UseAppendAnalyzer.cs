using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ZeroLog.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseAppendAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor UseAppendDiagnostic = new(
        DiagnosticIds.UseAppend,
        "Use Append",
        "Use Append syntax to add structured data",
        DiagnosticIds.Category,
        DiagnosticSeverity.Info,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        UseAppendDiagnostic
    );

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    private static void AnalyzeCompilationStart(CompilationStartAnalysisContext compilationStartContext)
    {
        var compilation = (CSharpCompilation)compilationStartContext.Compilation;

        var logType = compilation.GetTypeByMetadataName(ZeroLogFacts.TypeNames.Log);
        var exceptionType = compilation.GetTypeByMetadataName(typeof(Exception).FullName!);

        if (logType is null || exceptionType is null)
            return;

        var immediateLogMethods = logType.GetMembers()
                                         .Where(i => i.Kind == SymbolKind.Method)
                                         .OfType<IMethodSymbol>()
                                         .Where(i => i is
                                         {
                                             ReturnsVoid: true,
                                             IsStatic: false,
                                             DeclaredAccessibility: Accessibility.Public
                                         })
                                         .Where(i => ZeroLogFacts.IsLogLevelName(i.Name))
                                         .Where(i => i.Parameters is [_] || i.Parameters is [_, { Type: { } secondParameterType }] && SymbolEqualityComparer.Default.Equals(secondParameterType, exceptionType))
                                         .ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

        if (immediateLogMethods.Count == 0)
            return;

        compilationStartContext.RegisterOperationAction(
            ctx => AnalyzeOperation(ctx, immediateLogMethods),
            OperationKind.Invocation
        );
    }

    private static void AnalyzeOperation(OperationAnalysisContext context, ImmutableHashSet<IMethodSymbol> immediateLogMethods)
    {
        var invocation = (IInvocationOperation)context.Operation;

        if (!immediateLogMethods.Contains(invocation.TargetMethod))
            return;

        if (invocation.Syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax expressionSyntax })
            context.ReportDiagnostic(Diagnostic.Create(UseAppendDiagnostic, expressionSyntax.Name.GetLocation()));
    }
}
