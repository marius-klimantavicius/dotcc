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

int _EDG_NAS_A *pa = 0;
int _EDG_NAS_B *pb = 0;
int _EDG_NAS_C *pc = 0;

int main() {
#ifndef POS
	pa == pc;
#endif
	pa == pb;
}

