static int state = 19;
int second(void);
int main(void) { return state + second() == 42 ? 0 : 1; }
