using LumaSharp.Runtime.Reflection;
using LumaSharp.Compiler.Semantics.Model;
using AppContext = LumaSharp.Runtime.AppContext;
using MethodBuilder = LumaSharp.Compiler.Emit.MethodBuilder;
using LumaSharp.Compiler.Emit;

namespace LumaSharp_CompilerTests.Emit
{
    public static class EmitUtil
    {
        // Methods
        public static MetaMethod GetExecutableMethodOnly(MethodModel model)
        {
            // Create context
            AppContext context = new AppContext();

            // Store method
            MemoryStream stream = new MemoryStream();

            // Create method builder
            MethodBuilder builder = new MethodBuilder(context, model);



            MetaBuilder metaBuilder = new MetaBuilder(stream);
            ExecutableBuilder executableBuilder = new ExecutableBuilder(stream);

            // Emit method
            builder.BuildMemberExecutable(executableBuilder);
            builder.BuildMemberMeta(metaBuilder);

            // Emit to stream
            //BinaryWriter writer = new BinaryWriter(stream);
            //builder.EmitMetaModel(writer);
            //builder.EmitExecutableModel(writer, "current.instructions");


            stream.Seek(0, SeekOrigin.Begin);
            using (Stream fs = File.Create("current.bytecode"))
                stream.CopyTo(fs);

            // Return to start
            stream.Seek(0, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(stream);

            // Read method
            MetaMethod method = new MetaMethod(context);
            method.LoadMethodMetadata(reader);
            method.LoadMethodExecutable(reader);

            // Define the method
            context.DefineMember(method);

            return method;
        }
    }
}
