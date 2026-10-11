extern int trace;
extern int selections;

struct TestValue { int *alias; int expected; unsigned mask; };
struct Backoff { int *owner; TestValue test; int tail; };

int mark(int event, int value);
int *mark_owner(int event, int *owner);
TestValue &selected(TestValue &value);
TestValue make_test(int *alias, int expected, unsigned mask);
Backoff from_reference(int *owner, TestValue &test);
