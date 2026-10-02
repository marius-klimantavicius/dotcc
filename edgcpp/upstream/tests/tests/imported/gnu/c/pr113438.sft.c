//type: fp
//options: --c23
/* PR113438
 * { dg-do compile }
 * { dg-options "-std=c23 -g" } */

void g(struct foo { int x; } a);
void g(struct foo { int x; } a);

