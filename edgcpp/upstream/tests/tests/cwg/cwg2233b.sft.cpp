//type:fp
//options_all:--c++17 -tused -A
template<class ... T> struct A_ {
        int a(int b = 0, T...) {
                return 0;
        }
};
A_<int> a_;

int main()
{
        // _CXXWP - implements p1114r0 - 2019
        // _CXXWP - implements core 2233 - 2019
        if (a_.a(0,0) == 0)
                return(0);
        return(1);
}
