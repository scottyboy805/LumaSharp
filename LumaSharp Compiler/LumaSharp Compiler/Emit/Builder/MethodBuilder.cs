using LumaSharp.Runtime;
using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime.Handle;
using LumaSharp.Runtime.Reflection;
using LumaSharp_Compiler.Semantics;
using LumaSharp_Compiler.Semantics.Model;
using System.Runtime.CompilerServices;
using AppContext = LumaSharp.Runtime.AppContext;

namespace LumaSharp_Compiler.Emit.Builder
{
    internal unsafe sealed class MethodBuilder
    {
        // Private
        private AppContext loadContext = null;
        private MethodModel methodModel = null;
        private MethodBodyBuilder bodyBuilder = null;


        // Constructor
        public MethodBuilder(AppContext loadContext, MethodModel methodModel)
        {
            this.loadContext = loadContext;
            this.methodModel = methodModel;
            this.bodyBuilder = new MethodBodyBuilder(methodModel.HasBody == true ? methodModel.BodyStatements : null);
        }

        // Methods
        public Method BuildMethod()
        {
            // Create method
            Method method = new Method(loadContext, methodModel.MethodName, methodModel.MethodFlags);

            return method;
        }

        public int EmitMetaModel(BinaryWriter writer = null)
        {
            // Check for writer
            if (writer == null)
            {
                // Create memory
                Stream executableStream = new MemoryStream();

                // Create writer
                writer = new BinaryWriter(executableStream);
            }

            // Get method flags
            MethodFlags methodFlags = methodModel.MethodFlags;

            // Write metadata
            writer.Write(methodModel.SymbolToken);
            writer.Write(methodModel.MethodName);
            writer.Write((uint)methodFlags);

            // Write return type
            if((methodFlags & MethodFlags.ReturnValue) != 0)
            {
                // Write return type token
                writer.Write(methodModel.ReturnTypeSymbol.SymbolToken);
            }

            // Write parameters types
            if((methodFlags & MethodFlags.ParamValues) != 0)
            {
                // Number of parameters
                writer.Write((ushort)methodModel.ParameterSymbols.Length);

                // Write all parameters
                foreach(ILocalIdentifierReferenceSymbol parameter in methodModel.ParameterSymbols)
                {
                    // Build parameter flags
                    Parameter.ParameterFlags flags = 0;

                    // Check for by reference
                    if (parameter.IsByReference == true) flags |= Parameter.ParameterFlags.Reference;

                    // Check for optional
                    if (parameter.IsOptional == true) flags |= Parameter.ParameterFlags.DefaultValue;

                    // Check for variable length
                    //if(parameter.va)

                    // Parameter type
                    writer.Write(parameter.SymbolToken);
                    writer.Write((uint)flags);
                }
            }

            // Write generic types
            if((methodFlags & MethodFlags.Generic) != 0)
            {

            }

            // Get size required for this method image
            writer.Flush();
            return (int)writer.BaseStream.Position;
        }

        public int EmitExecutableModel(BinaryWriter writer = null)
        {
            // Check for writer
            if (writer == null)
            {
                // Create memory
                Stream executableStream = new MemoryStream();

                // Create writer
                writer = new BinaryWriter(executableStream);
            }

            // Start of writer
            long offset = writer.BaseStream.Position;

            // Write size of method
            writer.Write((uint)0);
                        

            // Write method handle
            _MethodHandle handle = methodModel.MethodHandle;
            handle.Write(writer);


            _StackHandle[] argLocalHandles = methodModel.MethodArgLocals;

            // Emit locals
            for(int i = 0; i < argLocalHandles.Length; i++)
                argLocalHandles[i].Write(writer);


            // Create instruction builder
            InstructionBuilder builder = new InstructionBuilder(writer);

            // Emit instructions
            bodyBuilder.EmitExecutionObject(builder);


            // Get size
            long endOffset = writer.BaseStream.Position;
            long size = endOffset - offset;

            // Return and write size of instructions
            writer.BaseStream.Seek(offset, SeekOrigin.Begin);
            writer.Write((uint)size);

            // Return to end
            writer.BaseStream.Seek(endOffset, SeekOrigin.Begin);

            // Get size required for this method image
            writer.Flush();
            return (int)writer.BaseStream.Position;
        }
    }
}
