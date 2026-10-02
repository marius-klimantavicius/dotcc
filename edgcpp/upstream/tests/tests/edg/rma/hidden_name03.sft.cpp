//options_all:-r -x -tused
//options: --strict;rp

//11277 - bug4.c
int main() {
        int A = 27;
        enum A { B=32 };
        enum A o = B;
}


