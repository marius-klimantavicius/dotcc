//options_all:-r -x -tused
//options: --strict;cn:;cp

extern "C" void f();              // f's type has extern "C" linkage
void (*pf)()                      // pf points to an extern "C++" function
             = &f;                // Error under the new rules

