
//remark:Packed types and back ends
//type:fp
//name:
//options:--gcc;fp:--g++;fp:--gcc -DNEG;fn: --g++ -DNEG;fn
//options_all:--multi_trans
//cases:
//source_files: d00385a_2.c
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

#ifdef NEG
#define WEAK
#else
#define WEAK __attribute__((weak))
#endif

WEAK void f() {}
WEAK short s;
void g() { f(); }

