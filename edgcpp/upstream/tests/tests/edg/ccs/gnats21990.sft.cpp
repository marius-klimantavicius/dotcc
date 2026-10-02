//type:cp
//options:--c++17
//options_all:-tused

template <typename T> void foo(T x){}

template <typename T>
struct S2 {
   using type = T;
};

void bar() {
   foo<typename ::S2<decltype(long{1234})>::type>(long{1234});
}
