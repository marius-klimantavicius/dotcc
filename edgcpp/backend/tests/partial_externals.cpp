extern "C" int host_add(int);
extern "C" void *host_allocate(unsigned long);
extern "C" void host_release(void *);
extern "C" int host_value;

extern "C" int check_external_runtime() {
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
