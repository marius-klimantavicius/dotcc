/* ABI fixture only: absolute dynamic symbol whose successful value is NULL.
   No application algorithm or executable native function body. */
__asm__(".globl native_loader_zero\n.set native_loader_zero, 0\n");
