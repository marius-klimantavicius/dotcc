// Extra reporting only: DuckDB's own Catch/SQLLogicTest code checks results.
// In particular, its stock XML reporter does not report skippedTests.
#define CATCH_CONFIG_EXTERNAL_INTERFACES
#include "catch.hpp"

namespace {

void JsonString(std::ostream &out, const std::string &value) {
	out << '"';
	for (unsigned char ch : value) {
		if (ch == '"' || ch == '\\') {
			out << '\\' << char(ch);
		} else if (ch < 32) {
			const char hex[] = "0123456789abcdef";
			out << "\\u00" << hex[ch >> 4] << hex[ch & 15];
		} else {
			out << char(ch);
		}
	}
	out << '"';
}

void Totals(std::ostream &out, const Catch::Totals &totals) {
	out << ",\"assertionsPassed\":" << totals.assertions.passed
	    << ",\"assertionsFailed\":" << totals.assertions.failed
	    << ",\"assertionsExpectedFailure\":" << totals.assertions.failedButOk
	    << ",\"casesPassed\":" << totals.testCases.passed
	    << ",\"casesFailed\":" << totals.testCases.failed
	    << ",\"casesExpectedFailure\":" << totals.testCases.failedButOk
	    << ",\"skipped\":" << totals.skippedTests;
}

struct EdgJsonReporter : Catch::ConsoleReporter {
	explicit EdgJsonReporter(const Catch::ReporterConfig &config) : Catch::ConsoleReporter(config) {
	}

	static std::string getDescription() {
		return "Upstream console diagnostics plus strict EDG verification receipts";
	}

	void testCaseEnded(const Catch::TestCaseStats &stats) override {
		Catch::ConsoleReporter::testCaseEnded(stats);
		stream << "\nEDG_DUCKDB_RECEIPT {\"kind\":\"case\",\"name\":";
		JsonString(stream, stats.testInfo.name);
		Totals(stream, stats.totals);
		stream << ",\"aborting\":" << (stats.aborting ? "true" : "false") << "}\n";
		stream.flush();
	}

	void testRunEnded(const Catch::TestRunStats &stats) override {
		Catch::ConsoleReporter::testRunEnded(stats);
		stream << "\nEDG_DUCKDB_RECEIPT {\"kind\":\"run\"";
		Totals(stream, stats.totals);
		stream << ",\"aborting\":" << (stats.aborting ? "true" : "false") << "}\n";
		stream.flush();
	}
};

} // namespace

CATCH_REGISTER_REPORTER("edg-json", EdgJsonReporter)
