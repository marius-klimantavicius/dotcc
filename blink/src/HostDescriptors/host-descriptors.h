#ifndef BLINK_PRIVATE_DESCRIPTORS_H
#define BLINK_PRIVATE_DESCRIPTORS_H
#include <unistd.h>
#define dup2 blink_host_dup2
#define dup3 blink_host_dup3
int dup2(int,int);
int dup3(int,int,int);
#endif
