//type:fp
//options_all:--c++17 -A -tused
//
class C { };
void h(int *(C[10])); // void h(int *(*_fp)(C _parm[10]));
// not: void h(int *C[10]);
