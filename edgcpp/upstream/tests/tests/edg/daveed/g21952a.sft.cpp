//remark:_Generic
//options:--c11;fn

void foo()
{
  (void) _Generic((void (*)())0,
                   void (*)(int)  : 0,
                   void (*)(void) : 0);
}
