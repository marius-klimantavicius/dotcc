extern int imported;
int tentative;
extern int redeclared;
int redeclared;
static int localStatic;
extern int initialized = 7;

int read_imported(void)
{
    return imported + tentative + redeclared + localStatic + initialized;
}
