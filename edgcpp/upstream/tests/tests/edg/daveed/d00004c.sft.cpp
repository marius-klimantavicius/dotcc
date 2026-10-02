/*
//remark:Variadic macros (GNU style)
//type:rp
//name:
//options:
//options_all:--extended_variadic_macros
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
*/

/* Test for GNU variadic macros. */

#if defined __cplusplus
extern "C"
#endif
int printf(char const*, ...);

#define DEBUG(fmt, args...) (printf(fmt , ## args))

int main() {
	DEBUG("%s %s %s?\n", "is", "this", "working");
	DEBUG("Done.\n");
	return 0;
}

