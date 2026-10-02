//remark:GNU C case ranges
//type:fp
//name:
//options:
//options_all:--gcc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:



void f(int k) {
	switch (k) {
#if 0
	case -1 ... -10:
#endif
	case 'b' ... 100LL:
	case 'a':
	case 'i' ... 'm':
	case 'z' ... 'z':
		++k;
	}
}

