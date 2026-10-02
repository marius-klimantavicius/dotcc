//remark:Named-register storage class
//type:fn
//name:
//options:-A;fn:;fn
//options_all:--named_registers --c99
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

register _EDG_REG_2 int r1;

void f() {
	int *p = &r1;
}
