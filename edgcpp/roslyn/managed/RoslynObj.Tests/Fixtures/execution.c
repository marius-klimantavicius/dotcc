int execution_counter;
int execution_dimension(void) { return ++execution_counter + 2; }
unsigned long execution_sizeof_type(void) {
  return sizeof(int[execution_dimension()]);
}
unsigned long execution_sizeof_expression(int count) {
  int row[count];
  int (*pointer)[count] = &row;
  return sizeof(*pointer++);
}
unsigned long execution_typedef_size(void) {
  typedef int Saved[execution_dimension()];
  return sizeof(Saved) + sizeof(Saved);
}
void execution_vla_cast(void *buffer) {
  (void)((int (*)[execution_dimension()])buffer);
}
unsigned long execution_unevaluated(void) {
  return sizeof(execution_dimension()) + _Alignof(int);
}
extern int execution_incomplete[];
int execution_zero[0];
int execution_generic(int value) {
  return _Generic(value, int: value + 1, default: execution_dimension());
}
int execution_choose(int value) {
  return __builtin_choose_expr(1, value + 2, execution_dimension());
}
int execution_switch(int value) {
  switch (value) {
    case 9: return 9;
    case 1: ++value;
    case 5 ... 7: return value;
    default: return 0;
  }
}
int execution_default_only(int value) {
  switch(value) { default: return 4; }
}
int execution_casts(double value, void *pointer) {
  return (int)value + (pointer != 0);
}
