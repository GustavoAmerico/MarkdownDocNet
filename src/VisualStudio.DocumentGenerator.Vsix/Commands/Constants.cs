using System;

namespace MarkdownVsix
{
    public class Constants
    {
        public const string DocExtension = ".xml";

        /// <summary>Caminho do diretório dos arquivos</summary>
        public const string ProjectDocPath = "AutoDocs";

        public static class PackageGuids
        {
            public const string PackageGuidString = ("d69f1580-274f-4d12-b13a-c365c759de66");

            public readonly static Guid SymbolGenDocProjectNodeGroup = new Guid("1ebc1a20-d2e7-4875-a7ff-2a3219b14686");

            public readonly static Guid SymbolGenDocSolutionFolderGroup = new Guid("1ebc1a20-d2e7-4875-a7ff-2a3219b14685");
            public readonly static Guid SymbolGenDocSolutionNodeGroup = new Guid("1ebc1a20-d2e7-4875-a7ff-2a3219b14683");
        }

        /// <summary>Helper class that exposes all GUIDs used across VS Package.</summary>
        public sealed partial class PackageIds
        {
            public const int CmdIDSymbolGenDocProjectNodeGroup = 0x1052;
            public const int CmdIDSymbolGenDocSolutionFolderGroup = 0x1051;
            public const int CmdIDSymbolGenDocSolutionNodeGroup = 0x1050;
        }
    }
}