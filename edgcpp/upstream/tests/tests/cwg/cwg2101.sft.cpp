//type:fp
//options_all:--c++17 -tused -A
//
template <const int* I>
struct B {};

template <typename T>
struct A
{
     int foo();
     static const int i = 0;
};

template <typename T>
int A<T>::foo()
{
     B<&i> b;
     return bar(b); // (*)
}

int bar(...) { return 0; }

int main()
{
     A<int> a;
     return a.foo();
}

//cwg: 2101
//title: Incorrect description of type- and value-dependence
//meeting: Kona 10/15
//edg_status: Passes
