//remark:Misformed constructor declarator
//type:fn
//name:
//options:
//options_all:-A
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

template <class T> struct B {
        typename T::template T<T>
        f(typename T::template T<T> x = typename T::template T<T>(T()))
        {
        new typename T::template T<T>;
        }
};

struct A {
        template <class U> struct T {
                template <class V> T(V);
T ) () ; 
                int x;
                typedef short TT;
        };
        A();
        template <class U> A(U);
        int x;
        typedef short TT;
};

void g()
{
        B<A> x;
        x.f();
        x.f(37);
        x.f(A());
}

