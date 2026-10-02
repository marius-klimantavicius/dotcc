//file
namespace A {
	int i;
	void f(int = A::i) {}
	void g(int = i) {}
	void h(int = i);
}
void A::h(int)
{
}
