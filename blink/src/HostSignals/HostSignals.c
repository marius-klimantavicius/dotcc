#include "HostSignals.h"

static _Thread_local blink_host_sigset virtual_host_delivery_mask;

void BlinkHostDeliveryMaskReset(void) {
  for (int i = 0; i != 16; ++i) virtual_host_delivery_mask.words[i] = 0;
}

void BlinkHostDeliveryMaskRead(blink_host_sigset *mask) {
  *mask = virtual_host_delivery_mask;
}

void BlinkHostDeliveryMaskWrite(const blink_host_sigset *mask) {
  virtual_host_delivery_mask = *mask;
}

BlinkHostJumpSlot PrepareVirtualSignalJump(BlinkHostSignalJump *env, int save_mask) {
  env->mask_saved = save_mask != 0;
  if (env->mask_saved) env->mask = virtual_host_delivery_mask;
  return BlinkHostSlotAddress(env);
}

void blink_host_siglongjmp(BlinkHostSignalJump *env, int value) {
  if (env->mask_saved) virtual_host_delivery_mask = env->mask;
  BlinkHostOrdinaryJump(env, value);
}
