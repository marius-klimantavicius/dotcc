template<template<int> typename N>
int add_ten(N<-10> wrapper) {
  return wrapper.value + 10;
}
