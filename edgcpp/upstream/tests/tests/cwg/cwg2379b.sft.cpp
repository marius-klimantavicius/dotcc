//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
template<class T> void f(int);
template<> void f<int>(int);
 
class X {
    friend void f<int>(consteval int x=1); 
};
