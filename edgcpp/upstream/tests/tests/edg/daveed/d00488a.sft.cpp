//remark:Extraneous qualifiers on member template declarations
//type:fp
//name:
//options:--g++:;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct S {
	void S::g() {}
	template<typename T> void S::f() {}
};
