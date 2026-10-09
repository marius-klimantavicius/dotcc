#include "zero_bound_arrays.h"

int main() {
  ZeroInterior copied = copy_interior(zero_table[0]);
  if (sizeof(ZeroInterior) != 2 || sizeof(copied.padding) != 0 ||
      copied.before != 3 || copied.after != 8 || zero_table[0].after != 7) return 1;
  if ((char *)copied.padding != &copied.after ||
      (char *)&zero_table[1] - (char *)&zero_table[0] != 2 ||
      zero_table[1].before != 11 || zero_table[1].after != 13) return 2;
  ZeroMatrix matrix = zero_matrix;
  if (sizeof(ZeroMatrix) != 2 || sizeof(matrix.padding) != 0 ||
      (char *)matrix.padding != &matrix.after || matrix.before != 17 || matrix.after != 19) return 3;
  ZeroAligned aligned = zero_aligned;
  if (sizeof(ZeroAligned) != 4 || alignof(ZeroAligned) != 4 || aligned.value != 23 ||
      (void *)aligned.padding != (void *)&aligned.value) return 4;
  zero_overlay.value = 29;
  ZeroOverlay overlay = zero_overlay;
  if (sizeof(ZeroOverlay) != 4 || overlay.value != 29 ||
      (void *)overlay.padding != (void *)&overlay.value) return 5;

  ZeroBound *p = zero_view(&copied);
  ZeroBound *q = p + zero_step();
  if (p != q || zero_calls != 1 || sizeof(*p) != 0) return 6;
  q = zero_step() + p;
  if (p != q || zero_calls != 2) return 7;
  if (p++ != q || p != q || ++p != q) return 8;
  p += zero_step();
  p -= zero_step();
  if (p != q || zero_calls != 4) return 9;
  ZeroBound *slots[2] = { p, p };
  unsigned index = 0;
  ZeroBound *previous = slots[index++]++;
  if (index != 1 || previous != p || slots[0] != p || slots[1] != p) return 10;
  ZeroBound *row = matrix.padding;
  ZeroBound *next_row = row + zero_step();
  if (row != next_row || zero_calls != 5) return 12;
  if (row-- != next_row || row != next_row || --row != next_row) return 13;

  FlexibleTail flexible = { 31 };
  UnknownBound *tail = unknown_view(&flexible);
  if (sizeof(FlexibleTail) != 4 || flexible.length != 31 ||
      (void *)tail != (void *)((char *)&flexible + sizeof(FlexibleTail))) return 11;
  return 0;
}
