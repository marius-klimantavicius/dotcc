//type: fp
//options: 
# 0 "./tree-ssa/pr38699.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/pr38699.c"
# 21 "./tree-ssa/pr38699.c"
static void delay_wait_us( unsigned char timeout ) {
    __asm__ __volatile__ ("wdr");

    (*(volatile unsigned char *)((0x32) + 0x20)) = timeout;
    (*(volatile unsigned char *)((0x38) + 0x20)) |= (1 << (0));


    while(!((*(volatile unsigned char *)((0x38) + 0x20)) & (1 << (0))));
}




int a;
static void delay_wait_us_ms( unsigned char timeout ) {
    delay_wait_us( timeout * a );
}


void makeUseOfTimerWait( unsigned char val ) {
    delay_wait_us( 10 );
    (*(volatile unsigned char *)((0x14) + 0x20)) |= 0x10;
    delay_wait_us( 10 );
    (*(volatile unsigned char *)((0x11) + 0x20)) |= 0x20;

    delay_wait_us( 20 );
    (*(volatile unsigned char *)((0x15) + 0x20)) &= ~0x10;
    delay_wait_us( 10 );
    (*(volatile unsigned char *)((0x12) + 0x20)) &= ~0x20;

    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );
    delay_wait_us_ms( 5 );

    (*(volatile unsigned char *)((0x15) + 0x20)) |= val & 0x10;
    delay_wait_us( 10 );
    (*(volatile unsigned char *)((0x12) + 0x20)) |= val & 0x20;

    delay_wait_us( 10 );
}
