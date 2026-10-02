//remark:Deprecated typedefs
//type:fn
//name:
//options:
//options_all:--gcc --diag_error deprecated_entity
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

typedef int T1 __attribute__ ((deprecated));
T1 x;           /* warning */
typedef T1 T2;  /* warning */
T2 y;           /* no warning */
typedef T1 T3 __attribute__ ((deprecated));
T3 z __attribute__ ((deprecated));

