int _ = 91;
int trace = 0;
int value() { trace = trace * 10 + 1; return 17; }
void effect() { trace = trace * 10 + 2; }

int main() {
  (void)0;
  (void)value();
  (void)((void)effect());
  int local = 3;
  (void)local;
  (void)(local = 9);
  for (int i = 0; i != 2; (void)effect(), ++i) {}
  return trace == 1222 && local == 9 && _ == 91 ? 0 : 1;
}
