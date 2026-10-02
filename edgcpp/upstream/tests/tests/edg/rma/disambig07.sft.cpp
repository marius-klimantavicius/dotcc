//options_all:-r -x -tused
//options: --strict;cn:;cp

void s822p1b()
{
  char carray[32] = "variable string literal";
  char (&const constr)[32] = carray;
  ++constr[0];
}

