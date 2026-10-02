//options_all:-r -x -tused
//options: --microsoft -n;cp

extern "C" { _inline void f(void) { __asm mov eax, fs:[0x10] } }
extern "C" { _inline void g(void) { __asm { mov eax, fs:[0x10] } } }

