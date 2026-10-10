int sum;
struct Check { ~Check() { if (sum != 42) throw 1; } };
Check check;
struct Element { int value; ~Element() { sum += value; } };
using Alias = Element;
Alias values[2] = {{11}, {31}};
int main() { return values[0].value + values[1].value == 42 ? 0 : 1; }
