//remark:Microsoft dllimport/dllexport compatibility
//type:fp
//name:
//options:
//options_all:--microsoft -x
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:


#define EXP __declspec(dllexport)
#define IMP __declspec(dllimport)
#define IXP __declspec(dllimport dllexport )

template<class T> struct EXP S {
        void f();
	friend void g() {}
	struct N {
		void nf();
	};
};

template<class T> void S<T>::f() {}
template<class T> void S<T>::N::nf() {}

template struct S<int>;

#include <typeinfo>

char const* x() {
	return typeid(S<int>()).name();
	throw S<int>();
}
