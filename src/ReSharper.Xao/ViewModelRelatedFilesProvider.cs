using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation.GoToRelated;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace ReSharper.Xao
{
    [RelatedFilesProvider(typeof(KnownProjectFileType))]
    public class ViewModelRelatedFilesProvider : IRelatedFilesProvider
    {
        private enum FileKind
        {
            None,
            View,
            ViewModel
        }

        public IEnumerable<JetTuple<IProjectFile, string, IProjectFile>> GetRelatedFiles(IProjectFile projectFile)
        {
            FileKind kind = GetFileKind(projectFile);
            int suffixStart;
            switch (kind)
            {
                case FileKind.View:
                case FileKind.ViewModel:
                    suffixStart = projectFile.Name.LastIndexOf(kind.ToString(), StringComparison.OrdinalIgnoreCase);
                    break;
                default:
                    return EmptyList<JetTuple<IProjectFile, string, IProjectFile>>.InstanceList;
            }

            string typeName = projectFile.Name.Substring(0, suffixStart);
            string newSuffix = kind == FileKind.View ? "ViewModel" : "View";

            IEnumerable<IClrDeclaredElement> searchResults = FindType(projectFile.GetSolution(), typeName + newSuffix);
            IEnumerable<IPsiSourceFile> sourceFiles = searchResults.SelectMany(element => element.GetSourceFiles());

            var elementCollector = new RecursiveElementCollector<ITypeDeclaration>();
            foreach (IFile file in sourceFiles.SelectMany(psiSourceFile => psiSourceFile.EnumerateDominantPsiFiles()))
            {
                elementCollector.ProcessElement(file);
            }

            return elementCollector.GetResults()
                                   .Select(declaration => declaration.GetSourceFile().ToProjectFile())
                                   .Select(file => JetTuple.Of(file, newSuffix, projectFile));
        }

        private FileKind GetFileKind(IProjectFile projectFile)
        {
            IEnumerable<string> typeNamesInFile = GetTypeNamesDefinedInFile(projectFile).ToList();
            if (typeNamesInFile.Any(s => s.EndsWith("View", StringComparison.OrdinalIgnoreCase))) return FileKind.View;
            if (typeNamesInFile.Any(s => s.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase))) return FileKind.ViewModel;
            return FileKind.None;
        }

        private IEnumerable<string> GetTypeNamesDefinedInFile(IProjectFile projectFile)
        {
            IPsiSourceFile psiSourceFile = projectFile.ToSourceFile();
            if (psiSourceFile == null)
            {
                return EmptyList<string>.InstanceList;
            }

            return psiSourceFile.GetPsiServices().Symbols.GetTypesAndNamespacesInFile(psiSourceFile)
                                .OfType<ITypeElement>()
                                .Select(element => element.ShortName);
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