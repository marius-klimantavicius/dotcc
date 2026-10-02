//remark:Microsoft dllimport/dllexport compatibility
//type:fn
//name:
//options::-DPOS;fp
//options_all:--microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:


#define EXP __declspec(dllexport)
#define IMP __declspec(dllimport)


template<class T> struct B {
	void b() { typename T::N *p; }
};

#ifdef POS
template<> struct B<int> { void b2() {} };
#endif


template<class T> struct M: B<T> {
	void m() {}
};

template struct M<int>;

template<class T> struct D: M<T> {
	void d() {}
};

template struct EXP D<int>;

