//type:rp
//options::--g++:--clang:--microsoft
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A
{
  A() = default;
  A(short s) { printf("Calling A::A\n"); }
};

struct B : A
{
  B(short s, double d = 1.0) { printf("Calling B::B\n"); }
  using A::A;
};

B b(5); // Not ambiguous
B b2(5, 10); // Not ambiguous

int main() {}
