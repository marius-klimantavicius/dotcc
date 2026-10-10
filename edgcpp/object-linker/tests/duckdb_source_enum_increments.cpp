template<class T> struct Counter {
  enum E { first = sizeof(T), second, third };
  static E get(T);
};
template<class T> struct Retain {
  decltype(Counter<T>::get(T())) value = {};
};
template<class T> concept ConcreteSelected = requires { Counter<int>::get(1); };
template<ConcreteSelected T> struct Keep { T value = {}; };
static_assert(Counter<int>::second == sizeof(int) + 1);
static_assert(Counter<int>::third == sizeof(int) + 2);
int main() { return Counter<int>::third == sizeof(int) + 2 ? 0 : 1; }
