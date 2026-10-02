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

template < template < class _T > class T , class U > void f ( T < U > , void * = new class C )
{ } 

template <class T> struct B {};

void g()
{
        B<int> bi;
        f(bi);
}

