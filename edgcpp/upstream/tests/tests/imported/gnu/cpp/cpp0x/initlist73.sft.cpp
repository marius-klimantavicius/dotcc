//type: fp
//options: --c++11
// PR c++/57682
// { dg-do compile { target c++11 } }

struct Class
{
  Class (int func)
  try
  : f { func }  { }
  catch ( ... ) { }

private:
  int f;
};
