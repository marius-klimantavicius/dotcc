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



struct S {};

void f(int n) {
	(int (S::*)[n])0;
	(int (*S::*)[n])0;
	typedef int T[n];
	(T *S::**)0;
}
