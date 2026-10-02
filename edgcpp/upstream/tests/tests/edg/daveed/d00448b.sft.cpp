//remark:Sun link scope specifiers
//type:fp
//name:
//options:;fp:-DNEG;fn
//options_all:--sun --sun_linker_scope
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

#pragma disable_ldscope
int *__global;
#pragma enable_ldscope
__global int i;

#ifdef NEG
int __hidden;
#endif

