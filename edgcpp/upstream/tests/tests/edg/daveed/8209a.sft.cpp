//remark:Microsoft VC8 bug fixes (friend class template)
//type:fn
//name:
//options:--microsoft_version=1400:--microsoft_version=1310;fp
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

template<class T>
class A;

template<class T>
struct A2 {
    friend class A;   // Error in msvc8, ok in msvc 7.1
};

// OK
template<class T>
struct B {
    template<class T>
    friend class A;
};
