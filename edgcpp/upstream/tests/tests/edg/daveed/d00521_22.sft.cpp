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

template <class T>
class A
{
    public:
        void l(int a) {};
};

class __declspec( dllexport ) B: public  A<char*>
{
};
