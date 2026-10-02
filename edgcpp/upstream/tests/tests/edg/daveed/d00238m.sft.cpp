//remark:Microsoft __interface support
//type:fp
//name:
//options:
//options_all:--microsoft -x
//cases:
//source_files:
//input_files:  
//output_files:
//ulimit:
//linker_options:
//execution_args:

__interface I {
	void* f(size_t) throw();
};
struct D : public I {
	virtual void* f(size_t) throw() { return 0; }
};
