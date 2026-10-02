//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
//

template<class T> void f(int);
template<> void f<int>(int);
 
class X {
    friend void f<int>(inline x=42); 
};

//cwg: 2379
//title: Missing prohibition against constexpr in friend declaration
//meeting: Kona 02/19
//edg_status: Passes
