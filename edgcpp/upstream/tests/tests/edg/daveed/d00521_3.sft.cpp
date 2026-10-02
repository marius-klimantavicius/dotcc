//remark:Microsoft dllimport/dllexport compatibility
//type:fn
//name:
//options:-DM=EXP:-DM=IMP:-DM=EXP -DNODEF:-DM=IMP -DNODEF
//options_all:--microsoft --microsoft_version=1300
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

#define EXP __declspec(dllexport)
#define IMP __declspec(dllimport)
#define IXP __declspec(dllexport dllimport)

struct S {
        void f();
};

struct EXP S;  // EXP ignored

void M S::f()
#ifdef NODEF
;
#else
{}
#endif
