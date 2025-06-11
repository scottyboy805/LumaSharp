using LumaSharp.Runtime.Handle;
using System.Runtime.InteropServices;
using System.Text;

namespace LumaSharp.Runtime.Reader
{
    public sealed class ILReader
    {
        // Type
        private struct FieldDefinition
        {
            // Public
            public string Name;
            public _TokenHandle Token;
            public TokenReference TypeToken;
        }

        private struct MethodDefinition
        {
            // Public
            public string Name;
            public _TokenHandle Token;
            //public _TokenHandle[] ReturnTypeToken;
            public BytecodeReader BodyReader;
        }

        private struct TypeDefinition
        {
            // Public
            public string Name;
            public _TokenHandle Token;
            public List<FieldDefinition> Fields;
            public List<MethodDefinition> Methods;
        }

        private struct TokenReference
        {
            // Public
            public string Identifier;
            public _TokenHandle ResolvedToken;
        }

        // Private
        private readonly TextReader reader;

        private string assemblyName;
        private List<TypeDefinition> types = new();
        private int typeCounter = 0;
        private int fieldCounter = 0;
        private int methodCounter = 0;

        // Constructor
        public ILReader(TextReader reader)
        {
            // Check for null
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            this.reader = reader;

            ReadIL();
        }

        // Methods
        public unsafe AssemblyContext GenerateAssembly(AppContext appContext)
        {
            // Create the assembly
            AssemblyContext assemblyContext = new AssemblyContext(appContext);
                        

            // Resolve all tokens
            for(int i = 0; i < types.Count; i++)
            {
                // Process all fields
                for(int j = 0; j < types[i].Fields.Count; j++)
                {
                    // Check for resolved
                    if (types[i].Fields[j].TypeToken.ResolvedToken.IsNil == true)
                    {
                        // Get the field
                        FieldDefinition fieldDef = types[i].Fields[j];

                        // Need to resolve
                        fieldDef.TypeToken.ResolvedToken = ResolveToken(
                            fieldDef.TypeToken.Identifier);

                        // Update the field definition
                        types[i].Fields[j] = fieldDef;
                    }
                }
            }


            // First define all types
            foreach(TypeDefinition typeDef in types)
            {
                // Calculate size
                uint typeSize = 0;

                foreach(FieldDefinition fieldDef in typeDef.Fields)
                {
                    // Increase type size
                    typeSize += fieldDef.TypeToken.ResolvedToken.Kind == TokenKind.PrimitiveTypeReference
                        ? RuntimeType.GetTypeSize((RuntimeTypeCode)fieldDef.TypeToken.ResolvedToken)
                        : RuntimeType.GetTypeSize(RuntimeTypeCode.Ptr);

                    // Create type handle
                    _TypeHandle typeHandle = new _TypeHandle(typeDef.Token, typeSize);

                    // Create native memory
                    _TypeHandle* ptr = (_TypeHandle*)NativeMemory.Alloc((nuint)Marshal.SizeOf<_TypeHandle>());

                    // Copy value
                    *ptr = typeHandle;

                    // Define the type handle
                    assemblyContext.typeHandles[typeDef.Token] = (IntPtr)ptr;
                }
            }


            // Finally define all members
            foreach (TypeDefinition typeDef in types)
            {
                // Define all fields
                foreach (FieldDefinition fieldDef in typeDef.Fields)
                {
                    // Create the field handle
                    _FieldHandle fieldHandle = new _FieldHandle(
                        fieldDef.Token,
                        default,
                        0,
                        ResolveTypeHandle(assemblyContext, fieldDef.TypeToken.ResolvedToken));

                    // Create memory
                    _FieldHandle* ptr = (_FieldHandle*)NativeMemory.Alloc((nuint)Marshal.SizeOf<_FieldHandle>());

                    // Copy value
                    *ptr = fieldHandle;

                    // Define the field
                    assemblyContext.fieldHandles[fieldDef.Token] = (IntPtr)ptr;
                }

                // Define all methods
                foreach(MethodDefinition methodDef in typeDef.Methods)
                {
                    // Create the method handle
                    _MethodHandle* methodHandle = methodDef.BodyReader.GenerateMethod(
                        ResolveToken);

                    // Define the method
                    assemblyContext.methodHandles[methodDef.Token] = (IntPtr)methodHandle;
                }
            }

            return assemblyContext;
        }

        private _TokenHandle ResolveToken(string identifier)
        {
            // Check for type
            foreach(TypeDefinition typeDef in types)
            {
                // Check for matching name
                if(typeDef.Name == identifier)
                    return typeDef.Token;

                // Check all fields
                foreach(FieldDefinition fieldDef in typeDef.Fields)
                {
                    // Check for matching name
                    if(fieldDef.Name == identifier)
                        return fieldDef.Token;
                }

                // Check all methods
                foreach(MethodDefinition methodDef in typeDef.Methods)
                {
                    // Check for matching name
                    if(methodDef.Name == identifier)
                        return methodDef.Token;
                }
            }
            throw new Exception("Could not resolve metadata token: " + identifier);
        }

