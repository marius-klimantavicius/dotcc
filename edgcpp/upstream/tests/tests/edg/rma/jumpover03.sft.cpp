//options_all:-r -x -tused
//options: --strict;cn:;cp

void f(int n) {
  switch (n) {
    case 2:  goto L2;            // Error: jump over i
    case 3:  goto L3;            // Error: jump over i,j
    case 4:  goto L4;            // Error: jump over i,j
    case 5:  goto L5;            // Error: jump over i
  }
L1:
  for (int i = 10; i > 0; --i) {
    switch (n) {
      case 1:  goto L1;          // Okay
      case 3:  goto L3;          // Error: jump over j
      case 4:  goto L4;          // Error: jump over j
      case 5:  goto L5;          // Okay
    }
L2:
    for (int j = 7; j > 0; --j) {
      switch (n) {
        case 1:  goto L1;        // Okay
        case 2:  goto L2;        // Okay
        case 4:  goto L4;        // Okay
        case 5:  goto L5;        // Okay
      }
L3:;
    }
    switch (n) {
      case 1:  goto L1;          // Okay
      case 2:  goto L2;          // Okay
      case 3:  goto L3;          // Okay
      case 5:  goto L5;          // Okay
    }
L4:;
  }
  switch (n) {
    case 1:  goto L1;            // Okay
    case 2:  goto L2;            // Okay
    case 3:  goto L3;            // Error: jump over j
    case 4:  goto L4;            // Error: jump over j
  }
L5:;
}

