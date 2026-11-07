namespace System;

/// <summary>Extensible policy used by runtime reflection to select methods and constructors.</summary>
public virtual class Binder : Object
{
	public this() { }

	public virtual System.Reflection.Method? BindToMethod(System.Type? type, string name, System.Type?[]? parameterTypes)
	{
		if (type == null)
			throw new ArgumentNullException("type");

		System.Type?[] arguments = parameterTypes ?? new System.Type?[0];
		int32 argumentCount = arguments.Length;
		var methods = type.GetMethods();
		System.Reflection.Method? selected = null;
		for (int32 methodIndex = 0; methodIndex < methods.Length; methodIndex++)
		{
			let candidate = methods[methodIndex];
			if (candidate.Name != name)
				continue;
			var candidateTypes = candidate.ParameterTypes;
			if (candidateTypes.Length != argumentCount)
				continue;

			bool matches = true;
			for (int32 parameterIndex = 0; parameterIndex < argumentCount; parameterIndex++)
				if (arguments[parameterIndex] == null || candidateTypes[parameterIndex] != arguments[parameterIndex])
				{
					matches = false;
					break;
				}

			if (matches)
			{
				if (selected != null)
					throw new InvalidOperationException("More than one reflected method matches the requested parameter types.");
				selected = candidate;
			}
		}
		return selected;
	}

	public virtual System.Reflection.Constructor? BindToConstructor(System.Type? type, System.Type?[]? parameterTypes)
	{
		if (type == null)
			throw new ArgumentNullException("type");

		System.Type?[] arguments = parameterTypes ?? new System.Type?[0];
		int32 argumentCount = arguments.Length;
		var constructors = type.GetConstructors();
		System.Reflection.Constructor? selected = null;
		for (int32 constructorIndex = 0; constructorIndex < constructors.Length; constructorIndex++)
		{
			let candidate = constructors[constructorIndex];
			var candidateTypes = candidate.ParameterTypes;
			if (candidateTypes.Length != argumentCount)
				continue;

			bool matches = true;
			for (int32 parameterIndex = 0; parameterIndex < argumentCount; parameterIndex++)
				if (arguments[parameterIndex] == null || candidateTypes[parameterIndex] != arguments[parameterIndex])
				{
					matches = false;
					break;
				}

			if (matches)
			{
				if (selected != null)
					throw new InvalidOperationException("More than one reflected constructor matches the requested parameter types.");
				selected = candidate;
			}
		}
		return selected;
	}
}
