using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace VSDocument.Format.Markdown
{
    public class DocParser : DocFormat, IDisposable
    {
        protected static HashSet<string> ignoredNamespaces = new HashSet<string>
        {
            "System",
            "System.Collections.Generic",
            "System.Text",
            "System.IO"
        };

        protected static Dictionary<Type, string> primitiveNames = new Dictionary<Type, string>
        {
            {typeof(byte), "byte"},
            {typeof(sbyte), "sbyte"},
            {typeof(short), "short"},
            {typeof(ushort), "ushort"},
            {typeof(int), "int"},
            {typeof(uint), "uint"},
            {typeof(long), "long"},
            {typeof(ulong), "ulong"},
            {typeof(char), "char"},
            {typeof(float), "float"},
            {typeof(double), "double"},
            {typeof(decimal), "decimal"},
            {typeof(bool), "bool"},
            {typeof(object), "object"},
            {typeof(string), "string"},
        };

        protected Dictionary<string, MemberDoc> MemberDocumentations = new Dictionary<string, MemberDoc>();
        private Assembly AssemblyInfo;
        private bool disposedValue = false;
        private XDocument Doc;
        protected string OutputPath { get; private set; }

        public DocParser(string docFile, string assemblyFile, string outputFile)
        {
            Doc = XDocument.Load(docFile, LoadOptions.SetLineInfo);
            AssemblyInfo = Assembly.LoadFile(assemblyFile);
            OutputPath = outputFile;
        }

        public static string CommonNamespacePrefix(string fullName1, string fullName2)
        {
            var elements1 = fullName1.Split('.');
            var elements2 = fullName2.Split('.');

            var potentialMatchLength = Math.Min(elements1.Length, elements2.Length);

            var output = new StringBuilder();
            bool first = true;

            for (var i = 0; i < potentialMatchLength; i++)
            {
                if (elements1[i].Equals(elements2[i]))
                {
                    if (!first)
                        output.Append(".");
                    first = false;

                    output.Append(elements1[i]);
                }
                else
                    break;
            }

            return output.ToString();
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above. GC.SuppressFinalize(this);
        }

        public string FixValueIndentation(XNode node, string text)
        {
            var lineInfo = (IXmlLineInfo)node.Parent;
            var indentationCount = lineInfo.LinePosition - 2;
            if (indentationCount > 0)
            {
                var indentation = "\n" + new String(' ', indentationCount);
                text = text.Replace(indentation, "\n");
            }

            return text.Trim();
        }

        public string FullNameFromDescriptor(string descriptor)
        {
            var descriptorElements = descriptor.Split(':');

            if (descriptorElements.Length != 2)
                throw new InvalidOperationException(String.Format("Invalid name descriptor: '{0}'", descriptor));

            return descriptorElements[1];
        }

        public void GenerateDoc()
        {
            var types = AssemblyInfo.GetExportedTypes();

            var typesSorted = types.OrderByDescending((Type a) =>
            {
                var impA = 0;
                if (MemberDocumentations.ContainsKey(a.FullName))
                    impA = MemberDocumentations[a.FullName].Importance;
                return impA;
            });

            foreach (var type in typesSorted)
            {
                WriteFile(type);
            }
        }

        public string HumanNameFromDescriptor(string descriptor, string parentTypeOrNamespace = null)
        {
            var descriptorElements = descriptor.Split(':');

            if (descriptorElements.Length != 2 || descriptorElements[0].Length != 1)
                throw new InvalidOperationException(String.Format("Invalid name descriptor: '{0}'", descriptor));

            var memberType = MemberDoc.TypeFromDescriptor(descriptorElements[0][0]);
            var fullName = descriptorElements[1];

            // Cut away any method signatures
            var fullNameNoSig = fullName.Split(new char[] { '(' }, 2)[0];

            if (String.IsNullOrEmpty(parentTypeOrNamespace))
                return fullName;

            var commonPrefix = "";
            var dotIndex = fullNameNoSig.LastIndexOf('.');
            if (dotIndex >= 0)
            {
                var possiblePrefix = fullNameNoSig.Substring(0, dotIndex);
                commonPrefix = CommonNamespacePrefix(possiblePrefix, parentTypeOrNamespace);
            }

            //if(memberType == MemberType.Type || memberType == MemberType.Namespace)
            return fullNameNoSig.Substring(commonPrefix.Length + 1);
            /*else
            {
                var declaringTypeName = fullNameNoSig.Substring(0, fullNameNoSig.LastIndexOf('.'));
                var declaringType = AssemblyInfo.GetType(declaringTypeName);
                if(declaringType == null || declaringType.FullName != parentTypeOrNamespace)
                    return fullNameNoSig.Substring(commonPrefix.Length + 1);
                else
                {
                    // So the given descriptor
                    var memberName = fullName.Substring(commonPrefix.Length + 1);

                    // For everything except methods, just return the member name
                    if (memberType != MemberType.Method)
                        return memberName;

                    // Try to find the exact matching method so we can print the correct signature
                    var possibleMatches = declaringType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                    foreach(var match in possibleMatches)
                    {
                        var memberId = FullNameFromMember(match);
                        if (memberId == fullName)
                            return match.Name + MakeSignature(match, true);
                    }

                    return memberName;
                }
            }
            */
        }

        public string LinkFromDescriptor(string descriptor, string contextMemberName, string linkName = null)
        {
            var link = FullNameFromDescriptor(descriptor);

            if (linkName == null)
                return " [" + HumanNameFromDescriptor(descriptor, contextMemberName) + "](#" + link + ") ";
            else
                return " [" + linkName + "](#" + link + ") ";
        }

        /// <summary>
        /// Parses the text inside a given XML node and returns a Markdown version of it.
        /// </summary>
        public string ParseDocText(XNode node, string contextMemberName)
        {
            if (node.NodeType == XmlNodeType.Text)
            {
                var text = ((XText)node).Value;
                return FixValueIndentation(node, text);
            }
            else if (node.NodeType == XmlNodeType.Element)
            {
                var element = (XElement)node;
                if (element.Name == "see")
                {
                    var descriptor = element.Attribute("cref").Value;
                    string linkName = null;
                    if (!String.IsNullOrEmpty(element.Value))
                        linkName = element.Value;
                    return LinkFromDescriptor(descriptor, contextMemberName, linkName);
                }
                else if (element.Name == "code")
                {
                    var code = FixValueIndentation(element, element.Value);
                    ParseDocText(element.FirstNode, contextMemberName);
                    return "\n```csharp\n" + code + "\n```\n";
                }
                else
                {
                    var output = new StringBuilder();
                    foreach (var child in element.Nodes())
                    {
                        if (child.NodeType == XmlNodeType.Element || child.NodeType == XmlNodeType.Text)
                            output.Append(ParseDocText(child, contextMemberName));
                    }
                    return output.ToString();
                }
            }
            else
                return "";
        }

        /// <summary>
        /// Parsed a single member node from the xml documentation and returns the corresponding
        /// MemberDoc object.
        /// </summary>
        public MemberDoc ParseMember(XElement member)
        {
            var memberInfo = new MemberDoc();

            string nameDescriptor = member.Attribute("name").Value;
            var descriptorElements = nameDescriptor.Split(':');

            if (descriptorElements.Length != 2)
                throw new InvalidOperationException(String.Format(
                        "Invalid name descriptor in line {0}: '{1}'",
                        ((IXmlLineInfo)member).LineNumber,
                        nameDescriptor
                        ));

            memberInfo.Type = MemberDoc.TypeFromDescriptor(descriptorElements[0][0]);

            memberInfo.FullName = descriptorElements[1];
            memberInfo.LocalName = memberInfo.FullName;

            var xImportance = member.Element("importance");
            if (xImportance != null)
            {
                int importance = 0;
                if (int.TryParse(xImportance.Value, out importance))
                    memberInfo.Importance = importance;
            }

            var xSummary = member.Element("summary");
            if (xSummary != null)
                memberInfo.Summary = ParseDocText(xSummary, memberInfo.FullName);

            var xRemarks = member.Element("remarks");
            if (xRemarks != null)
                memberInfo.Remarks = ParseDocText(xRemarks, memberInfo.FullName);

            var xReturns = member.Element("returns");
            if (xReturns != null)
                memberInfo.Returns = ParseDocText(xReturns, memberInfo.FullName);

            var xExample = member.Element("example");
            if (xExample != null)
                memberInfo.Example = ParseDocText(xExample, memberInfo.FullName);

            var xParams = member.Elements("param");
            foreach (var param in xParams)
            {
                var name = param.Attribute("name").Value;
                memberInfo.ParameterDescriptionsByName[name] = ParseDocText(param, memberInfo.FullName);
            }

            return memberInfo;
        }

        public void ParseXml()
        {
            var members = Doc.Element("doc").Element("members").Elements("member");
            foreach (var member in members)
            {
                var memberInfo = ParseMember(member);
                MemberDocumentations[memberInfo.FullName] = memberInfo;
            }
        }

        public void WriteFile(Type type)
        {
            string md = "";
            if (type.BaseType == typeof(System.MulticastDelegate))
            {
                // Todo: Docs for delegate types
            }
            else if (MemberDocumentations.ContainsKey(type.FullName) && (type.IsClass || type.IsInterface || type.IsEnum))
            {
                // Only print members that are documented

                // Print overview of all members
                var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                var events = type.GetEvents(BindingFlags.Instance | BindingFlags.Public);
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                var staticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
                var staticProperties = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
                var staticFields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);

                var output = new StringBuilder();
                WriteInfo(type, output);

                WriteStatic(staticFields, output);
                if (!type.IsEnum) WriteInfo(fields, output);

                WriteStatic(staticProperties, output);
                WriteInfo(properties, output);

                WriteInfo(events, output);
                WriteInfo(constructors, output);
                WriteInfo(methods, output);
                WriteStatic(staticMethods, output);
                output.AppendLine("");
                //output.Append(new StringBuilder());
                md = output.ToString();
            }

            if (!String.IsNullOrEmpty(md))
            {
                var assemblyName = type.Assembly.GetName().Name;
                var path = $"{OutputPath}\\{assemblyName}\\";
                if (!string.Equals(assemblyName, type.Namespace, StringComparison.CurrentCultureIgnoreCase))
                    path += type.Namespace;

                var output = new StringBuilder();
                output.Append(md);
                output.AppendLine("");
                output.AppendLine("---");
                output.AppendLine("");
                Directory.CreateDirectory(path);
                var file = $"{path}\\{type.Name}.md";
                File.WriteAllText(file, output.ToString());
            }
        }


        // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                this.AssemblyInfo = null;
                this.Doc = null;
                GC.ReRegisterForFinalize(this);
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }
    }

}