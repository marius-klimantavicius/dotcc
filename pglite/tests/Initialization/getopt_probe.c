/* Execute the original PostgreSQL option parser, including its own globals. */
#include "postgres_fe.h"
#include "getopt_long.h"

int dpg_getopt_probe(void)
{
    struct option options[] = {
        {"name", required_argument, NULL, 'n'},
        {"flag", no_argument, NULL, 'f'},
        {NULL, 0, NULL, 0}
    };
    char *arguments[] = {"probe", "--name=Ž猫", "--flag", "tail", NULL};
    int value, flags = 0;
    optind = 1;
    opterr = 0;
    while ((value = getopt_long(4, arguments, "n:f", options, NULL)) != -1)
    {
        if (value == 'n' && strcmp(optarg, "Ž猫") == 0) flags |= 1;
        else if (value == 'f') flags |= 2;
        else return -1;
    }
    return flags == 3 && optind == 3 && strcmp(arguments[optind], "tail") == 0 ? 73 : -2;
}

int dpg_getopt_index(void) { return optind; }
void dpg_getopt_set_index(int value) { optind = value; }

#ifdef DPG_NATIVE_MAIN
int main(void) { return dpg_getopt_probe() == 73 ? 0 : 1; }
#endif
