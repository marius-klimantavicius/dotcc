//remark:GNU asm symbolic operands
//type:fn
//name:
//options:--gcc:--g++:;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

void f() {
	register float a, r;
	asm("fsinx %%[ In ],%0" : [Out]"=f"(r) : "f"(a));
}
