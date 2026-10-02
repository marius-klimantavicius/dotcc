//remark:GNU C/C++ complex type support
//type:cp
//name:
//options:;cp:-DNEG1;fn:-DNEG2;cp:-DNEG3;fn:-DNEG4;fn
//options_all:--g++ --gnu_version=40000
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

struct S {
	operator double();
};

void f(S s) {
#ifdef NEG4
	_Complex double z = s;
#endif
	__imag 1.0;
	float f = 1.0;
	__real f = 2.0;
#ifdef NEG1
	__imag f = 2.0;
#endif
#ifdef NEG2
	__imag 1;
#endif
#ifdef NEG3
	__real s;
#endif
}
