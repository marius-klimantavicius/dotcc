//type:fn
//options_all:--microsoft_v 1912 --ms_c++17
struct A {
              virtual void f() noexcept;
};
 
struct B : A {
              virtual void f() { }
};
