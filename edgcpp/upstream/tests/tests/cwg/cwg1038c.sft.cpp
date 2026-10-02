//options_all:--c++20 -tused -A

    void g1(int*) {}
    void g1(long) {}

    void foo1() {
        (&g1)(0L);
    }
