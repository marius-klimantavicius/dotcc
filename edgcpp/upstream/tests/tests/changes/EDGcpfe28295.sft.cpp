//type:fp
//options_all:--gn 120200 --c++20
//remark:Class template argument deduction from default template argument
// 5/8/26   [EDGcpfe/28295]
//
// Class template argument deduction from default template argument
//
// Previously, the front end mistakenly deduced a type char const* instead of
// CTStr<1> for the deduced template argument of S in S{}.  In turn, this
// triggered a spurious error on the call Name.func().  That is now fixed.
template<int N> struct CTStr {
  constexpr CTStr(char const (&)[N]) {}
  void func() const {}
};
template<CTStr Name = ""> struct S {
  void f() const {
    Name.func();
  }
};
int main(){
  S{}.f();
}
