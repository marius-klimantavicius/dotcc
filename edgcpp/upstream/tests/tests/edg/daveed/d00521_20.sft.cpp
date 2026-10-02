//remark:Microsoft dllimport/dllexport compatibility
//type:fn
//name:
//options::-DSINGLE;fn
//options_all:--microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

__declspec(dllimport) void f() {}
#ifndef SINGLE
__declspec(dllimport) void f() {}
#endif

struct __declspec(dllimport) X {
   void f();
};

__declspec(dllimport) void X::f() {}
#ifndef SINGLE
__declspec(dllimport) void X::f() {}
#endif

