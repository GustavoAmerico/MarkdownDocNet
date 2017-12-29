using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace MarkdownVsix
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GenerateMarkdown
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("5813e236-e447-4319-a996-d3a2c1dacaf6");

        public static readonly Guid SymbolGenDocSolutionNode = new Guid("1ebc1a20-d2e7-4875-a7ff-2a3219b14683");
        public static readonly Guid SymbolGenDocSolutionFolderGroup = new Guid("1ebc1a20-d2e7-4875-a7ff-2a3219b14684");
        public static readonly Guid SymbolGenDocProjectNodeGroup = new Guid("1ebc1a20-d2e7-4875-a7ff-2a3219b14685");


        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateMarkdown"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private GenerateMarkdown(Package package)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var SymbolGenDocSolutionNodeCommandID = new CommandID(SymbolGenDocSolutionNode, 0x1050);
                var solutionmenuItem = new MenuCommand(this.GenerateMarkdownForSolutions, SymbolGenDocSolutionNodeCommandID);
                commandService.AddCommand(solutionmenuItem);

                var SymbolGenDocSolutionFolderGroupCommandID = new CommandID(SymbolGenDocSolutionFolderGroup, 0x1051);
                var foldermenuItem = new MenuCommand(this.GenerateMarkdownForSolutions, SymbolGenDocSolutionFolderGroupCommandID);
                commandService.AddCommand(foldermenuItem);

                var SymbolGenDocProjectNodeGroupCommandID = new CommandID(SymbolGenDocProjectNodeGroup, 0x1052);
                var projectmenuItem = new MenuCommand(this.GenerateMarkdownForSolutions, SymbolGenDocProjectNodeGroupCommandID);
                commandService.AddCommand(projectmenuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GenerateMarkdown Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new GenerateMarkdown(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(string message)
        {

            string title = "GenerateMarkdown";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private void GenerateMarkdownForProject(object sender, EventArgs e)
        {
            MenuItemCallback("Create Markdown for project");
            Console.WriteLine("Create Markdown for project");
        }

        private void GenerateMarkdownForSolutions(object sender, EventArgs e)
        {
            MenuItemCallback("Create Markdown for solutions");
            Console.WriteLine("Create Markdown for Solutions");
        }

        private void GenerateMarkdownForFolders(object sender, EventArgs e)
        {
            MenuItemCallback("Create Markdown for folders");
            Console.WriteLine("Create Markdown for Folders");
        }
    }
}
