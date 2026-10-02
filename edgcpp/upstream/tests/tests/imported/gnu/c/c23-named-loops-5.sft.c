//type: fp
//options: --c23
# 0 "./c23-named-loops-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-named-loops-5.c"




# 1 "./c23-named-loops-1.c" 1




void
foo (int w)
{
  d: e: f:;
  a: b: c:
  for (int x = 0; x < 32; ++x)
    {
      if (x == 0)
 continue a;
      else if (x == 1)
 continue b;
      else if (x == 2)
 continue c;
      else if (x == 31)
 break b;
    }
  int y = 0;
  g: h:
#pragma GCC unroll 2
  while (y < 16)
    {
      ++y;
      if (y == 12)
 continue g;
      else if (y == 13)
 continue h;
      else if (y == 14)
 break g;
    }
  i: j:;
  k: l:
  switch (y)
    {
    case 6:
      break;
    case 7:
      break k;
    case 8:
      break l;
    }
  m: n: o: p:
  for (int x = 0; x < 2; ++x)
    q: r: s: t:
    switch (x)
      {
      case 0:
 u: v:
      case 3:
 w: x:
 for (int y = 0; y < 2; ++y)
   y: z:
   for (int z = 0; z < 2; ++z)
     aa: ab: ac:
     for (int a = 0; a < 2; ++a)
       ad: ae: af:
       switch (a)
  {
  case 0:
    if (w == 0)
      break ae;
    else if (w == 1)
      break ab;
    else if (w == 2)
      break z;
    else if (w == 3)
      break v;
    else if (w == 4)
      break s;
    else if (w == 5)
      break p;
    else if (w == 6)
      break;
    else if (w == 7)
      continue aa;
    else if (w == 8)
      continue y;
    else if (w == 9)
      continue x;
    else if (w == 10)
      continue m;
    ag: ah:
    do
      {
        if (w == 11)
   break ag;
        else
   continue ah;
      }
    while (0);
    break;
  default:
    break;
  }
 break;
      default:
 break;
      }
  [[]] [[]] ai:
  [[]] [[]] aj:
  [[]] [[]] ak:
  [[]] [[]] [[]]
  for (int x = 0; x < 32; ++x)
    if (x == 31)
      break ak;
    else if (x == 30)
      break aj;
    else if (x == 29)
      continue ai;
  al:
  [[]] am:
  [[]]
  do
    {
      if (w == 42)
 continue am;
      else if (w == 41)
 break al;
    }
  while (1);
  an:
  [[]] ao:
  [[]] [[]]
  while (w)
    {
      if (w == 40)
 break ao;
      else if (w == 39)
 continue an;
    }
  [[]] ap:
  [[]] aq:
  [[]]
  switch (w)
    {
    case 42:
      break ap;
    default:
      break aq;
    }
}
# 6 "./c23-named-loops-5.c" 2
