//type: fp
//options: 
struct A
{
  virtual void f() = 0;
};

typedef A (*fp)();
