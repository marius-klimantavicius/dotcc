//type: fp
//options: 
# 0 "./lto/20081211-1_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20081211-1_0.C"
# 1 "./lto/20081211-1.h" 1
class foo {
 public:
  foo () {}
  virtual ~foo () {}
  virtual void key_method (void);
};
# 2 "./lto/20081211-1_0.C" 2

foo *
create_foo (void)
{
  return new foo;
}

void
destroy_foo (foo *p)
{
  delete p;
}

int
main ()
{
  return 0;
}
