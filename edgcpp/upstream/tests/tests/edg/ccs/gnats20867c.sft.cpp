//type:cp
//options_all:--c++14

template < typename... T > void sink(T ...){}

template < typename... T >
auto f(T... t){
  [=](auto ... j){
    sink((t + j)...);
  }(t...);
}

int main(){
  f(0);
  f();
  f(0, 0);
}
