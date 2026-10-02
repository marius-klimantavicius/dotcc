//type: fp
//options: 
# 0 "./warn/Wredundant-tags-5.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Wredundant-tags-5.C"







# 1 "./warn/Wredundant-tags-5.h" 1
# 10 "./warn/Wredundant-tags-5.h"
extern "C" {

  class C1 { };
  enum class EC1 { };
  enum E1 { };
  struct S1 { };
  union U1 { };

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wredundant-tags"
  class C1 fc1 (class C1);
  enum class EC1 fce1 (enum class EC1);
#pragma GCC diagnostic pop

  enum E1 fe1 (enum E1);
  struct S1 fs1 (struct S1);
  union U1 fu1 (union U1);

  C1 fc1 (C1);
  EC1 fce1 (EC1);
  E1 fe1 (E1);
  S1 fs1 (S1);
  U1 fu1 (U1);
}


extern "C++" {

  class C2 { };
  enum class EC2 { };
  enum E2 { };
  struct S2 { };
  union U2 { };

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wredundant-tags"
  class C2 fc2 (class C2);
  enum class EC2 fce2 (enum class EC2);
  struct S2 fs2 (struct S2);
  union U2 fu2 (union U2);
#pragma GCC diagnostic pop

  C2 fc2 (C2);
  EC2 fce2 (EC2);
  E2 fe2 (E2);
  S2 fs2 (S2);
  U2 fu2 (U2);
}


class C3 { };
enum class EC3 { };
enum E3 { };
struct S3 { };
union U3 { };

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wredundant-tags"
class C3 fc3 (class C3);
enum class EC3 fce3 (enum class EC3);
struct S3 fs3 (struct S3);
union U3 fu3 (union U3);
#pragma GCC diagnostic pop

C3 fc3 (C3);
EC3 fce3 (EC3);
E3 fe3 (E3);
S3 fs3 (S3);
U3 fu3 (U3);
# 9 "./warn/Wredundant-tags-5.C" 2

extern "C" {

  class C1
  fc1 (C1)
  {
    return C1 ();
  }

  EC1
  fce1 (enum class EC1)
  {
    return EC1 ();
  }

  E1
  fe1 (E1)
  {
    return (enum E1)0;
  }

  struct S1
  fs1 (S1)
  {
    return S1 ();
  }

  U1
  fu1 (union U1)
  {
    return U1 ();
  }

}


extern "C++" {

  class C2
  fc2 (C2)
  {
    return C2 ();
  }

  EC2
  fce2 (enum class EC2)
  {
    return EC2 ();
  }

  E2
  fe2 (E2)
  {
    return (enum E2)0;
  }

  struct S2
  fs2 (S2)
  {
    return S2 ();
  }

  U2
  fu2 (union U2)
  {
    return U2 ();
  }

}


class C3
fc3 (C3)
{
  return C3 ();
}

EC3
fce3 (enum class EC3)
{
  return EC3 ();
}

E3 fe3 (E3)
{
  return (enum E3)0;
}

struct S3
fs3 (S3)
{
  return S3 ();
}

U3
fu3 (union U3)
{
  return U3 ();
}
