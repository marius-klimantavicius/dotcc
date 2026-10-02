//type: fp
//options: 
# 0 "./torture/pr58201_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr58201_0.C"
# 1 "./torture/pr58201.h" 1
class A
{
 protected:
  A();
  virtual ~A();
};

class B : virtual public A
{
 public:
  B();
  virtual ~B();
};

class C
{
 private:
  class C2 : public B
   {
   public:
     C2();
     virtual ~C2();
   };
};
# 2 "./torture/pr58201_0.C" 2

C::C2::C2(){ }
C::C2::~C2() { }

int main ()
{
    return 0;
}
