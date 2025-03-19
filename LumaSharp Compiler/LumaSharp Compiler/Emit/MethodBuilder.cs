using LumaSharp.Runtime;
using LumaSharp.Runtime.Handle;
using LumaSharp.Compiler.Semantics;
using LumaSharp.Compiler.Semantics.Model;
using AppContext = LumaSharp.Runtime.AppContext;

namespace LumaSharp.Compiler.Emit
{
    internal unsafe sealed class MethodBuilder : MemberBuilder
    {
        // Private
        private AppContext loadContext = null;
        private MethodModel methodModel = null;
        private MethodBodyBuilder bodyBuilder = null;
        private int rva = 0;

        // Constructor
        public MethodBuilder(AppContext loadContext, MethodModel methodModel)
            : base(methodModel)
        {
            this.loadContext = loadContext;
            this.methodModel = methodModel;
            bodyBuilder = new MethodBodyBuilder(methodModel.ParameterSymbols.Length, methodModel.HasBody == true ? methodModel.BodyStatements : null);
        }

        // Methods
        //public Method BuildMethod()
        //{
        //    // Create argument handles
        //    if (methodModel.HasParameters == true)
        //    {
        //        // Create array
        //        methodArgumentHandles = new _VariableHandle[methodModel.ParameterSymbols.Length];

        //        // Process all
        //        for (int i = 0; i < methodModel.ParameterSymbols.Length; i++)
        //        {
        //            // Create the argument handle with placeholder data - correct size etc will be calculated by the runtime
        //            methodArgumentHandles[i] = new _VariableHandle(
        //                new _TypeHandle(methodModel.ParameterSymbols[i].TypeSymbol.PrimitiveType),
        //                uint.MaxValue);
        //        }
        //    }


        //    // Build signature flags
        //    _MethodSignatureFlags signatureFlags = 0;

        //    if (methodModel.IsGlobal == false) signatureFlags |= _MethodSignatureFlags.HasThis;
        //    if (methodModel.HasParameters == true) signatureFlags |= _MethodSignatureFlags.HasArguments;
        //    if (methodModel.HasReturnTypes == true) signatureFlags |= _MethodSignatureFlags.HasReturn;
        //    if (methodModel.HasReturnTypes == false && methodModel.HasParameters == false) signatureFlags |= _MethodSignatureFlags.VoidCall;

        //    // Build method signature
        //    methodSignatureHandle = new _MethodSignatureHandle((ushort)methodModel.ParameterSymbols.Length, signatureFlags);


        //    // Build method handle
        //    methodHandle = new _MethodHandle(-1, methodSignatureHandle, );

        //    // Create method
        //    Method method = new Method(loadContext, methodModel.MethodName, methodModel.MethodFlags);

        //    return method;
        //}

        public override void BuildMemberMeta(MetaBuilder builder)
        {
            // Check for rva
            if (rva == 0 && bodyBuilder == null)
                throw new InvalidOperationException("Method executable must be emitted before metadata");

            builder.WriteMethodMeta(methodModel, rva);
        }

