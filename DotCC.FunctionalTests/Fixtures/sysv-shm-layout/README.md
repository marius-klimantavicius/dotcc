The PGlite fork's pglitec.c owns its shared memory implementation and needs
System V data declarations. This reduction failed because sys/shm.h and
sys/ipc.h were missing. Compare emitted field offsets, signed keys/times and
64-bit segment lengths with Linux x64 libc. Kernel shared memory operations
are outside this change; no success stubs or native imports are added.
