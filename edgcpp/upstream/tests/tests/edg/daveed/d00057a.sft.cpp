//remark:Allow function redeclaration with incompatible calling convention
//type:fp
//name:
//options:
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

#ifdef __hpux
typedef void F();
  extern "C" F f;
  extern "C" void f() {}
#endif
