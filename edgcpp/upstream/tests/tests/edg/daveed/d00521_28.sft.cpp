//remark:Microsoft dllimport/dllexport treatment
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

struct __declspec(dllexport) X {
	__declspec(dllexport) void f();
};

template<class T> struct __declspec(dllexport) S {
	__declspec(dllexport) void f();
};

