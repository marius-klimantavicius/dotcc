//remark:Microsoft dllimport/dllexport compatibility
//type:fp
//name:
//options::-DNEG;fn
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

#ifdef NEG
static EXP void f1() {};
static IMP void f2();

static EXP int v1;
static IMP int v2;
#endif

namespace {
	EXP void f3() {}
	IMP void f4();

	EXP int v3;
	IMP extern int v4;
}
