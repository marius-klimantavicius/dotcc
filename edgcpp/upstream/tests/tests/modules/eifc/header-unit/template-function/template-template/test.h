template<template<typename> typename K, template<template<typename> typename> typename N>
int add_ten(N<K> wrapper) {
  return wrapper.value.value + 10;
}
