//options_all:-r -x -tused
//options: --strict;cn

struct TOP1 { };
struct LEFT1 : virtual public TOP1 { LEFT1 () { } };
struct RIGHT1 : virtual public TOP1 { RIGHT1 () { } };

struct BOTTOM1 : public LEFT1, public RIGHT1, public TOP1
{
    BOTTOM1 () : LEFT1::TOP1() {	/* OK */
    }
};

struct TOP2 { TOP2 () { } };
struct LEFT2 : virtual public TOP2 { LEFT2 () { } };
struct RIGHT2 : virtual public TOP2 { RIGHT2 () { } };

struct BOTTOM2 : public LEFT2, public RIGHT2, public TOP2
{
    BOTTOM2 () : RIGHT2::TOP2() {	/* OK */
    }
};

int main () { return 0; }

