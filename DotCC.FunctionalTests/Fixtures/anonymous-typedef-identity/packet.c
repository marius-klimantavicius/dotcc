#include "shared.h"
struct Node { int value; struct Node *next; };
static struct Node node = {42, &node};
static Packet storage = {{0}, callback};
int callback(struct Node *p) { return p == p->next ? p->value : -1; }
Packet *packet(void) { storage.value.node = &node; return &storage; }
int inspect(Packet *p) { return p->callback(p->value.node); }
