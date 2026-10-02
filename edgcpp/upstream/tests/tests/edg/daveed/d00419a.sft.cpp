//remark:WinMain and wWinMain calling conventions
//type:fp
//name:
//options:;fp:-DNEG;fn
//options_all:--microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

#ifdef NEG
#define CC __cdecl
#else
#define CC __stdcall
#endif

void WinMain();
void CC WinMain() {}

void wWinMain();
void CC wWinMain() {}

#ifdef NEG
void xWinMain();
void __stdcall xWinMain() {}
#endif
