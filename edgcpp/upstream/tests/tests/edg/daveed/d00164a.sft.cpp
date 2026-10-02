//remark:Checking of exception specifications of templates
//type:fn
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

template<typename T>
struct S {
        void f1();
        void f2() throw (typename T::Y);
};

template<typename T>
void S<T>::f1() throw (T) {
}

template<typename T>
void S<T>::f2() throw (typename T::X) {
}

template<typename T> void g();
template<typename T> void g() throw(typename T::X) {}

