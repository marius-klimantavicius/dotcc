//remark:Linkage of builtin functions
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

int (*pf)(int) = &__builtin_abs;
extern "C" int (*pf2)(int) = &__builtin_abs;

extern "C" typedef int F(int);

F *pf3 = & __builtin_abs;
