//type: fp
//options: 
# 0 "./lto/20081022_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20081022_0.C"
# 1 "./lto/20081022.h" 1
class foo
{
public:
  int bar ()
  {
    return 0;
  }
};
# 2 "./lto/20081022_0.C" 2

int
f (foo * a)
{
  return a->bar ();
}

int
main()
{
  return 0;
}
