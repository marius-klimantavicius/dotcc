//remark:Microsoft in-class specialization
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

template<typename T> struct S {
  template<typename X> void operator=(S<X> const&) {}
  template<> void operator=(S<T> const&) {}
};

template struct S<int>;

