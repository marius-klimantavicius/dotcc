//remark:Use of template parameter typedef
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

template <class T> struct A : T {
   typedef typename T::X my_x;
   friend void my_x::f(int);
};
