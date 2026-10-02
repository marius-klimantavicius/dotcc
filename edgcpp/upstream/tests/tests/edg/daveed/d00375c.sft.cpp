//remark:GNU template attributes
//type:fp
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

template<typename T> struct __attribute__((deprecated)) S;

//template<typename T> struct __attribute__((aligned(4))) S {};

S<int> *s;

