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

__interface A {
	typedef int T;
	enum E { e1, e2 };
};

__interface B {
	__interface N;
	struct S;
};

void g() {
	__interface I;
}
