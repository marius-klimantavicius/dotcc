//type:fp
//options_all:--gn 70100
//remark:[6.2] Spurious error with braced-initialization of a base class with a protected dtor
// 11/12/20 [EDGcpfe/21841]
//
// Spurious error with braced-initialization of a base class with a protected dtor
//
// When a class constructor has an explicit braced-initializer of its base class,
// and that base class has a protected-access destructor, the front end would
// issue a spurious accessibility error.
//
// This is now fixed.
class Base {
protected:
  ~Base();
};
struct Derived : public Base {
  Derived() : Base{} {} // Spurious "Base::~Base is inaccessible" error
};
