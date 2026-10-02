//type:rp
//options::--gnu_version 80200
//options_all:--c++17

extern "C" int printf(const char *, ...);

struct Foo { Foo() { printf("Calling Foo::Foo()\n"); } };

struct Bar : Foo {
  using Foo::Foo;
  Bar(void*);
};

int main() {
  Bar f;
}
