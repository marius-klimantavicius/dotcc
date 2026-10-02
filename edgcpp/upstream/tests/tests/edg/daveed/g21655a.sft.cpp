//remark:Error recovery in scan_typeof
//options:--gnu=40800;fn

  int x;
  int typeof;  // Aborts in version 5.1.  Now an ordinary error.

