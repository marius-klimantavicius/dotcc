//options_all:-r -x -tused
//options: --strict;cp

namespace std {
  extern "C" {
    int _xstat(const int, const char *, struct stat *);
    static int stat(const char *, struct stat *);
    static int stat(const char *_path, struct stat *_buf) {
      return (_xstat(2, _path, _buf));
    }
  } // extern "C"
} // namespace std

