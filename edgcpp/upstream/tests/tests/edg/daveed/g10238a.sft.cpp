//remark:GNU compatibility -- block-extern and namespace
//options:-A;ln:--g++ --gnu=40500;rp

namespace A {
class B {
              void f1();
              void f2();
      };
}
using namespace A;
void A::B::f1()
{
  void foo();
  foo();
}

void foo() {}

int main() {}
