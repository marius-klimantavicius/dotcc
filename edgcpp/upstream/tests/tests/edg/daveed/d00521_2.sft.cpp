//remark:Microsoft dllimport/dllexport compatibility
//type:fp
//name:
//options:-DC= -DM=EXP;fn:-DC=EXP -DM= ;fp:-DC=IXP -DM=IXP;fp:-DC=EXP -DM=IMP;fp:-DC=IMP -DM=EXP;fp
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
#define IXP __declspec(dllexport dllimport)

struct C S {
        void f();
};

void M S::f()
#ifdef NODEF
;
#else
{}
#endif
