//options_all:-r -x -tused
//options: --strict;cp

// EDGqa00880 -- segmentation violation with -x -b
void f() {
  for(;;) {
    try { /* ... */ }
    catch (int x) {
      if (x == 0) break;
    }
  }
}

