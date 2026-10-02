//remark:Microsoft dllimport/dllexport compatibility
//type:fp
//name:
//options:--microsoft
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

template <class T>
class Basic {
public:
        virtual ~Basic() {}
        void foo(int);
        void goo();
};

template <>
void Basic<int>::foo(int) {}

template class __declspec(dllimport) Basic<int>;
