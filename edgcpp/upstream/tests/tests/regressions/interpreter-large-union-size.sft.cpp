//type:fp
//options_all: --c++11

// This is the same test as in interpreter-large-struct-size.sft.cpp
// except it exercises the union code path

union arr {
  // The struct size limit in EDG is currently 2^30.
  unsigned char x[(unsigned long long)1 << 31];
};

int sink(arr) { return 0; };

int main() {
  arr x{};
  int y = sink(x);
  int z = sink(x);
}
