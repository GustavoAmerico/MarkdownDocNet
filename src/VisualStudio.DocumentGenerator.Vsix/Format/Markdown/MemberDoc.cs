using System;
using System.Collections.Generic;

namespace VSDocument.Format.Markdown
{
    public class MemberDoc
    {
        public string Example;

        public string Exception;
        public string FullName;

        public int Importance = 0;

        public string LocalName;

        public Dictionary<string, string> ParameterDescriptionsByName = new Dictionary<string, string>();

        public string ParentName;

        public string Remarks;

        public string Returns;

        public string Summary;
        public MemberType Type;

        public static MemberType TypeFromDescriptor(char descriptor)
        {
            switch (descriptor)
            {
                case 'T':
                    return MemberType.Type;

                case 'M':
                    return MemberType.Method;

                case 'E':
                    return MemberType.Event;

                case 'F':
                    return MemberType.Field;

                case 'P':
                    return MemberType.Property;

                default:
                    throw new ArgumentException("Unknown member descriptor: " + descriptor);
            }
        }
    }
}