#if TEST_NUMBER == 1
#define CLASS_KEY struct
#endif
#if TEST_NUMBER == 2
#define CLASS_KEY class
#endif
#if TEST_NUMBER == 3
#define CLASS_KEY union
#endif

template<template<typename> typename N>
CLASS_KEY add_ten_via_proxy {
public:
  add_ten_via_proxy(N<int> wrapper)
    : wrapper(wrapper.value + 10)
  {}
  N<int> wrapper;
};
