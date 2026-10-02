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

#define IMPORT __declspec(dllimport)
#define EXPORT __declspec(dllexport)

struct EXPORT C2 {
   virtual void foo();
};

void IMPORT C2::foo() {;}

