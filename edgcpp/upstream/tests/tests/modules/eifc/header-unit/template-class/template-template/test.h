#if TEST_NUMBER == 1
#define CLASS_KEY struct
#endif
#if TEST_NUMBER == 2
#define CLASS_KEY class
#endif
#if TEST_NUMBER == 3
#define CLASS_KEY union
#endif

template<template<typename> typename K, template<template<typename> typename> typename N>
CLASS_KEY add_ten_via_proxy {
public:
  add_ten_via_proxy(N<K> wrapper)
    : wrapper(wrapper.value.value + 10)
  {}
  N<K> wrapper;
};
