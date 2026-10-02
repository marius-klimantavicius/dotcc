//remark:Microsoft dllimport/dllexport compatibility
//type:fn
//name:
//options::-DSPECIAL
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
#define IXP __declspec(dllexport dllimport)

template<class T> struct
#ifndef SPECIAL
 EXP
#endif
 S {
EXP void f();  // Error: cannot specify a DLL interface if the class
	               // has such an interface.  (Also if interface comes
	               // from explicit specialization.)
};

template<class T> void S<T>::f() {}

#ifdef SPECIAL
template struct EXP S<int>;
#endif


