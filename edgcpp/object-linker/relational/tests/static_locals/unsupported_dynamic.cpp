int initial_value() { return 7; }
int &dynamic_value() { static int value = initial_value(); return value; }
int main() { return dynamic_value(); }
