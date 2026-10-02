//options_all:--c++23 -A
  typedef int *AP[3];        // array of 3 pointer to int
  typedef const int *const ACPC[3]; // array of 3 const pointer to const int
  ACPC &&r = AP{};          // binds directly
