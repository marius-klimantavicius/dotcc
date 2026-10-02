//type:fp
//options_all:--ms_c11 --microsoft_version=1927
//remark:[6.1] Microsoft compatibility: Enable C features with new command-line option
// 3/25/20  [EDGcpfe/22464]
//
// Microsoft compatibility: Enable C features with new command-line option
//
// A change has been made to enable certain C11 features when microsoft_version
// >= 1927 and a new command-line option, --ms_c11, is specified (similar to
// the /std:c11 command-line option for Microsoft Visual Studio).  This change
// enables _Alignof, _Alignas, _Noreturn, and restricted pointers.
// this test case is accepted with --ms_c11 --microsoft_version=1927:
char *restrict rp;
_Noreturn void f();
int _Alignas(128) y;
int x = _Alignof(int);
struct S { int m; };
void g() {
    struct S s = (struct S){0};
}
