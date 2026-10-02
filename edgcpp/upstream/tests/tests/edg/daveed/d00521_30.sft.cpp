//remark:Microsoft DLL attributes
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
//script:

#define EXPORT __declspec(dllexport)
#define IMPORT __declspec(dllimport)

template <class T>
struct B
{
      IMPORT void foo2();
};

template <class T> void B<T>::foo2() {;}

struct EXPORT D : B<int>
{
};


