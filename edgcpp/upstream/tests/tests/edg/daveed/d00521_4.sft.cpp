//remark:Microsoft dllimport/dllexport compatibility
//type:fp
//name:
//options::-DNEG;fn
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

template<class T> struct S {
        void f();
};

template struct EXP S<int>;
template struct S<int>; // Okay
#ifdef NEG
template struct IMP S<int>;  // Error: Cannot change DLL interface in 2nd specialization (however omitting the DLL interface is okay).
#endif

