//options_all:-r -x -tused
//options: --strict;cn

struct B { virtual void pure () = 0; };

struct D : public B { };	/* D is also pure */

void B::pure () { }		/* just to confuse things */

struct S {
	B member_1;		/* ERROR */
	D member_2;		/* ERROR */
	static B member_3;	/* ERROR */
	static D member_4;	/* ERROR */
};

int main () { return 1; }


