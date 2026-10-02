//remark:Real instantiations during prototype instantiations
//type:fp
//name:
//options:
//options_all:--parse
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

template<class T> struct S {
	void f1() { S<int> s; s.f2(0); }
	void f2(int) {}
};

