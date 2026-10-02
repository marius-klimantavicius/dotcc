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

template<class T> void f(T s) {
	__real s;
	__real T::f();
}
