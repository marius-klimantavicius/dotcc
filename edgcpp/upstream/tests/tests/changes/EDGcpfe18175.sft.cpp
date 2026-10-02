//type:fp
//options_all:--clang --ms_extensions --clang --ms_extensions --clang --ms_extensions
//remark:[4.14] Clang compatibility: Builtin functions with --clang --ms_extensions
// 4/5/17   [EDGcpfe/18175]
//
// Clang compatibility: Builtin functions with --clang --ms_extensions
//
// A change has been made to enable "Microsoft builtins" when ms_extensions
// is TRUE (previously they were only enabled when microsoft_mode was TRUE).
// A separate change has also been made to disable the diagnostic that occurs
// when a difference in dllimport/dllexport flags is detected on a builtin
// function.
__declspec(dllimport) __declspec(noreturn) void __cdecl _exit(int _Code);
void f() {
  __debugbreak();
}
