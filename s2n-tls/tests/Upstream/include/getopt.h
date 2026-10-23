#ifndef DOTCC_UPSTREAM_GETOPT_H
#define DOTCC_UPSTREAM_GETOPT_H
/* Linux x64 GNU getopt ABI. Test-only declaration surface for the original
 * upstream CLI. Option parsing is the actual host libc implementation. */
struct option {
    const char *name;
    int has_arg;
    int *flag;
    int val;
};
#define no_argument 0
#define required_argument 1
#define optional_argument 2
int getopt_long(int argc, char *const argv[], const char *short_options,
        const struct option *long_options, int *long_index);
char **dotcc_upstream_optarg(void);
int *dotcc_upstream_optind(void);
int *dotcc_upstream_opterr(void);
int *dotcc_upstream_optopt(void);
#define optarg (*dotcc_upstream_optarg())
#define optind (*dotcc_upstream_optind())
#define opterr (*dotcc_upstream_opterr())
#define optopt (*dotcc_upstream_optopt())
#endif
