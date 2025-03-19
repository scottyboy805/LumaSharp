using LumaSharp.Runtime;
using LumaSharp.Compiler.Semantics.Model;
using AppContext = LumaSharp.Runtime.AppContext;

namespace LumaSharp.Compiler.Emit
{
    internal sealed class TypeBuilder : MemberBuilder
    {
        // Private
        private AppContext loadContext = null;
        private TypeModel typeModel = null;
        private List<FieldBuilder> fieldBuilders = new List<FieldBuilder>();
        private List<MethodBuilder> methodBuilders = new List<MethodBuilder>();

        // Properties
        public IReadOnlyList<FieldBuilder> FieldBuilders
        {
            get { return fieldBuilders; }
        }

        public IReadOnlyList<MethodBuilder> MethodBuilders
        {
            get { return methodBuilders; }
        }

        // Constructor
        public TypeBuilder(AppContext loadContext, TypeModel typeModel)
            : base(typeModel)
        {
            this.loadContext = loadContext;
            this.typeModel = typeModel;

            // Add fields
            if (typeModel.MemberFields != null)
                fieldBuilders.AddRange(typeModel.MemberFields.Select(f => new FieldBuilder(f)));

            // Add methods
            if (typeModel.MemberMethods != null)
                methodBuilders.AddRange(typeModel.MemberMethods.Select(m => new MethodBuilder(loadContext, m)));
        }

        // Methods
        public override void BuildMemberMeta(MetaBuilder builder)
        {
            builder.WriteTypeMeta(typeModel);
        }

        public override void BuildMemberExecutable(ExecutableBuilder builder)
        {
            builder.WriteTypeExecutable(typeModel.TypeHandle);
        }

        //public int EmitMetaModel(BinaryWriter writer = null)
        //{
        //    // Check for writer
        //    if (writer == null)
        //    {
        //        // Create memory
        //        Stream executableStream = new MemoryStream();

        //        // Create writer
        //        writer = new BinaryWriter(executableStream);
        //    }

        //    // Get type flags
        //    TypeFlags typeFlags = typeModel.TypeFlags;

        //    // Write metadata
        //    writer.Write(typeModel.SymbolToken);
        //    writer.Write(typeModel.TypeName);
        //    writer.Write((uint)typeFlags);


        //    // Write fields
        //    writer.Write(typeModel.MemberFields.Count);

        //    // Write all field meta
        //    foreach (FieldBuilder fieldBuilder in fieldBuilders)
        //    {
        //        // Emit meta model
        //        fieldBuilder.EmitMetaModel(writer);
        //    }

        //    // Write methods
        //    writer.Write(typeModel.MemberMethods.Count);

        //    // Write all method meta
        //    foreach (MethodBuilder methodBuilder in methodBuilders)
        //    {
        //        // Emit meta model
        //        methodBuilder.EmitMetaModel(writer);
        //    }

        //    // Get size required for this type image
        //    writer.Flush();
        //    return (int)writer.BaseStream.Position;
        //}

        //public int EmitExecutableModel(BinaryWriter writer = null)
        //{
        //    // Check for writer
        //    if (writer == null)
        //    {
        //        // Create memory
        //        Stream executableStream = new MemoryStream();

        //        // Create writer
        //        writer = new BinaryWriter(executableStream);
        //    }

        //    // Get type handle
        //    _TypeHandle handle = typeModel.TypeHandle;
        //    handle.Write(writer);


        //    // Write all fields
        //    foreach (FieldBuilder fieldBuilder in fieldBuilders)
        //    {
        //        fieldBuilder.EmitExecutableModel(writer);
        //    }


        //    // Write all methods
        //    foreach (MethodBuilder methodBuilder in methodBuilders)
        //    {
        //        methodBuilder.EmitExecutableModel(writer);
        //    }

        //    // Get size required for this type image
        //    writer.Flush();
        //    return (int)writer.BaseStream.Position;
        //}
    }
}
