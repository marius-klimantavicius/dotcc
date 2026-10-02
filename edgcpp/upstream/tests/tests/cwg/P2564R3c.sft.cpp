//type:fn
//options_all:--c++20 -tused -A
consteval int id(int i) { return i; }
constexpr char id(char c) { return c; }


auto a = &f<char>; // ok, f<char> is not an immediate function

int x = 0;

template <typename T>
constexpr T h(T t = id(x)) { // h<int> is not an immediate function
    return t;
}

template <typename T>
constexpr T hh() {           // hh<int> is an immediate function
    return h<T>();
}

int i = hh<int>(); // ill-formed: hh<int>() is an immediate-escalating expression
                   // outside of an immediate-escalating function

