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

  void __stdcall WinMain() {}    
  void WinMain();   // redeclaration error

#ifdef NEG
void __stdcall f() {}
void f();
#endif
