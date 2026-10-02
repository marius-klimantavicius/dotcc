//remark:Error recovery when check EH specs
//type:fn
//name:
//options:
//options_all:-x
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:


template <template <class _T = double> class T> struct A {
        typedef void (T<>::*pmf1)(int);
        typedef void (T<int>::*pmf2)(int);
};

template <class T = short> struct B {};

A x B > ab ; 
A<B>::pmf1 x;
A<B>::pmf2 y;


