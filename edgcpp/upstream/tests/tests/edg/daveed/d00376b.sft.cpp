//remark:Deprecated typedefs
//type:fn
//name:
//options:
//options_all:--microsoft --diag_error deprecated_entity
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

typedef int __declspec(deprecated) T1;
T1 x;           /* warning */
typedef T1 T2;  /* warning */
T2 y;           /* no warning */
__declspec (deprecated) typedef T1 T3;
__declspec (deprecated) T3 z;

