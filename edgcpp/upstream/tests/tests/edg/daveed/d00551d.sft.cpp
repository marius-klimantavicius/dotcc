//remark:GNU __builtin_offsetof
//type:fp
//name:
//options:-DVIRTUAL;fn:;fn
//options_all:--g++ -tused --gnu_version=40000
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

#ifndef VIRTUAL
#define VIRTUAL
#else
#define VIRTUAL virtual
#endif

#define offsetof __builtin_offsetof

struct A {
	int m;
};

struct B1: VIRTUAL A {};

struct B2: VIRTUAL A {};

struct D: B1, B2 {};

int i = offsetof(D, m);
int k = offsetof(D, B1::m);
