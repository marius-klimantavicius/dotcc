//remark:Bad designator usage
//type:fn
//name:
//options:
//options_all:--c99
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct A {
	struct X {};
};

struct A a[] = {.x = 37};

