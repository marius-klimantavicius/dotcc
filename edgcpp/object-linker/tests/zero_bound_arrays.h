#pragma once

// These tests deliberately use GNU fixed-zero arrays and flexible members.
// No standalone zero-sized allocation or zero-stride pointer subtraction is
// exercised. Fixed [0] is complete; an unspecified [] bound is incomplete.
typedef char ZeroBound[0];
typedef char UnknownBound[];

struct ZeroInterior { char before; char padding[0]; char after; };
struct ZeroMatrix { char before; char padding[3][0]; char after; };
struct ZeroAligned { unsigned padding[0]; char value; };
union ZeroOverlay { unsigned padding[0]; unsigned value; };
struct FlexibleTail { unsigned length; char data[]; };

extern ZeroInterior zero_table[2];
extern ZeroMatrix zero_matrix;
extern ZeroAligned zero_aligned;
extern ZeroOverlay zero_overlay;
extern unsigned zero_calls;

ZeroBound *zero_view(ZeroInterior *value);
UnknownBound *unknown_view(FlexibleTail *value);
ZeroInterior copy_interior(ZeroInterior value);
unsigned zero_step();
