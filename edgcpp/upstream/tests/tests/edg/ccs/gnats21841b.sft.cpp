//type:cp
//options::--gnu_version 70000
//options_all:--c++14 -tused

template<typename T>
class Base {
protected:
  ~Base();
};

struct Derived : public Base<Derived> {
  Derived() : Base<Derived>{} {}
};
