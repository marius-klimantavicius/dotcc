/* Harness control only. No upstream tests are modified. */
#include <check.h>
#include <stdio.h>
static int control_setups,control_teardowns,control_bodies;
static void control_setup(void){control_setups++;}
static void control_teardown(void){control_teardowns++;}
static void control_setup_fail(void){control_setups++;fail_unless(0,"intentional setup failure");}
START_TEST(control_pass){control_bodies++;fail_unless(1);}
END_TEST
START_TEST(control_fail){control_bodies++;fail_unless(0,"intentional assertion failure");}
END_TEST
START_TEST(control_unreached){control_bodies++;fail("setup failure must skip the test body");}
END_TEST
int check_contract_control(void)
{
  control_setups=control_teardowns=control_bodies=0;
  Suite *suite=suite_create("harness-control");
  TCase *pass=tcase_create("pass");tcase_add_checked_fixture(pass,control_setup,control_teardown);tcase_add_test(pass,control_pass);suite_add_tcase(suite,pass);
  TCase *failcase=tcase_create("fail");tcase_add_checked_fixture(failcase,control_setup,control_teardown);tcase_add_test(failcase,control_fail);suite_add_tcase(suite,failcase);
  TCase *setupfail=tcase_create("setup-fail");tcase_add_checked_fixture(setupfail,control_setup_fail,control_teardown);tcase_add_test(setupfail,control_unreached);suite_add_tcase(suite,setupfail);
  SRunner *runner=srunner_create(suite);srunner_set_fork_status(runner,CK_NOFORK);srunner_run_all(runner,CK_NORMAL);
  int failures=srunner_ntests_failed(runner);
  printf("CONTROL setups=%d teardowns=%d bodies=%d failures=%d\n",control_setups,control_teardowns,control_bodies,failures);
  srunner_free(runner);
  return failures?1:0;
}
#ifdef DOTCC_CHECK_CONTROL_MAIN
int main(void){return check_contract_control();}
#endif
