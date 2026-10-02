//options_all:-r -x -tused
//options: --strict;cn:;cp

int i;
main () {
  for (int i = 0; i < 10; i++) { }
  i = 0;
  ::i = 1;
  i = 2;
}