        private unsafe _TypeHandle ResolveTypeHandle(AssemblyContext assemblyContext, _TokenHandle typeToken)
        {
            // Check kind
            if(typeToken.Kind == TokenKind.PrimitiveTypeReference)
                return new _TypeHandle((RuntimeTypeCode)typeToken);

            // Try to lookup type
            return *(_TypeHandle*)assemblyContext.typeHandles[typeToken];
        }

        private void ReadIL()
        {
            // Read all lines
            while (reader.Peek() != -1)
            {
                // Read the line
                string line = reader.ReadLine()
                    .Trim();

                // Check for empty - ignore whitespace
                if (string.IsNullOrEmpty(line) == true)
                    continue;

                // Split by colon
                string[] split = line.Split(':');

                // Check for no operand
                if (split.Length < 2)
                    throw new FormatException("Expected assembly or type info: " + line);

                string content = split[1].Trim();

                // Select type
                switch (split[0].Trim())
                {
                    case ".assembly":
                        {
                            ReadILAssemblyInfo(content);
                            break;
                        }
                    case ".type":
                        {
                            ReadILType(content);
                            break;
                        }
                    default: throw new FormatException("Unexpected root element: " + line);
                }
            }
        }

        private void ReadILAssemblyInfo(string content)
        {
            // Get the assembly name
            assemblyName = content;
        }

        private void ReadILType(string content)
        {
            // Get the type name
            string typeName = content;

            // Read block
            TextReader typeBodyReader = ReadILBlock(reader);

            // Create the type token
            _TokenHandle typeToken = _TokenHandle.TypeDef(typeCounter++);

            // Create the type
            TypeDefinition typeDef = new TypeDefinition
            {
                Token = typeToken,
                Name = typeName,
                Fields = new(),
                Methods = new(),
            };

            // Read the type body
            ReadILType(typeBodyReader, typeDef);

            // Add type
            types.Add(typeDef);
        }

        private void ReadILType(TextReader reader, TypeDefinition typeDef)
        {
            // Read all lines
            while (reader.Peek() != -1)
            {
                // Read the line
                string line = reader.ReadLine()
                    .Trim();

                // Check for empty - ignore whitespace
                if (string.IsNullOrEmpty(line) == true)
                    continue;

                // Split by colon
                string[] split = line.Split(':');

                // Check for no operand
                if (split.Length < 2)
                    throw new FormatException("Expected member info: " + line);

                string content = split[1].Trim();

                // Select type
                switch (split[0].Trim())
                {
                    case ".field":
                        {
                            // Read the field
                            typeDef.Fields.Add(ReadILField(content));
                            break;
                        }
                    case ".method":
                        {
                            // Read the method
                            typeDef.Methods.Add(ReadILMethod(content, reader));
                            break;
                        }
                    default: throw new FormatException("Unexpected type element: " + line);
                }
            }
        }

        private FieldDefinition ReadILField(string content)
        {
            // Split by comma
            string[] split = content.Split(',');

            // Get name
            string fieldName = split[0].Trim();

            // Check for type
            if (split.Length < 2)
                throw new FormatException("Expected field type: " + content);

            // Get the type identifier
            string fieldType = split[1].Trim();

            // Create the field token
            _TokenHandle fieldToken = _TokenHandle.FieldDef(fieldCounter++);

            // Try to parse
            _TokenHandle resolvedToken = default;

            // We can resolve runtime type codes here
            if (fieldType.All(char.IsDigit) == false && Enum.TryParse(fieldType, true, out RuntimeTypeCode typeCode) == true)
                resolvedToken = typeCode;

            // Create the definition
            return new FieldDefinition
            {
                Token = fieldToken,
                Name = fieldName,
                TypeToken = new TokenReference
                {
                    Identifier = fieldType,
                    ResolvedToken = resolvedToken
                },
            };
        }

        private MethodDefinition ReadILMethod(string content, TextReader reader)
        {
            // Split by comma
            string[] split = content.Split(',');

            // Get name
            string methodName = split[0].Trim();

            // Check for type
            if (split.Length < 2)
                throw new FormatException("Expected method return type: " + content);

            // Create the method token
            _TokenHandle methodToken = _TokenHandle.MethodDef(methodCounter++);

            // Read body
            TextReader methodBodyReader = ReadILBlock(reader);

            // Create the definition
            return new MethodDefinition
            {
                Token = methodToken,
                Name = methodName,
                BodyReader = new BytecodeReader(methodBodyReader),
            };
        }

        private TextReader ReadILBlock(TextReader reader)
        {
            // Read until start
            int current = default;
            while ((current = reader.Peek()) != '{')
            {
                // Check for eof
                if (current == -1)
                    throw new FormatException("Expected '{'");

                reader.Read();
            }

            // Read the {
            reader.Read();
            int depth = 0;

            // Create the builder
            StringBuilder builder = new();

            // Read until }
            while ((current = reader.Peek()) != '}' || depth > 0)
            {
                // Check for eof
                if (current == -1)
                    throw new FormatException("Expected '}'");

                if (current == '{') depth++;
                if (current == '}') depth--;

                builder.Append((char)reader.Read());
            }

            // Read the '}'
            reader.Read();

            // Create a string reader
            return new StringReader(builder.ToString());
        }
    }
}
