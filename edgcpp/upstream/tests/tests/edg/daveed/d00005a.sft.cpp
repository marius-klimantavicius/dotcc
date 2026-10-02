//remark:Block-extern declarations with internal linkage
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

static void f() {}

void g() {
	extern void f();
	{
		extern void f();
	}
}
