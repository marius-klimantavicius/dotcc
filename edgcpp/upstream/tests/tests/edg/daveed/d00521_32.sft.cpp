//remark:Microsoft DLL attributes
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
//script:

#define EXPORT __declspec(dllexport)
#define IMPORT __declspec(dllimport)

struct B
{
      IMPORT void foo2();
};

void B::foo2() {;}



