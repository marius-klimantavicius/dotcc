//type:cp
//options:--gnu_version 70000
//options_all:--c++17 -tused

struct A
{
  constexpr A()  {};
  A& operator=(A&&) {return *this;};
};

template <class T>
struct B : A
{
  constexpr B() = default;

  //B& operator=(B&&) = default;                  // This would be accepted as non-deleted
  B& operator=(B&&) noexcept(false) = default;  // Apparently not compatible - deleted
};

void f()
{
  B<int> a, b;
  b = static_cast<B<int> &&>(a); //clang and g++ accept, EDG rejects
}
