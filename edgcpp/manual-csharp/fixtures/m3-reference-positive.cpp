// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored scalar references and distinct-result overload witnesses.
using Mutable = int&;
using ReadOnly = const int&;
using Movable = int&&;
int object = 7;
const int fixed = 9;
Mutable alias = object;
ReadOnly read = object;
const int& materialized = 11;
Movable movable = 13;

int select(Mutable);
long select(ReadOnly);
bool select(Movable);
int select(int& value) { return value; }
long select(const int& value) { return value; }
bool select(int&& value) { return value; }

int& left(int& value) { return value; }
const int& right(const int& value) { return value; }
int&& expiring();
int& relay() { return left(object); }
const int& readonly_relay() { return right(fixed); }
int&& expiring_relay() { return expiring(); }

int writable() { return select(alias); }
long immutable() { return select(fixed); }
bool temporary() { return select(1); }
bool xvalue() { return select(expiring()); }
int named() { return select(movable); }
