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
	friend class X;
};

__interface I2 {
	friend X;
};

__interface I3 {
	friend void f();
};

__interface I4 {
	friend void f() {}
};

