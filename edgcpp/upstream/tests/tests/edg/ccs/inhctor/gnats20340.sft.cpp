//type:cp
//options::--gnu_version 80200
//options_all:--c++17

struct Foo { Foo() {} };

struct Bar : Foo {
  using Foo::Foo;
  Bar(void*);
};

int main() {
  Bar f;
}
