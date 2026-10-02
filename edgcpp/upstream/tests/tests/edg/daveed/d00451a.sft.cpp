//remark:Arrays of abstract class types
//type:fn
//name:
//options:;fn:-A;fn:--sun;fp:--g++;fp:--microsoft;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct S {
	void f(S[3]);
	virtual ~S() = 0;
};
