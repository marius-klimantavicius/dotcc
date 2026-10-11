#include "program.h"
int alive = 0;
int expected_alive = 0;
int bad_steps = 0;
int begins = 0;
int ends = 0;
Watch::Watch(int input) : value(input) { if (input < 0) throw 17; ++alive; }
Watch::~Watch() { --alive; }
Sentinel::Sentinel(int *value) : finish(value) {}
Cursor::Cursor(int *value) : pointer(value) {}
Range::Range(int *data, int size) : values(data), count(size) {}
int &Cursor::operator*() const { return *pointer; }
Cursor &Cursor::operator++() { if (alive != expected_alive) ++bad_steps; ++pointer; return *this; }
bool Cursor::operator!=(const Sentinel &other) const { return pointer != other.finish; }
Cursor Range::begin() const { ++begins; return Cursor{values}; }
Sentinel Range::end() const { ++ends; return Sentinel{values + count}; }
int early_return(Range &range) { for (Watch item : range) { return item.value; } return 0; }
