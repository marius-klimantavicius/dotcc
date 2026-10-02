//remark:GNU C block-extern declaration check
//type:fn
//name:
//options:--gcc:--c
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

int g();
int x;
void f() {
	extern unsigned g();
	extern float x;
}
