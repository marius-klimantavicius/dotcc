//remark:Microsoft member declarations with type names
//type:fp
//name:
//options:--microsoft_version=1300 -DNEWNEG:--microsoft_version=1310:--microsoft_version=1310 -DNEWNEG=1300;fn
//options_all:--microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

typedef struct S {};
typedef int I;

struct C {
  S();
#ifdef NEWNEG
  I();
#endif
};
