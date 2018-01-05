using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

namespace MarkdownVsix
{
    /// <summary>This is the class that implements the package exposed by this assembly.</summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio is to
    /// implement the IVsPackage interface and register itself with the shell. This package uses the
    /// helper classes defined inside the Managed Package Framework (MPF) to do it: it derives from
    /// the Package class that provides the implementation of the IVsPackage interface and uses the
    /// registration attributes defined in the framework to register itself and its components with
    /// the shell. These attributes tell the pkgdef creation utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset
    /// Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Constants.PackageGuids.PackageGuidString)]
    [ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]
    [ProvideBindingPath]
    //[ProvideMenuResource(1000, 1)] // This attribute is needed to let the shell know that this package exposes some menus.

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class GenerateMarkdownPackage : Package
    {
        /// <summary>An internal collection of the commands registered by this package.</summary>
        private readonly ICollection<BaseCommand> _commands = new List<BaseCommand>();

        /// <summary>
        /// The top level application instance of the VS IDE that is executing this package.
        /// </summary>
        private DTE2 _ide;

        /// <summary>Gets the currently active document, otherwise null.</summary>
        public Document ActiveDocument
        {
            get
            {
                try
                {
                    return IDE.ActiveDocument;
                }
                catch (Exception)
                {
                    // If a project property page is active, accessing the ActiveDocument causes an exception.
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the top level application instance of the VS IDE that is executing this package.
        /// </summary>
        public DTE2 IDE => _ide ?? (_ide = (DTE2)GetService(typeof(DTE)));

        /// <summary>Gets the version of the running IDE instance.</summary>
        public double IDEVersion => Convert.ToDouble(IDE.Version, CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets or sets a flag indicating if CodeMaid is running inside an AutoSave context.
        /// </summary>
        public bool IsAutoSaveContext { get; set; }

        /// <summary>Gets the menu command service.</summary>
        public OleMenuCommandService MenuCommandService => GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

        /// <summary>Gets the shell service.</summary>
        private IVsShell ShellService => GetService(typeof(SVsShell)) as IVsShell;

        /// <summary>Initializes a new instance of the <see cref="GenerateMarkdown"/> class.</summary>
        public GenerateMarkdownPackage()
        {
            // Inside this method you can place any initialization code that does not require any
            // Visual Studio service because at this point the package object is created but not
            // sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so
        /// this is the place where you can put all the initialization code that rely on services
        /// provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this));
            RegisterCommands();
            //GenerateMarkdown.Initialize(this);
            base.Initialize();
        }

        /// <summary>Register the package commands (which must exist in the .vsct file).</summary>
        private void RegisterCommands()
        {
            var menuCommandService = MenuCommandService;
            if (menuCommandService != null)
            {
                _commands.Add(new CreateMarkdownProjectCommand(this));

                // Add all commands to the menu command service.
                foreach (var command in _commands)
                {
                    menuCommandService.AddCommand(command);
                }
            }
        }
    }
}