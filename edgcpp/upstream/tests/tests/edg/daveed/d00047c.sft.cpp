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

template <class T> struct A {
        static int x;
};

template <class T>
void f ( T , int = A < class {} > :: x ) { } 

void g()
{
        f(42);
}

