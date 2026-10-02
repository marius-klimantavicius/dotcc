//remark:Microsoft dllimport/dllexport compatibility
//type:fp
//name:
//options:
//options_all:--microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:


#define EXP __declspec(dllexport)
#define IMP __declspec(dllimport)

struct S {
	static EXP int s;
};
EXP int S::s;

