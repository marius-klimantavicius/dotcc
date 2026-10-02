//remark:Member lookup during member substitution
//options:--c++17;fp

template<bool b> struct B { static constexpr bool value = b; };

struct S {
  template<typename T> static bool test();
  template<typename T> static auto test(int) -> B<test<T>()>{}

  template<typename> static B<false> test(...);
};

 
template< typename T > struct X : decltype( S::test<T>(0) ) {};
 
int main() {
  return X<int&>::value;
}
