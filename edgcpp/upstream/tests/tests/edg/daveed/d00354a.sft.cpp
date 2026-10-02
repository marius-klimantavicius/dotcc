//remark:Duplicate extern "C" definitions
//type:fn
//name:
//options:--g++;fn:-A;fn:--microsoft;fp:;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

extern "C" void f() {}
extern "C" int x;

namespace N {
  extern "C" void f() {}
  extern "C" int x;
}
