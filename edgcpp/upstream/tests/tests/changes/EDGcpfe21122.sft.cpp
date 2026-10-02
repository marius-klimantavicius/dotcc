//type:fn
//options_all:--microsoft_version 1920
//remark:[5.1] Microsoft compatibility: Disallow asm statements in lambda expressions
// 4/10/19  [EDGcpfe/21122]
//
// Microsoft compatibility: Disallow asm statements in lambda expressions
//
// Newer versions of MSVC reject asm statements in lambda expressions.  The front
// end has been updated to match this behavior.
void f()
{
  auto lambda = [&]
  {
    __asm { mov eax, 0 } // Previously accepted, now rejected.
  };
}
