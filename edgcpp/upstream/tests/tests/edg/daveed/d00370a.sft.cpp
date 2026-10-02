//remark:Error recovery on invalid dependent using-declaration
//type:fn
//name:
//options:
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

  template<typename T> struct B { typedef int I; };
  template<typename T> struct D: B<T> {
    using typename D::I;  // Previously an internal error.
  };        // Now a normal error.

