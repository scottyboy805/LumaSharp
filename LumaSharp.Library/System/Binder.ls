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
		var methods = type.GetMethods();
		System.Reflection.Method?[] applicable = new System.Reflection.Method?[methods.Length];
		int32 applicableCount = 0;
		for (int32 methodIndex = 0; methodIndex < methods.Length; methodIndex++)
		{
			let candidate = methods[methodIndex];
			if (candidate.Name != name)
				continue;
			if (IsApplicable(candidate.ParameterTypes, arguments))
				applicable[applicableCount++] = candidate;
		}
		if (applicableCount == 0)
			return null;

		System.Reflection.Method? selected = null;
		for (int32 candidateIndex = 0; candidateIndex < applicableCount; candidateIndex++)
		{
			System.Reflection.Method candidate = applicable[candidateIndex]!;
			bool betterThanAll = true;
			for (int32 otherIndex = 0; otherIndex < applicableCount; otherIndex++)
				if (candidateIndex != otherIndex && !IsBetter(candidate.ParameterTypes, applicable[otherIndex]!.ParameterTypes, arguments))
				{
					betterThanAll = false;
					break;
				}
			if (betterThanAll)
			{
				if (selected != null)
					throw new InvalidOperationException("More than one reflected method is the best match for the requested parameter types.");
				selected = candidate;
			}
		}
		if (selected == null)
			throw new InvalidOperationException("More than one reflected method is the best match for the requested parameter types.");
		return selected;
	}

	public virtual System.Reflection.Constructor? BindToConstructor(System.Type? type, System.Type?[]? parameterTypes)
	{
		if (type == null)
			throw new ArgumentNullException("type");

		System.Type?[] arguments = parameterTypes ?? new System.Type?[0];
		var constructors = type.GetConstructors();
		System.Reflection.Constructor?[] applicable = new System.Reflection.Constructor?[constructors.Length];
		int32 applicableCount = 0;
		for (int32 constructorIndex = 0; constructorIndex < constructors.Length; constructorIndex++)
		{
			let candidate = constructors[constructorIndex];
			if (IsApplicable(candidate.ParameterTypes, arguments))
				applicable[applicableCount++] = candidate;
		}
		if (applicableCount == 0)
			return null;

		System.Reflection.Constructor? selected = null;
		for (int32 candidateIndex = 0; candidateIndex < applicableCount; candidateIndex++)
		{
			System.Reflection.Constructor candidate = applicable[candidateIndex]!;
			bool betterThanAll = true;
			for (int32 otherIndex = 0; otherIndex < applicableCount; otherIndex++)
				if (candidateIndex != otherIndex && !IsBetter(candidate.ParameterTypes, applicable[otherIndex]!.ParameterTypes, arguments))
				{
					betterThanAll = false;
					break;
				}
			if (betterThanAll)
			{
				if (selected != null)
					throw new InvalidOperationException("More than one reflected constructor is the best match for the requested parameter types.");
				selected = candidate;
			}
		}
		if (selected == null)
			throw new InvalidOperationException("More than one reflected constructor is the best match for the requested parameter types.");
		return selected;
	}

	private bool IsApplicable(System.ReadOnlySpan<System.Type?> candidateTypes, System.Type?[] arguments)
	{
		if (candidateTypes.Length != arguments.Length)
			return false;
		for (int32 index = 0; index < arguments.Length; index++)
			if (ConversionRank(arguments[index], candidateTypes[index]) < 0)
				return false;
		return true;
	}

	private bool IsBetter(System.ReadOnlySpan<System.Type?> firstTypes, System.ReadOnlySpan<System.Type?> secondTypes,
		System.Type?[] arguments)
	{
		bool firstBetter = false;
		bool secondBetter = false;
		for (int32 index = 0; index < arguments.Length; index++)
		{
			System.Type? firstTarget = firstTypes[index];
			System.Type? secondTarget = secondTypes[index];
			System.Type? argument = arguments[index];
			int32 firstRank = ConversionRank(argument, firstTarget);
			int32 secondRank = ConversionRank(argument, secondTarget);
			if (firstRank < secondRank)
				firstBetter = true;
			else if (secondRank < firstRank)
				secondBetter = true;
			else if (firstTarget != null && secondTarget != null && firstTarget != secondTarget)
			{
				if (secondTarget.IsAssignableFrom(firstTarget) && !firstTarget.IsAssignableFrom(secondTarget) ||
					IsImplicitNumericConversion(firstTarget, secondTarget) && !IsImplicitNumericConversion(secondTarget, firstTarget))
					firstBetter = true;
				else if (firstTarget.IsAssignableFrom(secondTarget) && !secondTarget.IsAssignableFrom(firstTarget) ||
					IsImplicitNumericConversion(secondTarget, firstTarget) && !IsImplicitNumericConversion(firstTarget, secondTarget))
					secondBetter = true;
			}
		}
		return firstBetter && !secondBetter;
	}

	private int32 ConversionRank(System.Type? source, System.Type? target)
	{
		if (source == null || target == null)
			return -1;
		if (source == target)
			return 0;

		if (target.IsAssignableFrom(source))
		{
			System.Type? sourceValue = source.IsNullable ? source.ElementType : source;
			System.Type? targetValue = target.IsNullable ? target.ElementType : target;
			if (sourceValue != null && sourceValue.IsValueType && targetValue != null &&
				(targetValue.IsClass || targetValue.IsInterface))
				return 4;
			return 3;
		}

		System.Type? sourceValueType = source.IsNullable ? source.ElementType : source;
		System.Type? targetValueType = target.IsNullable ? target.ElementType : target;
		if (sourceValueType == null || targetValueType == null || source.IsNullable && !target.IsNullable)
			return -1;
		int32 numericRank = NumericConversionRank(sourceValueType, targetValueType);
		if (numericRank < 0)
			return -1;
		return numericRank + (target.IsNullable && !source.IsNullable ? 1 : source.IsNullable ? 1 : 0);
	}

	private int32 NumericConversionRank(System.Type source, System.Type target)
	{
		string from = NumericTypeName(source);
		string to = NumericTypeName(target);
		int32 enumPenalty = 0;
		if (source.IsEnum)
		{
			System.Type? underlyingType = source.EnumUnderlyingType;
			if (underlyingType == null)
				return -1;
			from = NumericTypeName(underlyingType);
			enumPenalty = 1;
		}
		if (from == to)
			return enumPenalty == 0 ? 1 : enumPenalty;
		if (from == "SByte" && (to == "Int16" || to == "Int32" || to == "Int64" || to == "Single" || to == "Double"))
			return (to == "Single" || to == "Double" ? 2 : 1) + enumPenalty;
		if (from == "Byte" && (to == "Int16" || to == "UInt16" || to == "Int32" || to == "UInt32" || to == "Int64" || to == "UInt64" || to == "Single" || to == "Double"))
			return (to == "Single" || to == "Double" ? 2 : 1) + enumPenalty;
		if (from == "Int16" && (to == "Int32" || to == "Int64" || to == "Single" || to == "Double"))
			return (to == "Single" || to == "Double" ? 2 : 1) + enumPenalty;
		if (from == "UInt16" && (to == "Int32" || to == "UInt32" || to == "Int64" || to == "UInt64" || to == "Single" || to == "Double"))
			return (to == "Single" || to == "Double" ? 2 : 1) + enumPenalty;
		if (from == "Int32" && (to == "Int64" || to == "Double"))
			return (to == "Double" ? 2 : 1) + enumPenalty;
		if (from == "UInt32" && (to == "Int64" || to == "UInt64" || to == "Double"))
			return (to == "Double" ? 2 : 1) + enumPenalty;
		if (from == "Single" && to == "Double")
			return 1 + enumPenalty;
		if (from == "Char" && (to == "UInt16" || to == "Int32" || to == "UInt32" || to == "Int64" || to == "UInt64" || to == "Single" || to == "Double"))
			return (to == "Single" || to == "Double" ? 2 : 1) + enumPenalty;
		return -1;
	}

	private string NumericTypeName(System.Type type)
	{
		if (type.Name == "IntPtr")
			return type.Is64BitNativeInteger ? "Int64" : "Int32";
		if (type.Name == "UIntPtr")
			return type.Is64BitNativeInteger ? "UInt64" : "UInt32";
		return type.Name;
	}

	private bool IsImplicitNumericConversion(System.Type source, System.Type target)
	{
		return NumericConversionRank(source, target) >= 0;
	}
}
