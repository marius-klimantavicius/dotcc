//remark:GNU __builtin_constant_p
//type:fn
//name:
//options:--g++:--gcc:--g++ -DPOS;fp:--gcc -DPOS;fp
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

int i;
int const c = 3;
#ifndef POS
int a1[1/__builtin_constant_p(i)];
int a2[1/__builtin_constant_p(c)];
#endif
int a3[1/__builtin_constant_p(3)];
