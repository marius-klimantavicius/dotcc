//type: s
//options: --c++14
# 0 "./cpp1y/lambda-generic-xudt.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp1y/lambda-generic-xudt.C"




# 1 "./cpp1y/lambda-generic-udt.C" 1




int i = 3;

struct S
{
  S () { ++i; }
  S (S const&) { ++i; }
  S (S&& old) { old.shadow = true; i += 2; }
  ~S () { if (shadow) i -= 2; else --i; }

  bool shadow = false;
};

extern "C" int printf(const char*, ...);



int main ()
{
  if (i == 3); else printf ("%s:%d: !(%s)\n", "./cpp1y/lambda-generic-udt.C", 23, "i == 3"), __builtin_abort ();;
  {
    S s; if (i == 4); else printf ("%s:%d: !(%s)\n", "./cpp1y/lambda-generic-udt.C", 25, "i == 4"), __builtin_abort ();;
# 44 "./cpp1y/lambda-generic-udt.C"
    byref (s); if (i == 4); else printf ("%s:%d: !(%s)\n", "./cpp1y/lambda-generic-udt.C", 44, "i == 4"), __builtin_abort ();;
    bycref (s); if (i == 4); else printf ("%s:%d: !(%s)\n", "./cpp1y/lambda-generic-udt.C", 45, "i == 4"), __builtin_abort ();;
    byval (s, 5); if (i == 4); else printf ("%s:%d: !(%s)\n", "./cpp1y/lambda-generic-udt.C", 46, "i == 4"), __builtin_abort ();;
    byrval (static_cast<S&&>(s), 6); if (i == 5); else printf ("%s:%d: !(%s)\n", "./cpp1y/lambda-generic-udt.C", 47, "i == 5"), __builtin_abort ();;
  }
  if (i == 3); else printf ("%s:%d: !(%s)\n", "./cpp1y/lambda-generic-udt.C", 49, "i == 3"), __builtin_abort ();;
}
# 6 "./cpp1y/lambda-generic-xudt.C" 2
