/* Reduced from upstream test_main/build_tools and test_devtools/test_pipeline.
 * Native exits zero. The affected emitter writes &new in conditional arms.
 * Coordinator owns the shared compiler repair; never patch generated C#.
 */
typedef struct options { int value; } options;
static int use(const options *p) { return p->value; }
int main(void) {
    int choose = 1;
    int answer = choose ? use(&(options){.value=7}) : use(&(options){.value=9});
    return answer == 7 ? 0 : 1;
}
