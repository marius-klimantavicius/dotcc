//remark:Microsoft mode: Allow multiple explicit instantiation directives
//type:fp
//name:
//options:--microsoft:;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

template<class> struct S {
	void f();
};
template<class T> void S<T>::f() {}
template struct S<int>;
template struct S<int>;

template<class T> void g() {}
template void g<int>();
template void g<int>();

