//remark:Microsoft __interface support
//type:fp
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
	void f();
};

struct D: B {
	virtual void f() {}
} d;

