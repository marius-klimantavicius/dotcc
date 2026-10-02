//type:fn
//options:--c++26

struct C
{
  char c;
  int i;
  long l;
};

template<typename T>
void f(T t, C c)
{
  auto [ ... b ] = b;           // error: used in initializer
  auto [ ... x, b1, b2, b3, b4 ] = c; // error: too many bindings
  auto [ ... y, ... z ] = c;          // error: only one pack allowed
}

void non_tmpl(C c)
{
  auto [ ... b ] = c;           // error: not inside template
}

auto [ ... b ] = C{ 'a', 2, 3 }; // error: not inside template
