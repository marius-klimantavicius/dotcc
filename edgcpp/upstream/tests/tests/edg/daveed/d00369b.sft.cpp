//remark:Using-declarations and inheritance
//type:fp
//name:
//options:
//options_all:-A
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct B { typedef int I; };
struct D: B {
  I x;
  typedef int I;
};
