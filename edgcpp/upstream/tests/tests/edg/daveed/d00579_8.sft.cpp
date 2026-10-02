//remark:GNU C/C++ complex type support
//type:fn
//name:
//options:
//options_all:--g++
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

struct S {};

double operator __real(S const&);

