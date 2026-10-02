//remark:GNU attributes and redeclarations
//type:fn
//name:
//options:
//options_all:--gcc -diag_error noreturn_function_does_return
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void f() __attribute__((noreturn));
__attribute__((const)) void f() {}
