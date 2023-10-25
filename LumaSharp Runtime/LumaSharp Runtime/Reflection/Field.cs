
namespace LumaSharp.Runtime.Reflection
{
    public class Field : Member
    {
        // Private
        private Type fieldType = null;

        // Properties
        public Type FieldType
        {
            get { return fieldType; }
        }

        // Constructor
        internal Field(string name, MemberFlags memberFlags)
            : base(name, memberFlags)
        {
        }
    }
}
