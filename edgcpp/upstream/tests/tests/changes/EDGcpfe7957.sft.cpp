//type:fp
//options_all:--g++
//remark:[4.4] GNU compatibility: increment/decrement on expressions with complex type
// 7/12/11  [EDGcpfe/7957,EDGcpfe/11897]
//
// GNU compatibility: increment/decrement on expressions with complex type
//
// GNU allows (both pre- and post-) increment and decrement operations on
// expressions with complex type.  These operations have the effect of increasing
// or decreasing the "real" component by 1.0 (of the appropriate type) while
// leaving the imaginary component unchanged (i.e., adding/subtracting 1.0+0.0i).
// This is now allowed in the front end in both C and C++ GNU emulation modes
// and lowered when LOWER_COMPLEX is TRUE.
void g() {
  _Complex long double l = 0.0+1.0i;
  _Complex double d = 0.0+1.0i;
  _Complex float f = 0.0+1.0i;
  --l;
  d--;
  ++f;
  (d=f)++;    // only in C++ mode
}
