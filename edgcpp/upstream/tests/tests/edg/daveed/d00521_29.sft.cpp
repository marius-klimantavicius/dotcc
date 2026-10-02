//remark:Non-dll fields in dll classes
//type:fn
//name:
//options:--diag_error=field_without_dll_interface
//options_all:--microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct S {
	virtual void f();
};

struct S2 {
	S2();
};

class __declspec(dllexport) C {
	S s;
	S2 s2;
};

