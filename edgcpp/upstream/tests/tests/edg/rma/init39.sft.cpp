//options_all:-r -x -tused
//options: --strict;rp

extern "C" int printf(char *, ...);
void ieq(int i, int j) { printf("%d %s %d\n", i, i==j?"==":"!=", j); }
int ivalue(int i) { return i; }

int main() {
	{
	// elaborated-type-specifier:
	//     class-key ::-opt nested-name-specifier-opt identifier
	// this variant tests class-key nested-name-specifier identifier
		struct A
			{
			struct B
				{
				struct C { int i; } C;
				};
			struct B B;
			};
		A a = { { { 0 } } } ;
		ieq(a.B.C.i, ivalue(0));
		struct A::B b = { { 1 } };          /* ! ARM */
		a.B = b;
		ieq(a.B.C.i, ivalue(1));
		struct A::B::C c = { 2 };
		a.B.C = c;
		ieq(a.B.C.i, ivalue(2));
	
	}
}


