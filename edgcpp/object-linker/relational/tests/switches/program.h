extern int alive;
struct Watch { int value; Watch(int input); ~Watch(); operator int() const; };
int regular(int input);
int controlled(int input);
int nested();
int wide(unsigned long long input);
int skipped(int input);
