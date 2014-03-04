using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation.GoToRelated;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace ReSharper.Xao
{
    [RelatedFilesProvider(typeof(KnownProjectFileType))]
    public class ViewModelRelatedFilesProvider : IRelatedFilesProvider
    {
        public IEnumerable<JetTuple<IProjectFile, string, IProjectFile>> GetRelatedFiles(IProjectFile projectFile)
        {
            string viewName = projectFile.Name.Substring(0, projectFile.Name.LastIndexOf("View", StringComparison.Ordinal));
            IEnumerable<IClrDeclaredElement> searchResults = FindType(projectFile.GetSolution(), viewName + "ViewModel");
            IEnumerable<IPsiSourceFile> sourceFiles = searchResults.SelectMany(element => element.GetSourceFiles());

            var elementCollector = new RecursiveElementCollector<ITypeDeclaration>();
            foreach (IFile file in sourceFiles.SelectMany(psiSourceFile => psiSourceFile.EnumerateDominantPsiFiles()))
            {
                elementCollector.ProcessElement(file);
            }

            return elementCollector.GetResults()
                                   .Select(declaration => declaration.GetSourceFile().ToProjectFile())
                                   .Select(file => JetTuple.Of(file, "ViewModel", projectFile));
        }

        private static IEnumerable<IClrDeclaredElement> FindType(ISolution solution, string typeToFind)
        {
            ISymbolScope declarationsCache = solution.GetPsiServices().Symbols
                .GetSymbolScope(LibrarySymbolScope.FULL, context: UniversalModuleReferenceContext.Instance, caseSensitive: false);
            
            List<IClrDeclaredElement> results = declarationsCache.GetElementsByShortName(typeToFind).ToList();
            return results;
        }
    }
}