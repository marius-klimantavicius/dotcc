//remark:Fall-through goto position
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

void f(int x) {
	switch (x) {
		case 0: x = 1;
		case 1: x = 0;
	}
}
