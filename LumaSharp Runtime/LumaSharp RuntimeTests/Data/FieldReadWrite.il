

.assembly: MyAssembly

.type: TestType
{
	.field: testField, i32

	.method: testMethod, void
	{
		.locals: ptr
		.maxstack: 1

		nop
		new, TestType
		st_var_0
		ld_var_0
		ld_i4, 1234
		st_fld, testField
		ld_var_0
		ld_fld, testField
		ret
	}
}