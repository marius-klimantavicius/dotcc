//remark:C99: indirect flexible members
//type:fp
//name:
//options:;fp:--strict;fn
//options_all:--c99
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct S {
	int i;
	int a[];
};

struct X {
	int j;
	struct S s;
};

