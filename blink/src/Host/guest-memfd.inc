/* Included in staged syscall.c. Match ordinary fd publication and exec-lock
 * ordering; the returned number belongs to InstanceIo, never the host OS. */
static int SysMemfdCreate(struct Machine *m, i64 nameaddr, u32 flags) {
  const char *name;
  char *label;
  struct Fd *record;
  int fd, limit;
  if (!(name = LoadStr(m, nameaddr))) return -1;
  if (!(limit = GetFileDescriptorLimit(m->system))) return emfile();
  if (flags & 1) LOCK(&m->system->exec_lock);
  fd = blink_host_memfd_create(name, flags);
  if (fd != -1) {
    if (fd >= limit) {
      VfsClose(fd);
      fd = emfile();
    } else if (!(label = (char *)malloc(strlen(name) + 18))) {
      VfsClose(fd);
      fd = enomem();
    } else {
      snprintf(label, strlen(name) + 18, "/memfd:%s (deleted)", name);
      LOCK(&m->system->fds.lock);
      record = AddFd(&m->system->fds, fd, O_RDWR | (flags & 1 ? O_CLOEXEC : 0));
      /* File-map bookkeeping requires a nonnull diagnostic label. This is not
       * a pathname that may be opened in either guest or host filesystem. */
      if (record) record->path = label;
      UNLOCK(&m->system->fds.lock);
      if (!record) {
        free(label);
        VfsClose(fd);
        fd = enomem();
      }
    }
  }
  if (flags & 1) UNLOCK(&m->system->exec_lock);
  return fd;
}
