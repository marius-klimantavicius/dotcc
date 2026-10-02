//remark:Named address space qualifiers
//type:fn
//name:
//options:;fn:-DPOS;fp
//options_all:--c99 --embedded_c
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct S {
	_EDG_NAS_A int **p;
};

#ifndef POS

struct S2 {
	int *_EDG_NAS_A p;
};

typedef int F();
F _EDG_NAS_C *pf;
F const *pf2;

#endif

