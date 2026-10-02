//remark:Microsoft dllimport/dllexport compatibility
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



#define EXP __declspec(dllexport)
#define IMP __declspec(dllimport)
#define IXP __declspec(dllimport dllexport )

template<class T> struct B {
        void f();
};

template<class T> void B<T>::f() {}

template struct B<int>;

struct IMP D1: B<int> {};

IMP void g();

EXP void gg() {
	g();
	D1 d1;
	d1.f();
}
