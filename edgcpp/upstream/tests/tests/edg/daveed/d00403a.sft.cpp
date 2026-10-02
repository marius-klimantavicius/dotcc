//remark:Packed types and back ends
//type:fp
//name:
//options:--gcc;fp:-DNEGPP --gcc;fp:--g++;fp:-DNEGPP --g++ --gnu=40800;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct S {
	int z;
	int a[];
} s = { 1, {
#ifdef NEGPP
7
#endif
} };

