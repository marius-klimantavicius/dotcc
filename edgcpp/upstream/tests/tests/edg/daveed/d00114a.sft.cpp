//remark:GNU C variable redeclaration with cv-mismatch
//type:fp
//name:
//options:
//options_all:--gcc --gnu_version=29500 --diag_warning=expr_not_a_modifiable_lvalue
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

extern int a;
int const a = 13;

extern int b = 13;
int const b;

int const c = 13;
int c;

void f() {
	a = 2;
	b = 2;
	c = 2;

}

