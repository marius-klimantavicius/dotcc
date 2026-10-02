//remark:GNU attributes on explicit template specializations
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

template<typename T> void f();

template<> __attribute__((noreturn)) void f<void>() __attribute__((const));

