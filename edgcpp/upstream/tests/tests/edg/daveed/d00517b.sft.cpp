//remark:Positional format specifier
//type:fp
//name:
//options:
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

void f(int) {}

template <class T> struct A {
        void g()
        {
                f(1);
                f((T)1);
                f(T(1));
                f((int)(T)(1));
                f(int(T(1)));
        }
};

void g()
{
        A<float> a;
        a.g();
}
