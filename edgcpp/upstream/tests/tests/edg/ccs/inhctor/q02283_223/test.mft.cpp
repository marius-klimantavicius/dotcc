//remark:Trans_corresp checking in Microsoft mode
//type:fp
//name:
//options::-DNEG;fp
//options_all:--microsoft --multi_trans
//cases:
//source_files:polyspace20_2.c
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

struct S;

#ifdef NEG
struct S {};
#endif
