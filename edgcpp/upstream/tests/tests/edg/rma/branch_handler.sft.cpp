//options_all:-r -x -tused
//options: --strict;cn

void f(int i) {
  if (i) goto L;        // Error
  switch (i) {
    case 0:
    try {
      if (i) goto L;    // Error
    }
    catch (int) {
      if (i) goto L;    // Okay
      ++i;
L:;
      if (i) goto L;    // Okay
    }
    catch (float) {
      case 1:           // Error on switch
        goto L;         // Error
    }
    if (i) goto L;      // Error
  }
  if (i) goto L;        // Error
}

