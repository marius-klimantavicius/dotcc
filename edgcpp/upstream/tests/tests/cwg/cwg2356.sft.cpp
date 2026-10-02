//type:fp
//options_all:--c++20 -A

struct A_ {
        A_() { }
        A_(A_ &&) { }
        template<class T> A_(T &&) { }
};
struct B_ : A_ {
        using A_::A_;
        B_(const B_ &);
        B_(B_ &&) = default;
        struct C_ { C_(C_ &&) = delete; } c_;
};
extern B_ ba_;
B_ bb_ = static_cast<B_&&>(ba_);
struct D_ { operator B_&&(); };
B_ bc_ = D_();

//cwg: 2356
//title: Base class copy and move constructors should not be inherited
//meeting: Rapperswil 6/18
//edg_status: EDGcpfe/21867
//fixed_in: 6.8
