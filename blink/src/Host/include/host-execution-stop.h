#ifndef BLINK_PRIVATE_EXECUTION_STOP_H
#define BLINK_PRIVATE_EXECUTION_STOP_H
/* Values match HostExecutionStopReason; no guest signal or exit status implied. */
#define BLINK_EXECUTION_RUNNING 0
#define BLINK_EXECUTION_REQUESTED 1
#define BLINK_EXECUTION_DEADLINE 2
#define BLINK_EXECUTION_BUDGET 3
int blink_host_execution_stop_reason(void);
#endif
