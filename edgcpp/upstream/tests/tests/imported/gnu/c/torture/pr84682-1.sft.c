//type: fp
//options: 
/* { dg-do compile } */

void b(char a) {
        asm("" : : "pir" (a));
}
