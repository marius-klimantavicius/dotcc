//type:fn
//options_all:--c++

struct Foo1 {
  Foo1();
  Foo1(const Foo1&);
};

struct Foo2 { };

struct Foo3 {
  Foo3(); 
  Foo3(Foo3&);
};

struct Bar {
  operator const Foo1&() const;
  operator const Foo2&() const;
  operator const Foo3&() const;
};

void f() {
  (void)(true ? Bar() : Foo1()); // ok
  (void)(true ? Bar() : Foo2()); // ok
  (void)(true ? Bar() : Foo3()); // error expected
}
