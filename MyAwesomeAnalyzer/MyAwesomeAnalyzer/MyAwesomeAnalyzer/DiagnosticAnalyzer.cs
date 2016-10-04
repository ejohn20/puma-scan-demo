using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MyAwesomeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MyAwesomeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "DEMO1";
        private static readonly string Title = "Weak Password Length";
        private static readonly string MessageFormat = "The ASP.NET Identity PasswordValidator does not meet the 12 character requirement.";
        private static readonly string Description = "The minimum password length is 12 characters.";
        private const string Category = "Password Management";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(Id, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeContext, SyntaxKind.ObjectCreationExpression);
        }

        private static void AnalyzeContext(SyntaxNodeAnalysisContext context)
        {
            var statement = context.Node as ObjectCreationExpressionSyntax;

            if (string.Compare(statement?.Type.ToString(), "PasswordValidator", StringComparison.Ordinal) != 0)
                return;

            var symbol = context.SemanticModel.GetSymbolInfo(statement).Symbol as ISymbol;
            if (string.Compare(symbol?.ContainingNamespace.ToString(), "Microsoft.AspNet.Identity", StringComparison.Ordinal) != 0)
                return;

            var initializer = statement.Initializer as InitializerExpressionSyntax;
            if (initializer?.Expressions.Count == 0)
                return;

            //Accouting for the inline assignment expression
            int minLength = 0;
            foreach (AssignmentExpressionSyntax expression in initializer.Expressions)
            {
                //Get the property value
                var value = context.SemanticModel.GetConstantValue(expression.Right);

                //If we have a value, set it to the local var
                if (value.HasValue && expression.Left.ToString().Equals("RequiredLength"))
                    minLength = (int)value.Value;
            }

            //Warn if length < 12 chars
            if (minLength < 12)
            {
                var diagnostic = Diagnostic.Create(Rule, statement.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}