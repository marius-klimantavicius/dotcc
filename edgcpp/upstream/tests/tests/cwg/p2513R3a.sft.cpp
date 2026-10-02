//type:fn
//options_all:--c++20 -tused -A
struct A {
	char8_t s[10];
};
struct B {
	char s[10];
};

void f(A);
void f(B);

int main() {
	f({u8""}); // ambiguous
}
