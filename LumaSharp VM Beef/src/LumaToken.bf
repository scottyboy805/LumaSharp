using System;

namespace LumaSharpVMBeef;

public enum LumaTokenKind : uint8
{
	Invalid = 0,
	StringReference,

	AssemblyReference,
	MemberReference,
	TypeReference,

	FieldDefinition,
	MethodDefinition,
	TypeDefinition,

	TypeSpecification,
}

public struct LumaToken
{
	// Private
	private uint32 token; 

	// Public
	public uint32 Token => token;

	[Inline]
	public LumaTokenKind Kind => (LumaTokenKind)(token & 0xFF);
	[Inline]
	public uint32 Index => (token >> 8) & 0xFFFFFF;

	// Constructor
	public this(LumaTokenKind kind, uint32 index)
	{
		token = ((uint32)kind) |
			((index & 0xFFFFFF) << 8);
	}
}