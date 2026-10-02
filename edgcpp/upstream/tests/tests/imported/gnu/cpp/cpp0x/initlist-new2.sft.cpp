//type: fp
//options: --c++11
// PR c++/57082
// { dg-do compile { target c++11 } }

struct X
{
private:
  ~X() {}
};

int main()
{
  new X;    // OK
  new X();  // OK
  new X{};  // { dg-bogus "" }
}
