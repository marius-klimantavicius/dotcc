//options_all:-r -x -tused
//options: --strict;ln

struct Collection {
        virtual ~Collection();
};

struct Set: public Collection {
};

struct IdentSet : public Set {
        virtual void f();       // This is required!!
};

int main()
{
        IdentSet *p = new IdentSet;
        delete p;
}

