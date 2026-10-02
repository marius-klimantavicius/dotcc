//options_all:-r -x -tused
//options: --strict;cn

void f() {
  for (int i = 0;
       int i = 1; ) { }         // Error (for-init, condition)
  for (int i = 0; ; ) {	   
    int i;                      // Error (for-init, outermost)
    { int i; }                  // Okay
    break;		   
  }			   
  for (; int i = 1; ) {	   
    int i;                      // Error (condition, outermost)
    { int i; }                  // Okay
    break;		   
  }			   
  for (int i = 0;	   
       int i = 1; ) {           // Error (for-init, condition)
    int i;                      // Error (for-init/condition, outermost)
    { int i; }                  // Okay
    break;
  }
}

