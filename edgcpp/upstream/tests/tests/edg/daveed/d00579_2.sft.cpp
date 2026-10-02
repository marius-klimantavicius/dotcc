//remark:GNU C/C++ complex type support
//type:fp
//name:
//options:--gcc:--g++
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

__complex__ double z1;

_Complex float f(__complex long double z2) {
	return z1+z2;
}