        public override void BuildMemberExecutable(ExecutableBuilder builder)
        {
            // Build arg and locals
            List<_VariableHandle> parameterHandles = new List<_VariableHandle>();
            List<_VariableHandle> localHandles = new List<_VariableHandle>();
            uint stackOffset = 0;

            // Process parameters
            for (int i = 0; i < methodModel.ParameterSymbols.Length; i++)
            {
                // Add parameter
                parameterHandles.Add(new _VariableHandle(methodModel.ParameterSymbols[i].TypeSymbol.TypeHandle, stackOffset));

                // Advance offset
                stackOffset += parameterHandles[parameterHandles.Count - 1].TypeHandle.TypeSize;
            }

            // Get all locals
            List<ILocalIdentifierReferenceSymbol> locals = new List<ILocalIdentifierReferenceSymbol>();

            // Add scoped locals
            foreach (IScopedReferenceSymbol scope in methodModel.DescendantsOfType<IScopedReferenceSymbol>(true))
            {
                // Check for locals
                if (scope.LocalsInScope != null)
                    locals.AddRange(scope.LocalsInScope);
            }


            // Build all locals
            for (int i = 0; i < locals.Count; i++)
            {
                // Add local
                localHandles.Add(new _VariableHandle(locals[i].TypeSymbol.TypeHandle, stackOffset));

                // Advance offset
                stackOffset += localHandles[localHandles.Count - 1].TypeHandle.TypeSize;
            }


            // Get the flags
            _MethodSignatureFlags signatureFlags = 0;

            if (methodModel.IsGlobal == false) signatureFlags |= _MethodSignatureFlags.HasThis;
            if (methodModel.HasParameters == true) signatureFlags |= _MethodSignatureFlags.HasArguments;
            if (methodModel.HasReturnTypes == true) signatureFlags |= _MethodSignatureFlags.HasReturn;
            if (methodModel.HasParameters == false && methodModel.HasReturnTypes == false) signatureFlags |= _MethodSignatureFlags.VoidCall;

            // Build the method signature
            _MethodSignatureHandle signatureHandle = new _MethodSignatureHandle((ushort)parameterHandles.Count, (ushort)localHandles.Count, signatureFlags);

            // Build the method body
            _MethodBodyHandle bodyHandle = new _MethodBodyHandle((ushort)bodyBuilder.MaxStack, (ushort)localHandles.Count);

            // Build the method handle
            _MethodHandle methodHandle = new _MethodHandle(methodModel.SymbolToken, methodModel.DeclaringTypeSymbol.SymbolToken, signatureHandle, bodyHandle);


            // Finally emit the method
            rva = builder.WriteMethodExecutable(methodHandle, parameterHandles.ToArray(), localHandles.ToArray(), bodyBuilder);
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

        //    // Get method flags
        //    MetaMethodFlags methodFlags = methodModel.MethodFlags;

        //    // Write metadata
        //    writer.Write(methodModel.SymbolToken);
        //    writer.Write(methodModel.MethodName);
        //    writer.Write((uint)methodFlags);

        //    // Write return type
        //    if ((methodFlags & MetaMethodFlags.ReturnValue) != 0)
        //    {
        //        // Write return type token
        //        writer.Write(methodModel.ReturnTypeSymbol.SymbolToken);
        //    }

        //    // Write parameters types
        //    if ((methodFlags & MetaMethodFlags.ParamValues) != 0)
        //    {
        //        // Get parameter count
        //        ushort paramCount = (ushort)methodModel.ParameterSymbols.Length;
        //        ushort paramOffset = 0;

        //        // Check for global
        //        if ((methodFlags & MetaMethodFlags.Global) == 0)
        //        {
        //            paramCount -= 1;
        //            paramOffset++;
        //        }

        //        // Number of parameters
        //        writer.Write(paramCount);

        //        // Write all parameters
        //        //foreach(ILocalIdentifierReferenceSymbol parameter in methodModel.ParameterSymbols)
        //        for (int i = paramOffset; i < paramOffset + paramCount; i++)
        //        {
        //            // Get parameter
        //            ILocalIdentifierReferenceSymbol parameter = methodModel.ParameterSymbols[i];

        //            // Build parameter flags
        //            ParameterFlags.ParameterFlags flags = 0;

        //            // Check for by reference
        //            if (parameter.IsByReference == true) flags |= Parameter.ParameterFlags.Reference;

        //            // Check for optional
        //            if (parameter.IsOptional == true) flags |= Parameter.ParameterFlags.DefaultValue;

        //            // Check for variable length
        //            //if(parameter.va)

        //            // Parameter type
        //            writer.Write(parameter.TypeSymbol.SymbolToken);
        //            writer.Write((uint)flags);
        //        }
        //    }

        //    // Write generic types
        //    if ((methodFlags & MetaMethodFlags.Generic) != 0)
        //    {

        //    }

        //    // Get size required for this method image
        //    writer.Flush();
        //    return (int)writer.BaseStream.Position;
        //}

        //public int EmitExecutableModel(BinaryWriter writer = null, string debugInstructionsOutput = null)
        //{
        //    // Check for writer
        //    if (writer == null)
        //    {
        //        // Create memory
        //        Stream executableStream = new MemoryStream();

        //        // Create writer
        //        writer = new BinaryWriter(executableStream);
        //    }

        //    // Start of writer
        //    long offset = writer.BaseStream.Position;

        //    // Write size of method
        //    writer.Write((uint)0);


        //    // Write method handle
        //    _MethodHandle handle = methodModel.MethodHandle;
        //    handle.Write(writer);


        //    _StackHandle[] argLocalHandles = methodModel.MethodArgLocals;

        //    // Emit locals
        //    for (int i = 0; i < argLocalHandles.Length; i++)
        //        argLocalHandles[i].Write(writer);


        //    // Create instruction builder
        //    BytecodeBuilder builder = new BytecodeBuilder(writer);

        //    // Emit instructions
        //    bodyBuilder.EmitExecutionObject(builder);

        //    if (debugInstructionsOutput != null)
        //        builder.ToDebugFile(debugInstructionsOutput);


        //    // Get size
        //    long endOffset = writer.BaseStream.Position;
        //    long size = endOffset - offset;

        //    // Return and write size of instructions
        //    writer.BaseStream.Seek(offset, SeekOrigin.Begin);
        //    writer.Write((uint)size);

        //    // Return to end
        //    writer.BaseStream.Seek(endOffset, SeekOrigin.Begin);

        //    // Get size required for this method image
        //    writer.Flush();
        //    return (int)writer.BaseStream.Position;
        //}
    }
}
