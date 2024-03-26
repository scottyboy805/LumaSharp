using LumaSharp.Runtime.Reflection;
using LumaSharp_Compiler.Semantics.Model;
using System.Reflection.Emit;
using AppContext = LumaSharp.Runtime.AppContext;
using MethodBuilder = LumaSharp_Compiler.Emit.Builder.MethodBuilder;

namespace LumaSharp_CompilerTests.Emit
{
    public static class EmitUtil
    {
        // Methods
        public static Method GetExecutableMethodOnly(MethodModel model)
        {
            // Create context
            AppContext context = new AppContext();

            // Store method
            MemoryStream stream = new MemoryStream();

            // Create method builder
            MethodBuilder builder = new MethodBuilder(context, model);

            // Emit to stream
            BinaryWriter writer = new BinaryWriter(stream);
            builder.EmitMetaModel(writer);
            builder.EmitExecutableModel(writer);


            // Return to start
            stream.Seek(0, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(stream);

            // Read method
            Method method = new Method(context);
            method.LoadMethodMetadata(reader);
            method.LoadMethodExecutable(reader);

            return method;
        }
    }
}
