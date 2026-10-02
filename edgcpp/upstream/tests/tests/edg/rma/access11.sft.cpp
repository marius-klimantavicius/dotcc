//options_all:-r -x -tused
//options: --strict;cp


class D {int i; };

#ifndef OK
class E : private virtual D { };
#else
class E {};             // Use this version and it is OK.
#endif

class F : public virtual D { };

class G : private E, private virtual F {
        static D &rD;
        static E &rE;
        static F &rF;
        void g();
};

G g;
G &rG = g;

D &G::rD = rG;  // We give an error here

