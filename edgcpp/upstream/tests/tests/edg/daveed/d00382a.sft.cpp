//remark:Thread-locals and DLLs
//type:fn
//name:
//options:--c;fn:;fn
//options_all:--microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

extern _declspec(dllimport thread) int z;
