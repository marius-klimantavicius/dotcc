//options:--c++11;fp:--gnu_version 70400;fp:--microsoft_version 1900;fp:--microsoft_version 1916;fn
//options_all:-w

#ifdef __asm
#undef __asm
#endif /* __asm */

void f()
{
  int y = 0;
  int x = 0;
  auto lambda = [&]
  {
   __asm ("\
      mov eax, x\
      mov y, eax\
    ");
  };
}
