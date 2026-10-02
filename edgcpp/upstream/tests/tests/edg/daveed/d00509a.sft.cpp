//remark:GNU C++ explicit instantiations with storage class specifiers
//type:fp
//name:
//options:--g++:;fn:-DNEG --g++;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

// begin
template <class C> static void Foo(const C& value) {
      return;
}

template static void Foo(const int& value);
template extern void Foo(const char& value);


template<typename> struct X {
	void f() {}
	static int i;
};

template<typename T> int X<T>::i = 3;

template extern void X<int>::f();
template extern int X<int>::i;
template int X<short>::i;

#ifdef NEG
template typedef void Foo(const short& value);
template static struct X<void>;
template extern void X<float>::f();
template extern int X<double>::i;
#endif

// end
