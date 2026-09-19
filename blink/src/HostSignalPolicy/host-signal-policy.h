#ifndef BLINK_HOST_SIGNAL_POLICY_H
#define BLINK_HOST_SIGNAL_POLICY_H
#include <signal.h>
#include <sys/time.h>
#include <unistd.h>
#define alarm blink_host_alarm
#define pause blink_host_pause
unsigned alarm(unsigned);
int pause(void);
#endif
