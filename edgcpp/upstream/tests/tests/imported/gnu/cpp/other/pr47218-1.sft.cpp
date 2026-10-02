//type: fp
//options: 
# 0 "./other/pr47218-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./other/pr47218-1.C"
# 1 "./other/pr47218.h" 1

class FooBaseBase0
{
public:
  virtual ~FooBaseBase0 () {}
};

class FooBaseBase1
{
public:
  virtual void Bar() {}
};


class FooBase: public FooBaseBase0, public FooBaseBase1
{
public:
  virtual void Bar() {}
};

class Foo2: public FooBase
{
public:
  ~Foo2 ();
  virtual void Bar();
};

class Foo3: public FooBase
{
public:
  ~Foo3 ();
  virtual void Bar();
};
# 2 "./other/pr47218-1.C" 2

Foo2::~Foo2 ()
{
  ((FooBaseBase1*)this)->Bar();
}

void Foo2::Bar()
{
}
