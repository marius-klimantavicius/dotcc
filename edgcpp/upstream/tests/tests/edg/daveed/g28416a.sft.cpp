//remark:Lifetimes for constexpr local variables
//options:--c++20;fp

#include <initializer_list>
struct S {
  constexpr S(std::initializer_list<int> lst): lst(lst) {}
  std::initializer_list<int> lst;
};
int main() {
  constexpr static std::initializer_list<int> li = {4};
  constexpr S s{li};
}

