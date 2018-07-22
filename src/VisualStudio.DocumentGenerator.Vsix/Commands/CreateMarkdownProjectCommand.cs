using EnvDTE;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VisualStudio.DocumentGenerate.Vsix.Format.Markdown;

namespace MarkdownVsix
{
    /// <summary>A command that provides for cleaning up code in the selected documents.</summary>
    internal class CreateMarkdownProjectCommand : BaseCommand
    {
        /// <summary>Gets the list of selected project items.</summary>
        private IEnumerable<ProjectItem> SelectedProjectItems
        {
            get
            {
                return SolutionHelper.GetSelectedProjectItemsRecursively(Package);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateMarkdownProjectCommand"/> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        internal CreateMarkdownProjectCommand(GenerateMarkdownPackage package)
            : this(package, new CommandID(Constants.PackageGuids.SymbolGenDocProjectNodeGroup, Constants.PackageIds.CmdIDSymbolGenDocProjectNodeGroup))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateMarkdownProjectCommand"/> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        internal CreateMarkdownProjectCommand(GenerateMarkdownPackage package, CommandID command)
            : base(package, command)
        {
        }

        /// <summary>Called to update the current status of the command.</summary>
        protected override void OnBeforeQueryStatus()
        {
            Enabled = Package.IDE.Solution.IsOpen;
        }

        /// <summary>Called to execute the command.</summary>
        protected override void OnExecute()
        {
            base.OnExecute();
            Package.IDE.Solution.SolutionBuild.Clean(true);
            //     Package.IDE.Solution.SolutionBuild.Build(true);

            Console.WriteLine("Iniciando a geração de documentos");

            if (SelectedProjectItems?.Any() == false)
            {
                Console.WriteLine("Não foi encontrado nenhum projeto selecionado");
                return;
            }

            using (var document = new ActiveDocumentRestorer(Package))
            {
                //const string documentationFile = "DocumentationFile";
                const string projectFullName = "Microsoft.VisualStudio.ProjectSystem.VS.Implementation.Package.Automation.OAProject";


                //&& (a.ContainingProject.Properties?.Item(documentationFile) != null)
                var projects = SelectedProjectItems
                    .Where(a => a.ContainingProject != null &&
                    a.ContainingProject.GetType().FullName == projectFullName)
                    .Select(a => new ProjectFile(a.ContainingProject))
                    .Where(a => !string.IsNullOrWhiteSpace(a.DocFile) && !a.InvalidProject)
                    .Distinct()
                    .ToArray();

                Console.WriteLine("Foram encontrados {0}/{1} arquivos xml", projects.Length, SelectedProjectItems.Count());

                var solutionDirectory = Path.GetDirectoryName(Package.IDE.Solution.FullName);
                solutionDirectory = Path.Combine(solutionDirectory, Constants.ProjectDocPath);

                foreach (var project in projects)
                {
                    Debug.WriteLine($"Executando {project.DocName}");
                    Package.IDE.StatusBar.Text = $"Executando {project.DocName}";

                    var parser = new MarkdownParse(project.DocFile, project.AssemblyFile, solutionDirectory);
                    Package.IDE.Solution.SolutionBuild.BuildProject("Debug", project.UniqueName, true);
                    parser.ParseXml();
                    parser.GenerateDoc();
                }
            }
        }
    }
}