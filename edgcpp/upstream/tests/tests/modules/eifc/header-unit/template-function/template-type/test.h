template<template<typename> typename N>
int add_ten(N<int> wrapper) {
  return wrapper.value + 10;
}
