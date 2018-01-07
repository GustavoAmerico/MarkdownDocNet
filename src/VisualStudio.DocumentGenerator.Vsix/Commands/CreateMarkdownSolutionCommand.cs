using System.ComponentModel.Design;

namespace MarkdownVsix
{
    internal class CreateMarkdownSolutionCommand : CreateMarkdownProjectCommand
    {
        internal CreateMarkdownSolutionCommand(GenerateMarkdownPackage package)
            : base(package, new CommandID(Constants.PackageGuids.SymbolGenDocSolutionNodeGroup, Constants.PackageIds.CmdIDSymbolGenDocSolutionNodeGroup))
        {
        }
    }
}