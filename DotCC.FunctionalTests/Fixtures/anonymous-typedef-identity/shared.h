struct Node;
typedef union { long number; struct Node *node; } SharedValue;
typedef struct { SharedValue value; int (*callback)(struct Node *); } Packet;
Packet *packet(void);
int callback(struct Node *);
int inspect(Packet *);
