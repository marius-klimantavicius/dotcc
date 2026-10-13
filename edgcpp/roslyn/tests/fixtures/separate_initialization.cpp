int initialization_effects;
int initialize(int value) { initialization_effects += value; return value; }

int global_first = initialize(1);
int global_second = initialize(2);
thread_local int thread_first = initialize(3);

struct SeparatePriority {
  int value;
  SeparatePriority(int input) : value(input) {}
  ~SeparatePriority() { initialization_effects += value; }
};
SeparatePriority global_priority __attribute__((init_priority(201))) (4);

int initialization_use() {
  return global_first + global_second + thread_first + global_priority.value;
}
