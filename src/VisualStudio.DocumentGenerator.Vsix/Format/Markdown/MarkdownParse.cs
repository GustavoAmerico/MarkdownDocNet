using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VSDocument.Format.Markdown;

namespace VisualStudio.DocumentGenerate.Vsix.Format.Markdown
{
    public class MarkdownParse : DocParser
    {
        public MarkdownParse(string docFile, string assemblyFile, string outputFile) : base(docFile, assemblyFile, outputFile)
        {
        }

        /// <summary>Returns the name of the given type as it would be notated in C#</summary>
        public string CSharpName(Type type)
        {
            //FullName is null when type is Generic with TResult, T e etc...
            var name = ignoredNamespaces.Contains(type.Namespace) ? type.Name : type.FullName ?? type.Name;
            name = name.Replace('+', '.');

            if ((type.IsPrimitive || type == typeof(string)) && primitiveNames.ContainsKey(type))
                return primitiveNames[type];

            if (!type.IsGenericType)
                return name;

            var output = new StringBuilder();
            output.Append(name.Substring(0, name.IndexOf('`')));
            output.Append("&lt;");
            output.Append(string.Join(", ", type.GetGenericArguments()
                                            .Select(t => CSharpName(t))));
            output.Append("&gt;");
            return output.ToString();
        }

        /// <summary>
        /// Returns the full name of the given member, in the same notation that is used in the XML
        /// documentation files for member ids and references.
        /// </summary>
        public string FullNameFromMember(MemberInfo member)
        {
            if (member is MethodInfo)
            {
                var method = (MethodInfo)member;
                if (method.GetParameters().Length > 0)
                    return method.DeclaringType.FullName + "." + method.Name + MakeSignature(method, humanReadable: false);
                else
                    return method.DeclaringType.FullName + "." + method.Name;
            }
            else
            {
                return member.DeclaringType.FullName + "." + member.Name;
            }
        }

        public string MakeSignature(MethodBase method, bool humanReadable = true)
        {
            var output = new StringBuilder();
            output.Append("(");
            var parameters = method.GetParameters();
            bool first = true;
            foreach (var p in parameters)
            {
                if (!first)
                    output.Append(humanReadable ? ", " : ",");

                if (p.IsOptional && humanReadable)
                    output.Append("[");

                if (humanReadable)
                {
                    output.Append(CSharpName(p.ParameterType));
                    output.Append(" ");
                    output.Append(p.Name);
                }
                else
                {
                    output.Append(p.ParameterType.FullName);
                }

                if (p.IsOptional && humanReadable)
                    output.Append("]");
                first = false;
            }
            output.Append(")");

            return output.ToString();
        }

        public string MemberListCategory(string title, IEnumerable<MemberInfo> members)
        {
            var output = new StringBuilder();
            output.AppendLine("");
            if (!String.IsNullOrEmpty(title))
            {
                output.AppendLine("## **" + title + "**");
                output.AppendLine("");
            }
            foreach (var member in members)
            {
                output.AppendLine(MemberListItem(member));
            }
            output.AppendLine("");
            output.AppendLine("");
            return output.ToString();
        }

        public string MemberListItem(MemberInfo member)
        {
            var fullName = FullNameFromMember(member);

            var output = new StringBuilder();
            output.AppendLine("<a id=\"" + FullNameFromMember(member) + "\"></a>");
            output.AppendLine("");
            output.Append("* ");
            if (member is FieldInfo || member is PropertyInfo)
            {
                Type type = null;
                if (member is FieldInfo)
                    type = ((FieldInfo)member).FieldType;
                else if (member is PropertyInfo)
                    type = ((PropertyInfo)member).PropertyType;

                if (type != null)
                    output.Append("*" + CSharpName(type) + "* ");

                output.Append("**" + member.Name + "**");
            }
            output.AppendLine("  ");
            if (MemberDocumentations.ContainsKey(fullName))
                this.WriteDocs(member, output);

            output.AppendLine("");

            return output.ToString();
        }

        protected override void WriteInfo(Type type, StringBuilder output)
        {
            output.AppendLine("<a id=\"" + type.FullName + "\"></a>");

            var typeType = "";

            if (type.IsValueType)
                typeType = "Struct";
            else if (type.IsInterface)
                typeType = "Interface";
            else if (type.IsClass)
                typeType = "Class";
            else if (type.IsEnum)
                typeType = "Enum";

            // Print the type name heading
            output.AppendLine("# " + typeType + " " + type.FullName);

            if (type.BaseType != typeof(object))
                output.AppendLine("*Extends " + type.BaseType.FullName + "*");

            output.AppendLine("");

            var doc = MemberDocumentations[type.FullName];
            // Print summary and remarks
            if (!String.IsNullOrEmpty(doc.Summary))
            {
                output.AppendLine(doc.Summary);
                output.AppendLine("");
            }

            if (!String.IsNullOrEmpty(doc.Remarks))
            {
                output.AppendLine(doc.Remarks);
                output.AppendLine("");
            }

            if (!String.IsNullOrEmpty(doc.Example))
            {
                output.AppendLine("**Examples**");
                output.AppendLine("");
                output.AppendLine(doc.Example);
                output.AppendLine("");
            }
        }

