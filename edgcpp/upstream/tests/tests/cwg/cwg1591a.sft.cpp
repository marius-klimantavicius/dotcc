//type:fp
//options_all:--c++14 -tused -A
template<class T, int N> void h(T const(&)[N]);

int main()
{
    h({1,2,3}); // T deduced as int; N deduced as 3
}

//cwg: 1591
//title: Deducing array bound and element type from initializer list
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
