//remark:Diagnostics for nonstandard anonymous unions
//type:fn
//name:
//options:-DPOS;fp:;fn
//options_all:--g++
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

  struct S { S(); };

#ifndef POS
  union U {
    struct {
      S s;
    };
  } x;
#endif

  struct X {
    struct {
      const int i;
    };
  };
