//type:fn
//options_all:--gcc
//remark:[4.4] Segmentation fault on invalid GNU case range
// 5/11/11  [EDGcpfe/11704]
//
// Segmentation fault on invalid GNU case range
//
// An invalid initial range on a GNU case range statement had resulted in a
// segmentation fault and is now fixed.
void f(int i) {
  switch (i) {
    case MISSING ... 10:    // had resulted in a segmentation violation
      break;
  }
}
