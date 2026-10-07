// Tests of the reporting mechanism, not DuckDB engine acceptance tests.
#define CATCH_CONFIG_MAIN
#include "catch.hpp"

TEST_CASE("control pass with \"quotes\" and backslash \\") {
	REQUIRE(2 + 2 == 4);
	SECTION("one") {
		REQUIRE(true);
	}
	SECTION("two") {
		REQUIRE(true);
	}
}

TEST_CASE("control skip") {
	SKIP_TEST("deliberate control skip");
}

TEST_CASE("control failure") {
	REQUIRE(false);
}

TEST_CASE("control expected failure", "[!mayfail]") {
	REQUIRE(false);
}

TEST_CASE("control empty") {
}
