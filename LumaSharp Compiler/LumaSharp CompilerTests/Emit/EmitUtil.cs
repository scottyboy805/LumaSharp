using LumaSharp.Runtime.Reflection;
using LumaSharp.Compiler.Semantics.Model;
using AppContext = LumaSharp.Runtime.AppContext;
using MethodBuilder = LumaSharp.Compiler.Emit.MethodBuilder;
using LumaSharp.Compiler.Emit;
using LumaSharp.Runtime;

namespace CompilerTests.Emit
{
    public static class EmitUtil
    {
        // Methods
        public static MetaMethod GetExecutableMethodOnly(MethodModel model)
        {
            // Create context
            AppContext context = new AppContext();

            // Store method
            MemoryStream metaStream = new MemoryStream();
            MemoryStream executableStream = new MemoryStream();

            // Create method builder
            MethodBuilder builder = new MethodBuilder(context, model);



            MetaBuilder metaBuilder = new MetaBuilder(metaStream);
            ExecutableBuilder executableBuilder = new ExecutableBuilder(executableStream);

            // Emit method
            builder.BuildMemberExecutable(executableBuilder);
            builder.BuildMemberMeta(metaBuilder);

            // Emit to stream
            //BinaryWriter writer = new BinaryWriter(stream);
            //builder.EmitMetaModel(writer);
            //builder.EmitExecutableModel(writer, "current.instructions");


            metaStream.Seek(0, SeekOrigin.Begin);
            using (Stream fs = File.Create("current.bytecode"))
                metaStream.CopyTo(fs);

            // Return to start
            metaStream.Seek(0, SeekOrigin.Begin);
            executableStream.Seek(0, SeekOrigin.Begin);

            // Read method
            ExecutableReader executableReader = new ExecutableReader(context, executableStream);
            MetaReader metaReader = new MetaReader(context, metaStream);

            // Read the method
            MetaMethod method = metaReader.ReadMethodMeta(true);

            // Read the executable
            unsafe { executableReader.ReadMethodExecutable(method.RVA, true); }

            //MetaMethod method = new MetaMethod(context);
            //method.LoadMethodMetadata(reader);
            //method.LoadMethodExecutable(reader);


            return method;
        }
    }
}
