//remark:C++ VLAs
//type:fn
//name:
//options:
//options_all:--g++ -x
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

void f(int n) {
	void g() throw(double(*)[n]);
}
