//type:fp
//options_all:--microsoft_v 1911
struct B_1 {};
struct B_2 {
    operator B_1() const;
    void B_1() const;
};
void test() {
    B_2 b;
    b.operator B_1();
}
