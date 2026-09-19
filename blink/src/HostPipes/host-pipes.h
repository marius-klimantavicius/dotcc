#ifndef BLINK_PRIVATE_HOST_PIPES_H
#define BLINK_PRIVATE_HOST_PIPES_H
#include <unistd.h>
#include <fcntl.h>
#define pipe blink_host_pipe
#define pipe2 blink_host_pipe2
int pipe(int *);
int pipe2(int *, int);
#endif
