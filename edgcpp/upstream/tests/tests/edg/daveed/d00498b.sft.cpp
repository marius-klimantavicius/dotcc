//remark:GNU C block-extern declaration check
//type:fp
//name:
//options:--gcc --gnu_version=30300:--c;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void f() {
	extern unsigned g();
	extern float x;
}
int g();
int x;