        protected override void WriteInfo(ConstructorInfo[] constructors, StringBuilder output)
        {
            if (constructors == null || !constructors.Any()) return;
            output.AppendLine("");
            foreach (var member in constructors)
            {
                var fullName = FullNameFromMember(member);
                output.AppendLine("<a id=\"" + fullName + "\"></a>");
                output.AppendLine("");
                output.Append("* ");

                var constructor = (ConstructorInfo)member;
                fullName = constructor.DeclaringType.FullName + ".#ctor" + MakeSignature(constructor, false);
                output.Append("**" + member.DeclaringType.Name + "** *" + MakeSignature(constructor) + "*");
                this.WriteDocs(member, output);
            }

            //output.Append(MemberListCategory("Constructors", constructors));

            output.AppendLine("");
        }

        protected override void WriteInfo(MethodInfo[] methods, StringBuilder output)
        {
            if (methods == null || !methods.Any()) return;
            output.AppendLine("## **Methods**");
            output.AppendLine("");
            output.AppendLine("");

            //TODO: Verificar se precisa remover IsSpecialName
            foreach (var method in methods.Where(m => !m.IsConstructor))
            {
                var returnValue = (method.ReturnType == null || method.ReturnType == typeof(void))
                    ? "void "
                    : $"*{CSharpName(method.ReturnType) }* ";

                output.Append($"### {returnValue} **{method.Name}** *{MakeSignature(method)}*");
                WriteDocs(method, output);

                output.AppendLine("");
                output.AppendLine("");
            }
            output.AppendLine("");
            output.AppendLine("");
        }

        protected override void WriteInfo(PropertyInfo[] properties, StringBuilder output)
        {
            if (properties.Length > 0)
            {
                output.Append(MemberListCategory("Properties", properties));
            }
        }

        protected override void WriteInfo(EventInfo[] events, StringBuilder output)
        {
            if (events.Length > 0)
            {
                output.Append(MemberListCategory("Events", events));
            }
        }

        protected override void WriteInfo(FieldInfo[] fields, StringBuilder output)
        {
            if (fields.Length > 0)
            {
                output.Append(MemberListCategory("Fields", fields));
            }
        }

        protected override void WriteStatic(FieldInfo[] staticFields, StringBuilder output)
        {
            if (staticFields == null || staticFields.Length == 0) return;

            output.AppendLine("## **Static Fields**");
            output.AppendLine("");

            foreach (var method in staticFields)
            {
                output.Append(MemberListItem(method));
            }
            output.AppendLine("");
        }

        protected override void WriteStatic(PropertyInfo[] staticProperties, StringBuilder output)
        {
            if (staticProperties.Length > 0)
            {
                output.Append(MemberListCategory("Static Properties", staticProperties));
            }
        }

        /// <summary>Metodo responsavel por escrever o summary, remarks, returns e example</summary>
        /// <param name="member">Atributo/metodo que será documentado</param>
        /// <param name="output">string de saída'</param>
        private void WriteDocs(MemberInfo member, StringBuilder output)
        {
            string fullName = FullNameFromMember(member);
            var docKey = MemberDocumentations.FirstOrDefault(d => d.Key.Replace("[^a-zA-Z0-9.]", "") == fullName.Replace("[^a-zA-Z0-9.]", ""));
            if (ReferenceEquals(docKey, null) || ReferenceEquals(docKey.Value, null)) return;
            var doc = docKey.Value;

            output.AppendLine("");

            //if (!MemberDocumentations.ContainsKey(fullName)) return;
            //var doc = MemberDocumentations[fullName];
            if (!String.IsNullOrEmpty(doc.Summary))
            {
                output.AppendLine("");
                output.AppendLine($"        Summary: {doc.Summary} ");
            }
            if (!String.IsNullOrEmpty(doc.Remarks))
            {
                output.AppendLine("");
                output.AppendLine($"        Remarks: {doc.Remarks}");
            }
            if (!String.IsNullOrEmpty(doc.Returns))
            {
                output.AppendLine("");
                output.AppendLine($"        **Returns:** {doc.Returns}");
            }
            if (!String.IsNullOrEmpty(doc.Exception))
            {
                output.AppendLine("");
                output.AppendLine("<b>Exception(s):</b>");
                output.AppendLine($"        <label style='color:red;'>{doc.Exception}</label>");
            }
            if (!String.IsNullOrEmpty(doc.Example))
            {
                output.AppendLine("");
                output.AppendLine("**Example:** ");
                output.AppendLine("");
                output.AppendLine($@"```CSharp");
                output.AppendLine("");
                var lines = doc.Example.Split(';');
                foreach (var line in lines)
                {
                    output.AppendLine(line);
                }


                output.AppendLine("");
                output.AppendLine($@"```");
            }

            //Documenta os parametros caso exista
            if (member is MethodBase method)
                this.WriteParameters(method, output, doc);
            output.AppendLine("");
        }

        private void WriteParameters(MethodBase method, StringBuilder output, MemberDoc doc)
        {
            var parameters = method?.GetParameters();
            if (parameters == null || !parameters.Any()) return;

            output.AppendLine("#### **Parameters:**");
            output.AppendLine("");
            output.AppendLine("");

            foreach (var paramInfo in parameters)
            {
                output.Append("* *" + CSharpName(paramInfo.ParameterType) + "* **" + paramInfo.Name + "**");
                if (paramInfo.IsOptional)
                    output.Append(" *(optional, default: " + paramInfo.DefaultValue.ToString() + ")* ");
                if (doc.ParameterDescriptionsByName.ContainsKey(paramInfo.Name))
                    output.Append($" - (_{doc.ParameterDescriptionsByName[paramInfo.Name]}_)");

                output.AppendLine("");
                output.AppendLine("");
            }
        }
    }
}