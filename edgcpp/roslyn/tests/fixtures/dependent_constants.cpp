template<class T> constexpr unsigned long dependent_expression_size(T& value) {
  return sizeof(value);
}

template<class T> struct PackFirst { T value; };
template<class T> struct PackSecond { T value; };

template<template<class> class... Templates>
constexpr unsigned template_pack_count() {
  return sizeof...(Templates);
}

enum OperatorEnum : unsigned short { operator_value = 3 };
using WrittenTypeOperand = __typeof__(const int);
using WrittenEnumOperand = __underlying_type(OperatorEnum);
WrittenTypeOperand written_type_value = 7;
WrittenEnumOperand written_enum_value = operator_value;

unsigned long dependent_constant_use() {
  int value = 2;
  return dependent_expression_size(value)
    + template_pack_count<PackFirst, PackSecond>()
    + written_type_value + written_enum_value;
}
