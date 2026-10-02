//options_all:-r -x -tused
//options: --strict;cn:;cp

// -x and --long_lifetime_temps
main () {
  int i;
  for (i = 0; i < 3; i++) new long (i);
}

