//type:fp
//options_all:--c++17 -tused -A


class C_;
template<class T> struct A_ {
        struct B { };
        int f(const C_&);
        struct D {
                int g() { return 8; }
        };
};
template<> struct A_<int> {
        struct B { };
        int f(const C_&);
        struct D {
                int g() { return 18; }
        };
};
class C_ {
        template<class T> friend struct A_<T>::B;
        template<class T> friend int A_<T>::f(const C_&);
        int member;
public:
        C_(int i) : member(i) { }
};

int A_<int>::f(const C_& c) { return 10+c.member; }
int main()
{
      {
        // _CXX11 - Implements core 1804 - in C++14 status 2015
        C_ c (7);
        A_<char> ac;
        A_<int>  ai;
        if (ac.f(c) == 7)
            return(0);
        if (ai.f(c ) == 17)
            return(0);
        return(1);
}

//cwg: 1804
//title: Partial specialization and friendship
//meeting: Urbana-Champaign 11/14
//edg_status: EDGcpfe/19869
