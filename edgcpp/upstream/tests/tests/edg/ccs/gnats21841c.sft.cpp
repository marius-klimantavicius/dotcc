//type:cp
//options::--gnu_version 70000
//options_all:--c++14 -tused

class Base {
protected:
  Base();
  ~Base();
};

struct Derived : public Base {
  Derived() : Base{} {}
};
