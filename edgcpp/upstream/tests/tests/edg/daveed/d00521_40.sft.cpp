//remark:Microsoft DLL attributes and extern "C"
//type:fp
//name:
//options:
//options_all:--microsoft --no_microsoft_bugs
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

namespace N1 { extern "C" __declspec(dllimport) int x; };
namespace N2 { extern "C" __declspec(dllimport) int x; };
