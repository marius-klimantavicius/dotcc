//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --cfront_3.0;rp

extern "C" int printf(char*, ...);
int ivalue(int i) { return i; }
void ieq(int i,int j) { printf("%d %s %d\n", i, i==j?"==":"!=", j); }
extern void do_nothing(void*) { }

// _124p35: dtors for virtual bases execute in reverse order of appearance
// in depth-first left-to-right traversal in DAG of bases
//
// Here's a representation of the DAG, with the derivations of E from A
// and H from B shown rather inadequately:
//
//            A   B 
//           / \ / \
//          C   D   E--(A)
//           \ / \ /
//            F   G
//             \ / 
//              H--(B)
//
// From this it's clear that the destructors of the virtual base classes
// of H should be called in the order G-D-B-A.  That's not how cfront does
// it, by the way.
//
int dtor = 0;
struct A
	{
	~A() { ieq(++dtor, ivalue(4)); }
	};
struct B
	{
	~B() { ieq(++dtor, ivalue(3)); }
	};
struct C : virtual public A { };
struct D : virtual public A, virtual public B
	{
	~D() { ieq(++dtor, ivalue(2)); }
	};
struct E : virtual public B, virtual public A { };
struct F : public C, virtual public D { };
struct G : virtual public D, public E
	{
	~G() { ieq(++dtor, ivalue(1)); }
	};
struct H : public F, virtual public G, virtual public B { };

int main()
{
                {
		H h;
		do_nothing(&h);
		}
	ieq(dtor, ivalue(4));
}

