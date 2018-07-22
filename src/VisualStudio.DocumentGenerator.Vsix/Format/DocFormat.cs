using System;
using System.Reflection;
using System.Text;

namespace VSDocument.Format.Markdown
{
    public abstract class DocFormat
    {
        protected virtual void WriteInfo(MethodInfo[] methods, StringBuilder output)
        {
        }

        protected virtual void WriteInfo(PropertyInfo[] properties, StringBuilder output)
        {
        }

        protected virtual void WriteInfo(FieldInfo[] fields, StringBuilder output)
        {
        }

        protected virtual void WriteInfo(EventInfo[] events, StringBuilder output)
        {
        }

        protected virtual void WriteInfo(ConstructorInfo[] events, StringBuilder output)
        {
        }

        protected virtual void WriteInfo(Type type, StringBuilder output)
        {
        }

        protected virtual void WriteStatic(FieldInfo[] staticFields, StringBuilder output)
        {
        }

        protected virtual void WriteStatic(MethodInfo[] staticMethods, StringBuilder output)
        {
        }

        protected virtual void WriteStatic(PropertyInfo[] staticProperties, StringBuilder output)
        {
        }
    }
}