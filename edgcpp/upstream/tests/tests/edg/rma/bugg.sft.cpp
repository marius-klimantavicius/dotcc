//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:--diag_warn=260;cp

extern "C" exit(int);

// standard conversion: derived* --> base*
// Single Inheritance

// class Bw { /* ... */ };
// class C1 {
// public:
//      class Bx : public Bw { /* ... */ };
// };
// class By : virtual public C1::Bx { /* ... */ };
// 
// class C2 {
// public:
//      class Bz : public virtual By { /* ... */ };
// };
// class D : public C2::Bz { /* ... */ };


int x=21;
enum { Bw_e=2, Bx_e=3, By_e=5, Bz_e=7, D_e=11 };

class Bw {
public:
        int i;
        int f() { return i; }
        operator int() { return this->i; }
        int operator +() { return 2*this->i; }
        int Bw_i;
        int Bw_f() { return Bw_i; }
        Bw() : i(Bw_e), Bw_i(Bw_e) { }
};

class C1 {
public:
        class Bx : public Bw {
        public:
                int i;
                int f() { return i; }
                operator unsigned() { return this->i; }
                int operator -() { return 2*this->i; }
                int Bx_i;
                int Bx_f() { return Bx_i; }
                Bx() : i(Bx_e), Bx_i(Bx_e) { }
        };
};

class By : virtual public C1::Bx {
public:
        int i;
        int f() { return i; }
        operator long() { return this->i; }
        int operator !() { return 2*this->i; }
        int By_i;
        int By_f() { return By_i; }
        By() : i(By_e), By_i(By_e) { }
};

class C2 {
public:
        class Bz : public virtual By {
        public:
                int i;
                int f() { return i; }
                operator unsigned long() { return this->i; }
                int operator ~() { return 2*this->i; }
                int Bz_i;
                int Bz_f() { return Bz_i; }
                Bz() : i(Bz_e), Bz_i(Bz_e) { }
        };
};

class D : public C2::Bz {
public:
};


