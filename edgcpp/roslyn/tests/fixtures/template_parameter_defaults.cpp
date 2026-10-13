template<class T> int selected_value = 11;
template<class T> int selected_value<T*> = 29;
int* selected_address() { return &selected_value<int*>; }
int* primary_address() { return &selected_value<int>; }

template<class T> struct DefaultBox { T value; };
template<template<class> class Container = DefaultBox>
struct UsesDefault { Container<int> item; };
UsesDefault<> default_instance;

void parameter_attributes([[maybe_unused]] int value);

struct DefaultLambdaHost {
  void invoke(int value = [] { return 17; }());
};
void use_default_lambda(DefaultLambdaHost& host) { host.invoke(); }

template<class T> struct AttributedInstantiation { T value; };
template struct __attribute__((visibility("default"))) AttributedInstantiation<int>;
