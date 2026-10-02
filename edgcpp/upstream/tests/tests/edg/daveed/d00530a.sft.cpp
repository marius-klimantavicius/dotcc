//remark:C99 void parameter list
//type:fp
//name:
//options:--c99:--c99 --strict:--c -A;fn
//options_all:--diag_error=nonstd_void_param_list
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

typedef void V;
void f(V) {}

