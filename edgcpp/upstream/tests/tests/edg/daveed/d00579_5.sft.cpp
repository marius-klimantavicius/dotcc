//remark:GNU C/C++ complex type support
//type:fp
//name:
//options:
//options_all:--g++
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

void f(_Complex double);
void f(_Complex float);

void g() {
	f(1.0F + 2.0*__I__);
	f(1.0F + 2.0F*__I__);
}
