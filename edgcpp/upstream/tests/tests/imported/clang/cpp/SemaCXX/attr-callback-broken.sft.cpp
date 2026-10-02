//type: fn
//options:  --c++23
# 1 "SemaCXX/attr-callback-broken.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 499 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/attr-callback-broken.cpp" 2


class C_in_class {

# 1 "SemaCXX/../Sema/attr-callback-broken.c" 1


__attribute__((callback())) void no_callee(void (*callback)(void));

__attribute__((callback(1, 1))) void too_many_args_1(void (*callback)(void)) {}
__attribute__((callback(1, -1))) void too_many_args_2(double (*callback)(void));
__attribute__((callback(1, 2, 2))) void too_many_args_3(void (*callback)(int), int);

__attribute__((callback(1, 2))) void too_few_args_1(void (*callback)(int, int), int);
__attribute__((callback(1))) void too_few_args_2(int (*callback)(int));
__attribute__((callback(1, -1))) void too_few_args_3(void (*callback)(int, int)) {}

__attribute__((callback(-1))) void oob_args_1(void (*callback)(void));
__attribute__((callback(2))) void oob_args_2(int *(*callback)(void)) {}
__attribute__((callback(1, 3))) void oob_args_3(short (*callback)(int), int);
__attribute__((callback(-2, 2))) void oob_args_4(void *(*callback)(int), int);
__attribute__((callback(1, -2))) void oob_args_5(void *(*callback)(int), int);
__attribute__((callback(1, 2))) void oob_args_6(void *(*callback)(int), ...);

__attribute__((callback(1))) __attribute__((callback(1))) void multiple_cb_1(void (*callback)(void));
__attribute__((callback(1))) __attribute__((callback(2))) void multiple_cb_2(void (*callback1)(void), void (*callback2)(void));


__attribute__((callback(0))) void oob_args_0(void (*callback)(void));
# 33 "SemaCXX/../Sema/attr-callback-broken.c"
__attribute__((callback(1, -1))) void vararg_cb_1(void (*callback)(int, ...)) {}
__attribute__((callback(1, 1))) void vararg_cb_2(void (*callback)(int, ...), int a);

__attribute__((callback(1, -1, 1, 2, 3, 4, -1))) void varargs_1(void (*callback)(int, ...), int a, float b, double c) {}
__attribute__((callback(1, -1, 4, 2, 3, 4, -1))) void varargs_2(void (*callback)(void *, double, int, ...), int a, float b, double c);

__attribute__((callback(1, -1, 1))) void self_arg_1(void (*callback)(int, ...)) {}
__attribute__((callback(1, -1, 1, -1, -1, 1))) void self_arg_2(void (*callback)(int, ...));

__attribute__((callback(cb))) void unknown_name1(void (*callback)(void)) {}
__attribute__((callback(cb, ab))) void unknown_name2(void (*cb)(int), int a) {}

__attribute__((callback(callback, 1))) void too_many_args_1b(void (*callback)(void)) {}
__attribute__((callback(callback, __))) void too_many_args_2b(double (*callback)(void));
__attribute__((callback(callback, 2, 2))) void too_many_args_3b(void (*callback)(int), int);

__attribute__((callback(callback, a))) void too_few_args_1b(void (*callback)(int, int), int a);
__attribute__((callback(callback))) void too_few_args_2b(int (*callback)(int));
__attribute__((callback(callback, __))) void too_few_args_3b(void (*callback)(int, int)) {}

__attribute__((callback(__))) void oob_args_1b(void (*callback)(void));

__attribute__((callback(callback))) __attribute__((callback(callback))) void multiple_cb_1b(void (*callback)(void));
__attribute__((callback(1))) __attribute__((callback(callback2))) void multiple_cb_2b(void (*callback1)(void), void (*callback2)(void));


__attribute__((callback(this))) void oob_args_0b(void (*callback)(void));
# 68 "SemaCXX/../Sema/attr-callback-broken.c"
__attribute__((callback(callback, __))) void vararg_cb_1b(void (*callback)(int, ...)) {}
__attribute__((callback(1, a))) void vararg_cb_2b(void (*callback)(int, ...), int a);

__attribute__((callback(callback, __, callback, a, b, c, __))) void varargs_1b(void (*callback)(int, ...), int a, float b, double c) {}
__attribute__((callback(1, __, c, a, b, c, -1))) void varargs_2b(void (*callback)(void *, double, int, ...), int a, float b, double c);

__attribute__((callback(1, __, callback))) void self_arg_1b(void (*callback)(int, ...)) {}
__attribute__((callback(callback, __, callback, __, __, callback))) void self_arg_2b(void (*callback)(int, ...));
# 6 "SemaCXX/attr-callback-broken.cpp" 2

};

class ExplicitParameterObject {
  __attribute__((callback(2, 0))) void explicit_this_idx(this ExplicitParameterObject* self, void (*callback)(ExplicitParameterObject*));
  __attribute__((callback(2, this))) void explicit_this_identifier(this ExplicitParameterObject* self, void (*callback)(ExplicitParameterObject*));
};
