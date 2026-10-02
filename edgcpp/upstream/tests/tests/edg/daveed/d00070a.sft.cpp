//remark:Microsoft compat: allow using-decl of compatible but distinct extern "C" decls
//type:fp
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

namespace A {
extern "C" void f ();
}

extern "C" void f ();
using A::f;


void g ()
{
        f ();  // Microsoft issues an ambiguity error here, but we accept it
}              // in now (in Microsoft mode).

