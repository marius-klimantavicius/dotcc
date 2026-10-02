//type:fp
//options_all:--c++11
//remark:[4.9] Override of a virtual destructor in a class with a template base class
// 3/20/14  [EDGcpfe/14943]
//
// Override of a virtual destructor in a class with a template base class
//
// A spurious error had been issued when the virtual destructor declared in a
// class with a template base class specified the "override" function modifier.
// Now fixed.
template <class T> struct A : T {
  virtual ~A() override;
};
