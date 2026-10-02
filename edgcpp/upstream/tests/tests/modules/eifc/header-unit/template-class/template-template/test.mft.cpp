//type:rp
//header_unit_files:test.h
//cases:3
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
  add_ten_via_proxy<inner_wrapper, wrapper> value(wrapped_value);
  return value.wrapper.value.value;
}
