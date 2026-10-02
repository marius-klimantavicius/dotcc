//remark:UPC warnings
//type:fp
//name:
//options::--diag_error=exit_forall;fn
//options_all:--upc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void f() {
	int k;
	upc_forall(k = 0; k<10; ++k; continue) {
		break;
	}
}

