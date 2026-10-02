//remark:Microsoft mode: duplicate dllimport definition
//type:fn
//name:
//options:
//options_all:--microsoft_bugs --diag_warning=decl_modifiers_invalid_for_this_decl
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

#if 1
__declspec(dllimport) void f() {}
__declspec(dllimport) void f() {}
#endif

struct __declspec(dllimport) X {
	void f();
};

__declspec(dllimport) void X::f() {}
__declspec(dllimport) void X::f() {}

