/* DIAGNOSTIC ONLY: existing dotcc declarations plus unimplemented services.
   rwlock is an opaque probe handle, NOT the glibc ABI. */
#pragma once
#include "probe-pthread-base.h"
typedef int pthread_rwlock_t;
int pthread_rwlock_init(pthread_rwlock_t *lock, const void *attr);
int pthread_rwlock_destroy(pthread_rwlock_t *lock);
int pthread_rwlock_rdlock(pthread_rwlock_t *lock);
int pthread_rwlock_wrlock(pthread_rwlock_t *lock);
int pthread_rwlock_unlock(pthread_rwlock_t *lock);
int pthread_condattr_setclock(pthread_condattr_t *attr, int clock);
