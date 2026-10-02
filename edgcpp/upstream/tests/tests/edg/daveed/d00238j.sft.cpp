//remark:Microsoft __interface support
//type:fn
//name:
//options:
//options_all:--microsoft
//cases:
//source_files:
//input_files:  
//output_files:
//ulimit:
//linker_options:
//execution_args:

__interface I1 {
	struct S {};
};

__interface I2 {
	struct S;
};

struct I2::S {};


__interface I3 {
	template<typename> class X;
	template<typename> void f();
};

