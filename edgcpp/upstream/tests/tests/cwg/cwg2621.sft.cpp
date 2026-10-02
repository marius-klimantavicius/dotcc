//type:fp
//options_all:--c++20 -tused -A
enum class E {
  a, b, c
};

using MyE = E;

int main() {
  using enum MyE;   // #1
}
