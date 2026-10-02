//type: fp
//options:  --c++11
// { dg-require-effective-target c++11 }

struct A
{
protected:
  A() = default;
  int i;
};

struct B: A {
  B() = default;
};

int main()
{
  B();
}
