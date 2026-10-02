//remark:UPC Extensions
//type:fp
//name:
//options:;fp:--c99;fp:--gcc;fp
//options_all:--upc --upc_threads=4
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void f() {
	static shared int i;
	upc_elemsizeof i;
	upc_elemsizeof(shared int [3]);
}

