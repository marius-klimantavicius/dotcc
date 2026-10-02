//type:fp
//options_all:--c++17 -tused -A
//
struct A {};

struct X
{
     template <typename Q>
     int memfunc();
};

template <int (X::* P) ()>
int foo(...);

template<class T>
struct B
{
     static int bar()
     {
         A a;
         return foo<&X::memfunc<T> >(a);
     }
};

template <int (X::* P) ()>
int foo(A a)
{ return 0; }

int main()
{
     return B<int>::bar();
}

//cwg: 2109
//title: Value dependence underspecified
//meeting: Jacksonville 2/16
//edg_status: Passes
