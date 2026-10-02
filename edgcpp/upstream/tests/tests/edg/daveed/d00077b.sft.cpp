//remark:Diagnose destructor typedef names
//type:fn
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

struct S {
	typedef ~S();
};

