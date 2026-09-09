using LLVMSharp.Interop;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics;

namespace LumaSharp.Compiler.Emit.LLVM
{
    internal sealed partial class LLVMCodeGen
    {
        // Private
        private SemanticModel model;
        private LLVMModuleRef module;
        private LLVMBuilderRef builder;
        private LLVMContextRef context;

        // Properties
        public LLVMModuleRef Module => module;

        // Constructor
        public LLVMCodeGen(SemanticModel model)
        {
            this.model = model;
            this.module = LLVMModuleRef.CreateWithName("main");
            this.builder = LLVMBuilderRef.Create(module.Context);
            this.context = module.Context;
        }

        // Methods
        public void TestEmit()
        {
            //LLVMTypeRef returnType = LLVMTypeRef.Int32;

            //LLVMFunctionBuilder mainFunc = new(module, builder, "main", returnType, []);

            //LLVMTypeRef functionType = LLVMTypeRef.CreateFunction(returnType, [], false);

            //LLVMValueRef function = module.AddFunction("main", functionType);
            //var end = function.AppendBasicBlock(".body");

            MethodSyntax method = Syntax.Method("main", Syntax.TypeReference(ReservedType.Int32))
                .WithBody(
                    Syntax.Condition(Syntax.Literal(false)).WithInlineStatement(Syntax.Return(Syntax.Literal(20))),
                    Syntax.LocalVariable(Syntax.TypeReference(ReservedType.Int32), "myVar", Syntax.VariableAssignment(Syntax.Literal(69))),
                    Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.VariableAssignment(Syntax.Literal(496))),
                    Syntax.Return(Syntax.VariableReference("myVar"))
                );

            // Emit the method
            EmitMethod(method, null);

            //builder.PositionAtEnd(end);
            //EmitStatement(Syntax.While(Syntax.Literal(true), Syntax.Return(Syntax.Literal(123))), mainFunc);
            //EmitStatement(Syntax.Condition(Syntax.Literal(false)).WithInlineStatement(Syntax.Return(Syntax.Literal(20))), mainFunc);
            //EmitStatement(Syntax.LocalVariable(Syntax.TypeReference(ReservedType.Int32), "myVar", Syntax.VariableAssignment(Syntax.Literal(69))), mainFunc);
            //EmitStatement(Syntax.Assign(Syntax.VariableReference("myVar"), Syntax.VariableAssignment(Syntax.Literal(496))), mainFunc);
            //EmitStatement(Syntax.Return(Syntax.VariableReference("myVar")), mainFunc);
        }

        private LLVMValueRef GetLLVMCoerceValue(LLVMTypeRef targetType, LLVMValueRef value, string name)
        {
            // Check for same
            if (value.TypeOf == targetType)
                return value;

            // Convert integer
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && targetType.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                uint sourceWidth = value.TypeOf.IntWidth;
                uint targetWidth = targetType.IntWidth;

                if (sourceWidth < targetWidth)
                    return builder.BuildSExt(value, targetType, name);
                if (sourceWidth > targetWidth)
                    return builder.BuildTrunc(value, targetType, name);
            }

            // Check for float type
            if ((value.TypeOf.Kind == LLVMTypeKind.LLVMFloatTypeKind || value.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind) &&
                (targetType.Kind == LLVMTypeKind.LLVMFloatTypeKind || targetType.Kind == LLVMTypeKind.LLVMDoubleTypeKind))
            {
                if (value.TypeOf.Kind == LLVMTypeKind.LLVMFloatTypeKind && targetType.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
                {
                    return builder.BuildFPExt(value, targetType, name);
                }

                if (value.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind && targetType.Kind == LLVMTypeKind.LLVMFloatTypeKind)
                {
                    return builder.BuildFPTrunc(value, targetType, name);
                }
            }

            // Check for inter to float kind
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind &&
                (targetType.Kind == LLVMTypeKind.LLVMFloatTypeKind || targetType.Kind == LLVMTypeKind.LLVMDoubleTypeKind))
            {
                return builder.BuildSIToFP(value, targetType, name);
            }

            if (value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind && targetType.Kind == LLVMTypeKind.LLVMStructTypeKind)
            {
                return builder.BuildLoad2(targetType, value, name);
            }

            throw new NotImplementedException();
        }

        //private LLVMTypeRef GetLLVMTypeRef(TypeReferenceSyntax type)
        //{
        //    // Default pointer type
        //    LLVMTypeRef llvmType = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);

        //    // Check for built in
        //    if (type.IsReservedType == true)
        //    {
        //        // Get the build in type
        //        type.GetReservedType(out ReservedType builtinType);

        //        // Check kind
        //        switch(builtinType)
        //        {
        //            case ReservedType.Bool: llvmType = LLVMTypeRef.Int1; break;
        //            case ReservedType.Char: llvmType = LLVMTypeRef.Int16; break;

        //            case ReservedType.Int8:
        //            case ReservedType.UInt8: llvmType = LLVMTypeRef.Int8; break;
        //            case ReservedType.Int16:
        //            case ReservedType.UInt16: llvmType = LLVMTypeRef.Int16; break;
        //            case ReservedType.Int32:
        //            case ReservedType.UInt32: llvmType = LLVMTypeRef.Int32; break;
        //            case ReservedType.Int64:
        //            case ReservedType.UInt64: llvmType = LLVMTypeRef.Int64; break;
        //            case ReservedType.Float: llvmType = LLVMTypeRef.Float; break;
        //            case ReservedType.Double: llvmType = LLVMTypeRef.Double; break;
        //        }
        //    }

        //    // Check for array
        //    if(type.IsArrayType == true)
        //    {
        //        llvmType = LLVMTypeRef.CreatePointer(llvmType, 0);
        //    }
        //    return llvmType;
        //}

        internal static LLVMTypeRef GetLLVMTypeRef(ITypeSymbol type)
        {
            // Check for built in
            if(type.IsReserved == true)
            {
                // Select based on built in type
                return type.ReservedType switch
                {
                    ReservedType.Void => LLVMTypeRef.Void,

                    ReservedType.Bool => LLVMTypeRef.Int1,
                    ReservedType.Char => LLVMTypeRef.Int16,

                    ReservedType.Int8 => LLVMTypeRef.Int8,
                    ReservedType.UInt8 => LLVMTypeRef.Int8,
                    ReservedType.Int16 => LLVMTypeRef.Int16,
                    ReservedType.UInt16 => LLVMTypeRef.Int16,
                    ReservedType.Int32 => LLVMTypeRef.Int32,
                    ReservedType.UInt32 => LLVMTypeRef.Int32,
                    ReservedType.Int64 => LLVMTypeRef.Int64,
                    ReservedType.UInt64 => LLVMTypeRef.Int64,
                    ReservedType.Float => LLVMTypeRef.Float,
                    ReservedType.Double => LLVMTypeRef.Double,

                    ReservedType.String => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
                    ReservedType.Object => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),

                    _ => throw new NotSupportedException(type.ReservedType.ToString()),
                };
            }

            // Create pointer type
            return LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
        }
    }
}
