//type:cp
//options::-DNEG;fn
//options_all:-tused --c++11

template<typename T>
struct A { };

template<typename... T>
struct B { };

template<typename... T>
auto f(T...) -> A<T...>
{
  return A<T...>();
}

template<typename ...T> 
auto f(T...) -> B<T...>
{
  return B<T...>();
}

int main()
{
#if NEG
  f<int>(1);
#endif /* NEG */
  f<int, char>(1, 'c');
  f();
}
