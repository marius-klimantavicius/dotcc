//type:rp
//options_all:--c++17 -tused -A
  template <class... T> struct V {}; 
  template <class... Ts, class... Us> int Foo(V<Ts...>, V<Us&...>) {return 1;} 
  template <class... Us> int Foo(V<>, V<Us&...>) {return 0;}                  
  int h() { 
    return Foo(V<>(), V<>()); 
  }
int main()
{
    return h();
}

//cwg: 2235
//title: Partial ordering and non-dependent types
//meeting: Jacksonville 2/18
//edg_status: Passes
