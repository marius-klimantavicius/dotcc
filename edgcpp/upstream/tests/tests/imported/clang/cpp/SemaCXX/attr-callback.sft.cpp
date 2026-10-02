//type: fp
//options:  --c++23
# 1 "SemaCXX/attr-callback.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 499 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/attr-callback.cpp" 2




class C_in_class {
# 1 "SemaCXX/../Sema/attr-callback.c" 1




__attribute__((callback(1))) void no_args(void (*callback)(void));
__attribute__((callback(1, 2, 3))) void args_1(void (*callback)(int, double), int a, double b);
__attribute__((callback(2, 3, 3))) void args_2(int a, void (*callback)(double, double), double b);
__attribute__((callback(2, -1, -1))) void args_3(int a, void (*callback)(double, double), double b);

__attribute__((callback(callback))) void no_argsb(void (*callback)(void));
__attribute__((callback(callback, a, 3))) void args_1b(void (*callback)(int, double), int a, double b);
__attribute__((callback(callback, b, b))) void args_2b(int a, void (*callback)(double, double), double b);
__attribute__((callback(2, __, __))) void args_3b(int a, void (*callback)(double, double), double b);
__attribute__((callback(callback, -1, __))) void args_3c(int a, void (*callback)(double, double), double b);
# 7 "SemaCXX/attr-callback.cpp" 2
};

class ExplicitParameterObject {
  __attribute__((callback(2, 1))) void explicit_this_idx(this ExplicitParameterObject* self, void (*callback)(ExplicitParameterObject*));
  __attribute__((callback(2, self))) void explicit_this_identifier(this ExplicitParameterObject* self, void (*callback)(ExplicitParameterObject*));
};

struct Base {

  void no_args_1(void (*callback)(void));
  __attribute__((callback(1))) void no_args_2(void (*callback)(void));
  __attribute__((callback(callback))) void no_args_3(void (*callback)(void)) {}

  __attribute__((callback(1, 0))) virtual void
  this_tr(void (*callback)(Base *));

  __attribute__((callback(1, this, __, this))) virtual void
  this_unknown_this(void (*callback)(Base *, Base *, Base *));

  __attribute__((callback(1))) virtual void
  virtual_1(void (*callback)(void));

  __attribute__((callback(callback))) virtual void
  virtual_2(void (*callback)(void));

  __attribute__((callback(1))) virtual void
  virtual_3(void (*callback)(void));
};

__attribute__((callback(1))) void
Base::no_args_1(void (*callback)(void)) {
}

void Base::no_args_2(void (*callback)(void)) {
}

struct Derived_1 : public Base {

  __attribute__((callback(1, 0))) virtual void
  this_tr(void (*callback)(Base *)) override;

  __attribute__((callback(1))) virtual void
  virtual_1(void (*callback)(void)) override {}

  virtual void
  virtual_3(void (*callback)(void)) override {}
};

struct Derived_2 : public Base {

  __attribute__((callback(callback))) virtual void
  virtual_1(void (*callback)(void)) override;

  virtual void
  virtual_2(void (*callback)(void)) override;

  virtual void
  virtual_3(void (*callback)(void)) override;
};

void Derived_2::virtual_1(void (*callback)(void)) {}

__attribute__((callback(1))) void
Derived_2::virtual_2(void (*callback)(void)) {}

void Derived_2::virtual_3(void (*callback)(void)) {}
