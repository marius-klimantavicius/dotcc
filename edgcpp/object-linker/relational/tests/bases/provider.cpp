#include "program.h"

int trace = 0;
void mark(int value) { trace = trace * 10 + value; }

Left::Left(int value) : left(value) { mark(1); }
Left::~Left() { mark(6); }
int Left::read_left() const { return left; }

Right::Right(int value, bool fail) : right(value) {
    mark(2);
    if (fail) throw 7;
}
Right::~Right() { mark(7); }
int Right::read_right() const { return right; }

Derived::Derived(bool fail) : Left(10), Right(20, fail), value(30) { mark(3); }
Derived::~Derived() { mark(4); }

int exercise(bool fail) {
    Derived object(fail);
    Left *left = &object;
    Right *right = &object;
    if (left->read_left() != 10) return 1;
    if (right->read_right() != 20) return 2;
    if (add_right(right, 2) != 22) return 3;
    Derived *back = static_cast<Derived *>(right);
    if (back != &object) return 4;
    if (back->value != 30) return 5;
    Derived *null_derived = nullptr;
    Right *null_right = null_derived;
    if (null_right != nullptr) return 6;
    if (static_cast<Derived *>(null_right) != nullptr) return 7;
    return 0;
}
