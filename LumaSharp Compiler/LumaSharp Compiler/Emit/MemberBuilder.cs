using LumaSharp.Compiler.Semantics.Model;

namespace LumaSharp.Compiler.Emit
{
    internal abstract class MemberBuilder
    {
        // Private
        private MemberModel memberModel = null;

        // Constructor
        protected MemberBuilder(MemberModel memberModel)
        {
            this.memberModel = memberModel;
        }

        // Methods
        public abstract void BuildMemberMeta(MetaBuilder builder);

        public abstract void BuildMemberExecutable(ExecutableBuilder builder);
    }
}
