//type:fp
//options:--c23

void test_var(void) {
  const int        a = 0;
  typeof_unqual(a) b = 1;

  b = 2;
}

void test_array_var(void) {
  const char* const alphabet[3] = {
    "a", "b", "c"
  };

  const char              a;
  const char              b;
  const char              c;
  typeof_unqual(alphabet) arr = {&a, &b, &c};
  arr[0] = &c;
  arr[1] = &b;
  arr[2] = &a;
}
