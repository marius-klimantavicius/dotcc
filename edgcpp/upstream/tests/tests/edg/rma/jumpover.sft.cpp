//options_all:-r -x -tused
//options: --cfront_3.0;cp

void f(int i) {
  switch (i) {
    case 1:  goto L1;     // error: jump over j;             (cfront j)
    case 2:  goto L2;     // error: jump over j,k;           (cfront k)
    case 3:  goto L3;     // error: jump over j;             (cfront j)
    case 4:  goto L4;     // error: jump over j,m;           (cfront m)
  }  /* switch */
  int j = 0;
  switch (i) {
    case 1:  goto L1;     // okay
    case 2:  goto L2;     // error: jump over k;             (cfront k)
    case 3:  goto L3;     // okay
    case 4:  goto L4;     // error: jump over m;             (cfront m)
  }  /* switch */
  if (i != 0) {
L1:;
    switch (i) {
      case 1:  goto L1;   // okay
      case 2:  goto L2;   // error: jump over k;             (cfront k)
      case 3:  goto L3;   // error??? (jump over k,l?)
      case 4:  goto L4;   // error: jump over m (also k,l?)  (cfront m)
    }  /* switch */
    int k = 0;
L2:;
    switch (i) {
      case 1:  goto L1;   // okay
      case 2:  goto L2;   // okay
      case 3:  goto L3;   // error??? (jump over l?)
      case 4:  goto L4;   // error: jump over m (also l?)    (cfront m)
    }  /* switch */
    int l = 0;
  }
L3:
  int m = 0;
L4:;
  switch (i) {
    case 1:  goto L1;     // okay
    case 2:  goto L2;     // error: jump over k;            (cfront generic)
    case 3:  goto L3;     // okay
    case 4:  goto L4;     // okay
  }  /* switch */
}

