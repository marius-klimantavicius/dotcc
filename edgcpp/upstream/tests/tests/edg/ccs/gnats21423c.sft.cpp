//type:cp
//options_all:--c++17

struct S1 {
    constexpr S1() {}
    constexpr operator int() const { return 0b1; }
};
constexpr S1 s1;

//test 4
void f4 (){
  int a[s1];
}

//test 5
void f5 (){
  int a[s1];
}
