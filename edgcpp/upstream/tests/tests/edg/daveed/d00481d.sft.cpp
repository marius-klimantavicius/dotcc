/*
//remark:UPC warnings
//type:fp
//name:
//options:
//options_all:--upc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
*/

int g();

int t() {
	int k;
	for (k = 0; k<10; ++k) {
		return g();
	}
}

int f() {
	int k;
	upc_forall(k = 0; k<10; ++k; continue) {
		return g();
	}
}

