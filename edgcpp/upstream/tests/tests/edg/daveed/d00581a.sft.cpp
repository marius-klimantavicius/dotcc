//remark:Microsoft alignment directives
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
//script:

  #pragma pack(2)
  struct __declspec(align(4)) A {
    char a;
  };
  struct S {
    char s;
    A a;  // Aligned on a 4-byte boundary despite the 2-byte boundary
  };      // packing in effect.  Therefore, sizeof(S) == 8 (not 4).

int v[sizeof(S) == 8];
