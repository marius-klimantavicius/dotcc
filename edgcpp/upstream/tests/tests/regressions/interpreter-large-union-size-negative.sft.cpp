//type:fn
//options_all: --c++11

// This test is the same test as interpreter-large-struct-size-negative.sft.cpp
// but it exercises the union code path.

union Foo {
  decltype(id) x;
};

int sink(Foo) { return 0; };

int main() {
  Foo x{};
  int y = sink(x);
  int z = sink(x);
}
