//type: fp
//options: 
# 0 "./lto/pr45621_1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/pr45621_1.C"
# 1 "./lto/pr45621.h" 1
struct S
{
  void m ();
  virtual void v1 ();
  virtual void v2 ();
};

extern S s;
# 2 "./lto/pr45621_1.C" 2

void
S::v1 ()
{
  v2 ();
}

void
S::m ()
{
  v1 ();
}
