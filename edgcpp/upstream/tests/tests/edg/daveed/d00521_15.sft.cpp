//remark:Microsoft dllimport/dllexport compatibility
//type:fn
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

extern int g1;
int EXP g1;
