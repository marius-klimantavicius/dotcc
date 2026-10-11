#include "program.h"
Base::Base(int *output) : trace(output) { *trace = *trace * 10 + selected_value(this); }
Base::~Base() { *trace = *trace * 10 + selected_value(this); }
int Base::value() { return 1; }
Derived::Derived(int *output) : Base(output), answer(2) { *trace = *trace * 10 + selected_value(this); }
Derived::~Derived() { *trace = *trace * 10 + selected_value(this); }
int Derived::value() { return answer; }
int selected_value(Base *object) { return object->value(); }
int Derived::additional() { return answer + 5; }
int selected_additional(Base *object) { return object->additional(); }
Left::Left(int input) : left(input) {}
int Left::read_left() { return left; }
Right::Right(int input) : right(input) {}
int Right::read_right() { return right; }
Both::Both() : Left(10), Right(20), result(30) {}
int Both::read_left() { return result + left; }
int Both::read_right() { return result + right; }
int selected_right(Right *object) { return object->read_right(); }
int Base::plus(int operand) { return value() + operand; }
int Derived::plus(int operand) { return answer + operand; }
Base *selected_once(Base *object, int *count) { ++*count; return object; }
