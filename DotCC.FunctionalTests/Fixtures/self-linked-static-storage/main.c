#include <stdio.h>
#include "graph.h"
extern struct Node later[2];
struct Node *forward = &later[1];
struct Node scalar = {.next=&scalar,.prev=&scalar,.value=9};
static struct Node nodes[3] = {{&nodes[0],&nodes[2],1},{&nodes[2],&nodes[0],2},{&nodes[1],&nodes[1],3}};
struct Node *early = &nodes[1];
unsigned long storage_address(void) { return (unsigned long)&nodes[0]; }
int storage_identity_check(void) { return scalar.next==&scalar && nodes[0].next==&nodes[0] && nodes[1].next==&nodes[2] && forward==&later[1] && later[0].next==&scalar && later[1].next==&later[0]; }
int main(void) { struct Node head = { &head, &head, 4 }; static struct Node saved = {&saved,&saved,5};
 printf("%d %d %d %d %d %d %d %d %d %d\n",scalar.next==&scalar,scalar.prev->value,nodes[0].next==&nodes[0],nodes[1].next->value,early==&nodes[1],head.next==&head,saved.next==&saved,head.prev->value+saved.prev->value,forward->value,storage_identity_check()); return 0; }
