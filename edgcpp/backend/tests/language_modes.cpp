// Header-free modern C++ frontend/managed backend boundary.
template<class T> concept Addable = requires(T value) { value + value; };
template<Addable T> constexpr T twice(T value) { return value + value; }
consteval int frozen(int value) { return twice(value); }
struct Counter {
  int value;
  constexpr int operator()() const { return value; }
};
template<Counter C> constexpr int selected = C();
static_assert(frozen(21)==42);
static_assert(selected<Counter{9}> == 9);
#if __cplusplus >= 202302L
constexpr int context(int value) {
  if consteval { return value*2; } else { return value+1; }
}
static_assert(context(3)==6);
#else
constexpr int context(int value) { return value+1; }
#endif
int main() {
  auto sum=[]<Addable T>(T left,T right){return left+right;};
  int value=7;
  return twice(value)!=14 || sum(2,3)!=5 || selected<Counter{9}> !=9 || context(value)!=8;
}
