//remark:Switch breaks
//type:fp
//name:
//options:
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

void f(int n) {
	switch (n) {
		case 3:
			break;
		default:
			while(1) break;
	}
}

