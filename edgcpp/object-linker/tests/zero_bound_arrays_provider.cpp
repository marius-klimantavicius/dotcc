#include "zero_bound_arrays.h"

ZeroInterior zero_table[2] = { { 3, {}, 7 }, { 11, {}, 13 } };
ZeroMatrix zero_matrix = { 17, {}, 19 };
ZeroAligned zero_aligned = { {}, 23 };
ZeroOverlay zero_overlay = {};
unsigned zero_calls;

ZeroBound *zero_view(ZeroInterior *value) { return &value->padding; }
UnknownBound *unknown_view(FlexibleTail *value) { return &value->data; }
ZeroInterior copy_interior(ZeroInterior value) { value.after += 1; return value; }
unsigned zero_step() { ++zero_calls; return 5; }
