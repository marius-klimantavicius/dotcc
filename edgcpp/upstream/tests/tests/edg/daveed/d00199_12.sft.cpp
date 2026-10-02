//remark:C++ VLAs
//type:fp
//name:
//options:--g++;fp:--c99;fp
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
	(double (*)[n][n]){ 0 };
}
