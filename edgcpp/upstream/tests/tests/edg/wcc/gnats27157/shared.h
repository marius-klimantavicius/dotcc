using a_size_type = unsigned long long;

template<typename T>
struct foo {
  a_size_type x = 0;
};

inline void bar(foo<int> x) {}
