//type: fp
//options: --c++14
// { dg-do compile { target c++14 } }

bool b;
auto f()
{
  if (b)
    return 42;
  else
    return f();
}
