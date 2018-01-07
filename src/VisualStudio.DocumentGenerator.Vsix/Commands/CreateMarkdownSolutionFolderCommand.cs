using System.ComponentModel.Design;

namespace MarkdownVsix
{
    internal class CreateMarkdownSolutionFolderCommand : CreateMarkdownProjectCommand
    {
        internal CreateMarkdownSolutionFolderCommand(GenerateMarkdownPackage package)
            : base(package, new CommandID(Constants.PackageGuids.SymbolGenDocSolutionFolderGroup, Constants.PackageIds.CmdIDSymbolGenDocSolutionFolderGroup))
        {
        }
    }
}