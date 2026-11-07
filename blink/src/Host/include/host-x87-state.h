#ifndef BLINK_HOST_X87_STATE_H_
#define BLINK_HOST_X87_STATE_H_
struct Machine;
/* ProducerProtocol1; all buffers are exclusive owned host staging images. */
int blink_host_guest_x87_active(struct Machine *, unsigned);
int blink_host_guest_x87_state(struct Machine *, unsigned, int, void *, unsigned);
void blink_host_guest_x87_reject_legacy(struct Machine *, unsigned);
#endif
