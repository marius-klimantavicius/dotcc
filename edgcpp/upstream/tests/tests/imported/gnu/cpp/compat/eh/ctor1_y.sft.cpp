//type: fp
//options: 
# 0 "./compat/eh/ctor1_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/eh/ctor1_y.C"
extern bool was_f_in_Bar_destroyed;

# 1 "./compat/eh/ctor1.h" 1
struct Foo
{
  ~Foo ();
};

struct Bar
{
  ~Bar ()



  noexcept(false)

  ;
  Foo f;
};
# 4 "./compat/eh/ctor1_y.C" 2

Foo::~Foo()
{
  was_f_in_Bar_destroyed=true;
}

Bar::~Bar()



noexcept(false)

{
  throw 1;
}
