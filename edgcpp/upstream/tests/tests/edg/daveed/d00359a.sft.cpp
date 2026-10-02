//remark:Diagnostics on cv-qualified nonclass return types in templates
//type:fp
//name:
//options:
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

template<typename T> struct S {
	int const f();
	T g();
};

S<int const> s;

