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
      IMPORT static int s;
};

template <class T> int B<T>::s = 3;

struct EXPORT D : B<int>
{
};


