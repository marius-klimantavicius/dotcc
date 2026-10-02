//type:fp
//remark:[4.12] Glvalue conditional operator and cv-qualifier differences
// 6/1/16   [EDGcpfe/17007,EDGcpfe/17057]
//
// Glvalue conditional operator and cv-qualifier differences
//
// The resolution of Core issue 587 allows a conditional operator whose second and
// third operands are glvalues to have those operands have identical types except
// for cv-qualification and still produce a glvalue result.  The front end now
// implements that rule (except in Microsoft mode and in some GNU modes).
int x = 1;
int const y = 2;
int const *p = &(1 ? x : y);  // Previously an error.  Now okay.
