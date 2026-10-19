#include "graph.h"
extern struct Node scalar;
struct Node later[2]={{&scalar,&later[1],6},{&later[0],&scalar,7}};
