//remark:GNU __builtin_offsetof
//type:fn
//name:
//options:
//options_all:--g++ -tused --gnu_version=40000
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

struct B {
	int i;
};

struct D: private B {
};

void f() {
	__builtin_offsetof(D, i);
}
