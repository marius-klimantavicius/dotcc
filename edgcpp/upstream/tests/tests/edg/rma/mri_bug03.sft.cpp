//options_all:-r -x -tused
//options: --strict;cn:--diag_warning=260;rp

class C {                                                                      
public:
	struct {
		f() {};
	} S;
};                                                                             
main()
{
	C *c1; c1->S.f();
}          

