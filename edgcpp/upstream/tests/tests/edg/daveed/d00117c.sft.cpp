//remark:GNU case ranges
//type:fp
//name:
//options:--gcc:--g++:;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

void g(int i) {
	switch (i) {
		case -1 ... 1:
			break;
	}
}

