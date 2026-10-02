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

inline void __declspec(dllexport) f() {}

extern "C" IMP char cc;
char cc;


IMP char cp;
extern char cp;

extern IMP char cp2;
char cp2;

