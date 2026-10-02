//remark:__builtin_classify_type and fixed-point expressions
//type:fn
//name:
//options:
//options_all:--gcc --fixed_point
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void f() {
	_Fract x;
	__builtin_classify_type(x);
}

