//remark:Copy elision with trivial copy constructor
//options:--c++17;rp

	static int count = 0;
	
	struct A
	{
          int x;
	  A() = default;
	  A(const A&) = default;
	  ~A() { count++; }
	};

	A foo(A *pa)
	{
	  // Creates temporary
	  return static_cast<A>(*pa);
	}

	int main(void)
	{
	  A a;
	  A x = foo(&a);
	  return count != 0;
	}
