//type:rp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

template<typename T>
struct inner_wrapper {
  inner_wrapper(T value)
    : value(value)
  {}
  T value;
};

template<template<typename> typename N>
struct wrapper {
  wrapper(N<int> value)
    : value(value)
  {}
  N<int> value;
};

int main() {
  inner_wrapper<int> inner_value(-10);
  wrapper<inner_wrapper> wrapped_value(inner_value);
  return add_ten(wrapped_value);
}
