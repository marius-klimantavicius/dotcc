//type:fp
//options_all:--c++17 -tused -A
template <typename T>
class A
{};
template <typename T>
class B
{
typedef A<T> A; // ill-formed, no diagnostic required
A a;
};
int main()
{
B<int> b;
}

//cwg: 1875
//title: Reordering declarations in class scope
//meeting: Lenexa 5/15
//edg_status: Passes
