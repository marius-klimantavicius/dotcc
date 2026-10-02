//type:rp
//options_all: -A --c++20 -tused -e 200 --no_wrap
// fails when not c++20
  template <typename... T> struct X;
  template <> struct X<> {};
  template <typename T, typename... Ts> struct X<T, Ts...> : X<Ts...> {};
  struct D : X<int> {};
  template <typename... T>
  int f(const X<T...>&) {
    return sizeof...(T);
  }
  int main() {
    return f(D())-1;  // calls f<int>, not f<>
                   // B is X<>, C is X<int>
  }

//cwg: 2303
//title: Partial ordering and recursive variadic inheritance
//meeting: Kona 02/19
//edg_status: EDGcpfe/25421
//fixed_in: 6.4
