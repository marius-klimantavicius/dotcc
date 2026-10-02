//remark:Microsoft dllimport/dllexport compatibility
//type:fn
//name:
//options::-DPOS;fp
//options_all:--microsoft -tused
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
	static T i;
};
template<class T> T B<T>::i = 3;

#ifdef POS
template struct IMP B<void*>;
#else
template struct EXP B<void*>;
#endif

