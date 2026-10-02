//remark:Bizarre use of elaborated type name in func template default argument
//type:fn
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

template <class T = short> struct A {
        static int x;
};

template <class T> int A<T>::x = 37;

template <template <class _T = short> class T, class U>
void f ( T < U > , int = T < class > :: x ) { } 

void g()
{
        A<double> a;
        f(a);
}

