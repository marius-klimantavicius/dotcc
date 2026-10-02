//remark:In Sun mode, extern "C" name linkage is also imbued on static or inline functions
//type:fp
//name:
//options:
//options_all:--sun
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

extern "C" {
namespace X {
    inline int f(int) { return 0; }
}
}
extern "C" {
    using X::f;
}
extern "C" int f(int);

