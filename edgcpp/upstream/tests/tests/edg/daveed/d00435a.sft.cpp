//remark:__declspec on fields
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

struct S {
        __declspec(dllimport) int i1;
        __declspec(dllexport) int i2;
        __declspec(thread) int i3;
        __declspec(naked) int i4;
        __declspec(nothrow) int i5;
        __declspec(novtable) int i6;
        __declspec(noreturn) int i7;
        __declspec(noinline) int i8;
        __declspec(selectanu) int i9;
};
