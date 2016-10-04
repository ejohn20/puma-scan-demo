using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace MyAwesomeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class AdditionalFilesAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "DEMO2";
        private static readonly string Title = "Debug Build Enabled";
        private static readonly string MessageFormat = "Debug compilation is enabled. {0}({1}): {2}";
        private static readonly string Description = "Debugging reveals too much information about the app.";
        private const string Category = "Misconfiguration";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(Id, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(AnalyzeCompilation);
        }

        private void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            //Get the additional content files (.aspx, .cshtml, .config, etc.)
            ImmutableArray<AdditionalText> files = context.Options.AdditionalFiles;

            //Return if we don't have any extra files
            if (files.Length == 0)
                return;

            IEnumerable<AdditionalText> configFiles = files.Where(f => string.Compare(Path.GetExtension(f.Path), ".config") == 0).ToList();

            if (configFiles.Count() == 0)
                return;

            foreach (AdditionalText src in configFiles)
            {
                XDocument doc = XDocument.Load(src.Path, LoadOptions.SetLineInfo);

                //"configuration/system.web/compilation"
                XElement element = doc.Descendants("compilation").FirstOrDefault();

                if (element == null)
                    continue;

                XAttribute attribute = element.Attribute("debug");

                if (attribute == null)
                    continue;

                if(attribute.Value == "true")
                {
                    IXmlLineInfo lineInfo = element as IXmlLineInfo;
                    context.ReportDiagnostic(Diagnostic.Create(Rule, Location.None, src.Path, lineInfo.LineNumber, element.ToString()));
                }
            }            
        }
    }
}
