//remark:C++ VLAs
//type:fp
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

struct S {
	S();
	~S();
};

void f(int n) {
	S x[n];
}
