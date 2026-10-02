//remark:C++ VLAs
//type:fn
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

void f(int n) {
	typedef int (*T)[n];
	T (*pf)();
}
