//remark:_Generic
//options:--c11;fn

  int r = _Generic((void (*)())0,
                   void (*)(int)  : 0,
                   void (*)(void) : 0);  // Previously aborted.  Now an
                                         // ordinary error.

