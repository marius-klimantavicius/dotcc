//type:fp
//options_all:--g++ --c++20 --gnu_version=130999
//remark:Evaluation of captured constexpr variable in a constant-evaluated context
// 4/2/26   [EDGcpfe/28749]
//
// Evaluation of captured constexpr variable in a constant-evaluated context
//
// Previously, the constant-evaluation interpreter could not look through the
// capture of c to determine the value of c[1]: A spurious error about an attempt
// at accessing run-time storage ensued.  That is now fixed.
struct C {
  int val;
  constexpr int &operator[](int) const { return (int&)val; }
};
int main() {
  constexpr C c{1};
  [c]() {
    if constexpr (c[1]) {}
  }();
}
