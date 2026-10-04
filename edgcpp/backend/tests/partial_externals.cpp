extern "C" int host_add(int);
extern "C" void *host_allocate(unsigned long);
extern "C" void host_release(void *);
extern "C" int host_value;
extern "C" int host_variadic(int, ...);

extern "C" int check_external_runtime() {
  int (*variadic)(int, ...) = &host_variadic;
  if (host_variadic(3, (short)1, 2, 3) != 6 || variadic(2, 4, 5) != 9) return -1;
  int (*indirect)(int) = &host_add;
  int *data = (int *)host_allocate(2 * sizeof(int));
  data[0] = host_add(3);
  data[1] = indirect(4);
  int sum = data[0] + data[1];
  host_release(data);
  int *address = &host_value;
  int previous = *address;
  *address = previous + 1;
  return sum + previous;
}
