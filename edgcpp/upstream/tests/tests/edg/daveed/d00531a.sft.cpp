//remark:Microsoft C mode redeclarations
//type:fp
//name:
//options:
//options_all:--microsoft --c
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

  unsigned int i;
  unsigned long i;  // Already accepted in Microsoft C mode (with a warning).

  typedef unsigned long UL;
  UL i;  // Now also accepted in Microsoft C mode (with a warning).
         // Previously an error.
