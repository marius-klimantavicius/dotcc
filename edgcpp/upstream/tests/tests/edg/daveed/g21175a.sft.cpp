//remark:Nontype template arguments and local static variables
//options:-w --gnu=80000 --c++17;fp

template <int *>
struct C {};
 
void f()
{
    static int array[] = {};
    C<array> c;
}
