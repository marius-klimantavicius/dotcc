//remark:GNU C++ Variable redeclaration through using-declaration
//type:fp
//name:
//options:--g++:;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

namespace N { int i; }
using N::i;
extern int i;

