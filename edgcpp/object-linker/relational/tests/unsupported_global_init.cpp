int side_effect();
int unused_global = side_effect();
int side_effect() { return 42; }
int main() { return 0; }
