//remark:GNU C block-extern declaration check
//type:fn
//name:
//options:--gcc:--c;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

int x;

void f() {
	unsigned int x;
}

unsigned int x;
