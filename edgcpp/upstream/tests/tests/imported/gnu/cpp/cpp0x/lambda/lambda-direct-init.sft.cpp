//type: fp
//options: --c++11
// Test that capture by copy uses direct-initialization.
// { dg-do compile { target c++11 } }

struct A
{
  A();
  explicit A(const A&);
};

int main()
{
  A a;
  [a]{};
}
