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

template<class T> struct A {};

void f(A<__complex double>) {}

void f(__complex double z1) {
	-z1;
	+z1;
	z1 + 3.0;
}

