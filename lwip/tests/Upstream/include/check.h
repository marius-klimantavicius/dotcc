#ifndef DOTCC_LWIP_CHECK_H
#define DOTCC_LWIP_CHECK_H
/* Check 0.12 registration ABI supported by upstream lwip_check.h.
 * Assertions and fixtures are executed; unsupported APIs fail compilation. */
#define CHECK_MAJOR_VERSION 0
#define CHECK_MINOR_VERSION 12
#define CHECK_MICRO_VERSION 0
typedef void Suite;
typedef void TCase;
typedef void SRunner;
typedef void (*TFun)(int);
typedef void (*SFun)(void);
#define CK_NORMAL 1
#define CK_NOFORK 0
#define CK_FORK 1
#define START_TEST(name) static void name(int _i)
#define END_TEST
Suite *suite_create(const char *name);
TCase *tcase_create(const char *name);
void suite_add_tcase(Suite *suite,TCase *test_case);
void tcase_add_checked_fixture(TCase *test_case,SFun setup,SFun teardown);
void _tcase_add_test(TCase *test_case,TFun test,const char *name,int signal,int allowed_exit,int start,int end);
SRunner *srunner_create(Suite *suite);
void srunner_add_suite(SRunner *runner,Suite *suite);
void srunner_set_xml(SRunner *runner,const char *path);
void srunner_set_fork_status(SRunner *runner,int status);
void srunner_run_all(SRunner *runner,int verbosity);
int srunner_ntests_failed(SRunner *runner);
void srunner_free(SRunner *runner);
void dotcc_check_fail(const char *expression,const char *file,int line,const char *format,...);
#define fail_unless(condition,...) do { if (!(condition)) dotcc_check_fail(#condition,__FILE__,__LINE__,"" __VA_ARGS__); } while(0)
#define fail_if(condition,...) do { if ((condition)) dotcc_check_fail("!(" #condition ")",__FILE__,__LINE__,"" __VA_ARGS__); } while(0)
#define fail(...) dotcc_check_fail("fail()",__FILE__,__LINE__,"" __VA_ARGS__)
#define tcase_add_test(test_case,test) _tcase_add_test(test_case,test,#test,0,0,0,1)
#endif
