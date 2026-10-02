//remark:__declspec attributes separator
//type:fp
//name:
//options:-DNOSEP;fp:;fp
//options_all:--microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

#ifndef NOSEP
#define SEP ,
#else
#define SEP /*NOthing*/
#endif
struct __declspec(intrin_type SEP align(8)) S { char a; };
struct __declspec(intrin_type SEP align(8) SEP) T { char a; };
