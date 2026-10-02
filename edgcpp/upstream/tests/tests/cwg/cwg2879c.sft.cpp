//options_all:--c++20 -A
  typedef int *A[3];                // array of 3 pointer to int
  typedef const int *const CA[3];   // array of 3 const pointer to const int

  auto &&r2 = const_cast<A&&>(CA{}); // OK, temporary materialization conversion is performed
