using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarkdownVsix
{
    public class ProjectFile : IEquatable<ProjectFile>
    {
        private const string assemblyName = "AssemblyName";
        private const string documentationFile = "DocumentationFile";
        private const string fullPath = "FullPath";
        public string AssemblyFile => Directory.GetFiles(Path, AssemblyName + ".dll", SearchOption.AllDirectories)?.FirstOrDefault();

        public string AssemblyName { get; set; }

        public string DocFile => Directory.GetFiles(Path, DocName, SearchOption.AllDirectories)?.FirstOrDefault();

        public string DocName { get; set; }

        public string Path { get; set; }

        public string UniqueName { get; private set; }

        public bool InvalidProject { get; }

        /// <summary></summary>
        /// <param name="project"></param>
        /// <exception cref="ArgumentNullException">Ocorre quando project é nulo</exception>
        public ProjectFile(EnvDTE.Project project)
        {
            if (ReferenceEquals(project, null))
            {

                Console.WriteLine("O Programa esta tentando criar a documentação para um projeto que não foi encontrado");
                InvalidProject = true;
            }

            Path = project.Properties.Item(fullPath)?.Value?.ToString();
            AssemblyName = project.Properties.Item(assemblyName)?.Value?.ToString();
            DocName = AssemblyName + ".xml";
            UniqueName = project.UniqueName;
        }

        public ProjectFile()
        {
        }

        public static bool operator !=(ProjectFile file1, ProjectFile file2)
        {
            return !(file1 == file2);
        }

        public static bool operator ==(ProjectFile file1, ProjectFile file2)
        {
            return EqualityComparer<ProjectFile>.Default.Equals(file1, file2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProjectFile);
        }

        public bool Equals(ProjectFile other)
        {
            return other != null &&
                   AssemblyName == other.AssemblyName;
        }

        public override int GetHashCode()
        {
            return -1184256330 + EqualityComparer<string>.Default.GetHashCode(AssemblyName);
        }
    }
}