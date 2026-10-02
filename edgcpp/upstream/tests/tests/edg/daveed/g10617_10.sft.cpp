//remark:noexcept
//options:--c++11;fp:--c++11 -A;fp

// [class.copy]
// test move constructors and assignment operators with default/delete

struct A {
	A() = default;
	~A() = default;
	A(A&) = default;
	A(const A&) = default;
	A(A&&) = default;
	A& operator=(A&) = default;
	A& operator=(const A&) = default;
	A& operator=(A&&) = default;
	virtual void f();
	int x : 7;
};

struct B : virtual A {
	A a;
	B() = default;
	~B() = default;
	B(B&) = default;
	B(const B&) = default;
	B(B&&) = default;
	B& operator=(B&) = default;
	B& operator=(const B&) = default;
	B& operator=(B&&) = default;
	virtual void f();
	int y : 13;
};

struct C : virtual A, virtual B {
	A a;
	B b;
	C() = default;
	~C() = default;
	C(C&) = default;
	C(const C&) = default;
	C(C&&) = default;
	C& operator=(C&) = default;
	C& operator=(const C&) = default;
	C& operator=(C&&) = default;
	virtual void f();
	int z : 19;
};

void f()
{
	B b;
	B bb = (B&&)b;
	b = bb;
	b.f();

	C c;
	C cc = (C&&)c;
	c = cc;
	c.f();
}
