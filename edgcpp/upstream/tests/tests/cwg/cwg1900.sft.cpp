//type:fp
//option_all:--c++20 -tused -A
namespace A {
  struct B {
    friend void foo(signed);
    //friend void foo(unsigned);
  };
}

void A::foo(signed) { }

//cwg: 1900
//title: Do friend declarations count as “previous declarations”?
//meeting: Virtual 11/20*
//edg_status: Passes
