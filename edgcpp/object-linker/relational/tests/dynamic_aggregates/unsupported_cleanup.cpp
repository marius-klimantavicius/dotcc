struct Token { int value; Token(int input) : value(input) {} ~Token() {} };
struct Packet { Token token; int tail; };
int main() { Packet value{Token(1), 2}; return value.tail; }
