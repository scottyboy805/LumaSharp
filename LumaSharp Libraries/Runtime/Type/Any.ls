
export type any override
{
	// Properties
	export i32 Hash override => read; 

	export string String override => read;

	// Constructor
	protected this() override;

	// Methods
	export bool Equals(any other) override;
}