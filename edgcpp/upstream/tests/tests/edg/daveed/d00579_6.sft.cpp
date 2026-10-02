//remark:GNU C/C++ complex type support
//type:fp
//name:
//options:--gcc:--g++:-DNEG --gcc;fn:-DNEG --g++;fn
//options_all:--diag_warn complex_integral_type --gnu_version=40000
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

__complex float z1 = 1.2fj;
__complex float z2 = 1.2jf;
#ifdef NEG
__complex float z3 = 1Lj;
#endif
__complex float z4 = 1e6j;
#ifdef NEG
__complex float z5 = 1je6;
__complex float z6 = 1.2ifj;
__complex float z7 = 1LLj;
#endif
__complex float z8 = 1.0J;
#ifdef NEG
__complex float z9 = 1FJ;
#endif
__complex float z10 = 0J;
__complex float z11 = 1J;
__complex float z12 = 12J;

