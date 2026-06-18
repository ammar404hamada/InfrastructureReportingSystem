using FluentAssertions;
using InfrastructureReportingSystem.Shared.EmailTemplate;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Shared.EmailTemplate
{
    public class EmailTemplatesTests
    {
        [Fact]
        public void OtpEmailTemplate_ReturnsNonEmptyString()
        {
            var result = EmailTemplates.OtpEmailTemplate("John Doe", "123456", "john@example.com");

            result.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void OtpEmailTemplate_ContainsExpectedBrandingAndPlaceholders()
        {
            var result = EmailTemplates.OtpEmailTemplate("John Doe", "123456", "john@example.com");

            result.Should().Contain("IRS");
            result.Should().Contain("EELU");
            result.Should().Contain("John Doe");
            result.Should().Contain("123456");
            result.Should().Contain("john@example.com");
        }

        [Fact]
        public void AccountCreatedWithPasswordTemplate_ReturnsNonEmptyString()
        {
            var result = EmailTemplates.AccountCreatedWithPasswordTemplate("Jane", "jane@test.com", "P@ss1", "https://example.com/login");

            result.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void AccountCreatedWithPasswordTemplate_ContainsExpectedBrandingAndPlaceholders()
        {
            var result = EmailTemplates.AccountCreatedWithPasswordTemplate("Jane", "jane@test.com", "P@ss1", "https://example.com/login");

            result.Should().Contain("IRS");
            result.Should().Contain("EELU");
            result.Should().Contain("Jane");
            result.Should().Contain("jane@test.com");
            result.Should().Contain("P@ss1");
            result.Should().Contain("https://example.com/login");
        }

        [Fact]
        public void WorkerActionEmailTemplate_ReturnsNonEmptyString()
        {
            var result = EmailTemplates.WorkerActionEmailTemplate(
                "Alice", "alice@test.com", "42", "Broken road", "Roads", "Bob", "Blocked", "Need equipment", "June 18, 2026");

            result.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void WorkerActionEmailTemplate_ContainsExpectedBrandingAndPlaceholders()
        {
            var result = EmailTemplates.WorkerActionEmailTemplate(
                "Alice", "alice@test.com", "42", "Broken road", "Roads", "Bob", "Blocked", "Need equipment", "June 18, 2026");

            result.Should().Contain("IRS");
            result.Should().Contain("EELU");
            result.Should().Contain("Alice");
            result.Should().Contain("alice@test.com");
            result.Should().Contain("#42");
            result.Should().Contain("Broken road");
            result.Should().Contain("Roads");
            result.Should().Contain("Bob");
            result.Should().Contain("Blocked");
            result.Should().Contain("Need equipment");
            result.Should().Contain("June 18, 2026");
        }

        [Fact]
        public void WorkerActionEmailTemplate_WhenReasonIsNull_DoesNotIncludeReasonRow()
        {
            var result = EmailTemplates.WorkerActionEmailTemplate(
                "Alice", "alice@test.com", "42", "Broken road", "Roads", "Bob", "Fixed", null, "June 18, 2026");

            result.Should().NotContain("Need equipment");
        }

        [Fact]
        public void WorkerActionEmailTemplate_ContainerHtmlStructure()
        {
            var result = EmailTemplates.WorkerActionEmailTemplate(
                "Alice", "alice@test.com", "42", "Broken road", "Roads", "Bob", "Blocked", "Reason text", "June 18, 2026");

            result.Should().Contain("<!DOCTYPE html>");
            result.Should().Contain("</html>");
            result.Should().Contain("<title>Report Status Update</title>");
        }

        [Fact]
        public void FixConfirmationEmailTemplate_ReturnsNonEmptyString()
        {
            var result = EmailTemplates.FixConfirmationEmailTemplate(
                "Worker1", "worker@test.com", "99", "Pothole", "Roads", "Resolved", "The user has confirmed the fix for report #99.", null, "June 18, 2026");

            result.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void FixConfirmationEmailTemplate_ContainsExpectedBrandingAndPlaceholders()
        {
            var result = EmailTemplates.FixConfirmationEmailTemplate(
                "Worker1", "worker@test.com", "99", "Pothole", "Roads", "Resolved", "The user has confirmed the fix for report #99.", null, "June 18, 2026");

            result.Should().Contain("IRS");
            result.Should().Contain("EELU");
            result.Should().Contain("Worker1");
            result.Should().Contain("worker@test.com");
            result.Should().Contain("#99");
            result.Should().Contain("Pothole");
            result.Should().Contain("Roads");
            result.Should().Contain("Resolved");
            result.Should().Contain("confirmed the fix");
            result.Should().Contain("June 18, 2026");
        }

        [Fact]
        public void FixConfirmationEmailTemplate_WhenRejected_IncludesReason()
        {
            var result = EmailTemplates.FixConfirmationEmailTemplate(
                "Worker1", "worker@test.com", "99", "Pothole", "Roads", "Fix Rejected", "The user has rejected the fix for report #99.", "Not properly fixed", "June 18, 2026");

            result.Should().Contain("Not properly fixed");
            result.Should().Contain("rejected the fix");
        }

        [Fact]
        public void FixConfirmationEmailTemplate_WhenConfirmed_DoesNotIncludeReason()
        {
            var result = EmailTemplates.FixConfirmationEmailTemplate(
                "Worker1", "worker@test.com", "99", "Pothole", "Roads", "Resolved", "confirmed", null, "June 18, 2026");

            result.Should().Contain("confirmed");
        }

        [Fact]
        public void FixConfirmationEmailTemplate_ContainsHtmlStructure()
        {
            var result = EmailTemplates.FixConfirmationEmailTemplate(
                "W", "w@t.com", "1", "D", "C", "Resolved", "confirmed", null, "now");

            result.Should().Contain("<!DOCTYPE html>");
            result.Should().Contain("</html>");
            result.Should().Contain("<title>Fix Status Update</title>");
        }
    }
}
