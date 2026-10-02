//remark:Microsoft dllimport/dllexport compatibility
//type:fn
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


#define EXP __declspec(dllexport)
#define IMP __declspec(dllimport)


template<class T> struct B {
	void b() { typename T::N *p; }
};

template<class T> struct M: B<T> {
	void m() {}
};


struct EXP D: M<int> {};
