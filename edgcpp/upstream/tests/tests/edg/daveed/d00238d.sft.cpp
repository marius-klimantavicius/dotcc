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

__interface B {
	void f() {}
};

B b;

__interface A {};

A a;

__interface V {
	void f() = 0;
};
