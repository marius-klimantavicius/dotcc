//type: fp
//options: --c++11
// PR c++/79566
// { dg-do compile { target c++11 } }

struct X {
  struct Y { };

  Y* begin();
  Y* end();
};

void f()
{
  X x;
  for (struct X::Y& y : x)
    ;
}
