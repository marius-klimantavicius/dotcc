//remark:Named address space qualifiers
//type:fn
//name:
//options:
//options_all:--c99 --embedded_c
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

typedef struct S {
	int a, b;
} S;

void f() {
	(int _EDG_NAS_A[3]){ 1, 2, 3 };
	(S _EDG_NAS_B){ 7, 8 };
}

