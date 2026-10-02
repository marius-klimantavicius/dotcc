//remark:Prototype instantiation work
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
//script:

  template<int *> struct B {};
  template<typename T> struct S {
    static int si;
    struct D: B<&si> {};
  };
