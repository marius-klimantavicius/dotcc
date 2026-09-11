static inline const char *message(void) { return "hello"; }
int main(void) { return message()[0] - 'h'; }
