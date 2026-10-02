//type:fp
//options:--microsoft_version 1900:--microsoft_version 1916
//options_all:-w

void f()
{
  int y = 0;
  int x = 0;
  __asm {
    mov eax, x
    mov y, eax
  }
}
