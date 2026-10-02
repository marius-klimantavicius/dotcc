//type:fn
//options_all: --c++11

// This test is similar to interpreter-large-struct-size.sft.cpp but
// exercises the alternative scenario where folding fails due to an
// error node. In this context, the struct size cache should be
// populated with a sentinel value to indicate folding failed due to
// an input error. There should be a graceful error.


struct Foo {
  decltype(id) x;
};

int sink(Foo) { return 0; };

int main() {
  Foo x{};
  int y = sink(x);
  int z = sink(x);
}
