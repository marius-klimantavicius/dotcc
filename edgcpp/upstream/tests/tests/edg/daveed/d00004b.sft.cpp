/*
//remark:Variadic macros (GNU style)
//type:fp
//name:
//options:
//options_all:-P --extended_variadic_macros
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

#define DEBUG(args...) (printf("DEBUG: "), printf(args))

int main() {
	DEBUG("%s %s %s?\n", "is", "this", "working");
	return 0;
}

