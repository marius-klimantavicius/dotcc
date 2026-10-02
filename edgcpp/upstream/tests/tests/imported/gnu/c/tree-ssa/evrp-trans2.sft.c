//type: fp
//options: 
# 0 "./tree-ssa/evrp-trans2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/evrp-trans2.c"




# 1 "./tree-ssa/evrp-trans.c" 1




void keep();
void kill();

void
f1 (unsigned x, unsigned y, unsigned z)
{
  if (x > y)
    if (y > z)
      {
 if (x > z)
   keep ();
 else
   kill ();
      }
}

void
f2 (unsigned w, unsigned x, unsigned y, unsigned z)
{

  if (w == z)
    if (x > y)
      if (y > z)
 {
   if (x > w)
     keep ();
   else
     kill ();
 }
}

void
f3 (unsigned a, unsigned w, unsigned x, unsigned y, unsigned z)
{

  if (a == x)
    if (w == z)
      if (x > y)
 if (y > z)
   {
     if (a > w)
       keep ();
     else
       kill ();
   }
}

void
f4 (unsigned x, unsigned y, unsigned z)
{

  if (x > y)
    if (y >= z)
      {
        if (x > z)
          keep ();
        else
          kill ();
      }
}
void
f5 (unsigned x, unsigned y, unsigned z)
{

  if (x >= y)
    if (y > z)
      {
        if (x > z)
          keep ();
        else
          kill ();
      }
}

void
f6 (unsigned x, unsigned y, unsigned z)
{

  if (x >= y)
    if (y >= z)
      {
        if (x > z)
          keep ();
        else if (x == z)
   keep ();
         else
          kill ();
      }
}

void
f7 (unsigned x, unsigned y, unsigned z)
{

  if (y <= x)
    if (z <= y)
      {
        if (x > z)
          keep ();
        else if (x == z)
   keep ();
 else
          kill ();
      }
}

void
f8 (unsigned x, unsigned y, unsigned z)
{

  if (x >= y)
    if (z <= y)
      {
        if (x > z)
          keep ();
        else if (x == z)
   keep ();
 else
          kill ();
      }
}

void
f9 (unsigned x, unsigned y, unsigned z)
{

  if (y <= x)
    if (y >= z)
      {
        if (x > z)
          keep ();
        else if (x == z)
   keep ();
        else
          kill ();
      }
}
# 6 "./tree-ssa/evrp-trans2.c" 2
