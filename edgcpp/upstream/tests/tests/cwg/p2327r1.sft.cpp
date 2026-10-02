//options_all:--c++20 -A --diag_error=3012

int main()
{
int neck, tail;
volatile int brachiosaur;
brachiosaur = neck; // OK
tail = brachiosaur; // OK
brachiosaur = brachiosaur + neck; // OK
brachiosaur |= neck; // OK new item for p2327r1
}

