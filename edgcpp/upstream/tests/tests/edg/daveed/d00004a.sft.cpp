/*
//remark:Variadic macros (C9X style)
//type:fp
//name:
//options:
//options_all:-P --variadic_macros
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
*/

/* Test for C9X variadic macros. */

#if defined __cplusplus
extern "C"
#endif
int printf(char const*, ...);

#define DEBUG(...) (printf("DEBUG: "), printf(__VA_ARGS__))

int main() {
	DEBUG("%s %s %s?\n", "is", "this", "working");
	DEBUG();
	return 0;
}

