using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureReportingSystem.Shared.EmailTemplate
{
    /// <summary>
    /// Static class containing email HTML templates used throughout the application.
    /// All templates use placeholder tokens like {UserName}, {CODE}, {UserEmail} that are
    /// replaced at runtime with actual values.
    /// </summary>
    public static class EmailTemplates
    {
        /// <summary>
        /// Generates an OTP (One-Time Password) verification email template.
        /// </summary>
        /// <param name="UserName">The recipient's display name (e.g., "Mostafa Ahmed")</param>
        /// <param name="CODE">The 6-digit verification code to display in the email</param>
        /// <param name="UserEmail">The recipient's email address, displayed in the footer</param>
        /// <returns>A formatted HTML email string ready to be sent via SMTP</returns>
        public static string OtpEmailTemplate(string UserName, string CODE, string UserEmail)
        {
            string template = $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                  <meta charset="UTF-8" />
                  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                  <title>IRS Verification Email</title>
                </head>

                <body style="margin:0; padding:0; background-color:#000000; font-family:Arial, Helvetica, sans-serif;">

                  <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#000000; padding:40px 20px;">
                    <tr>
                      <td align="center">

                        <table width="600" cellpadding="0" cellspacing="0" border="0"
                          style="background-color:#2b2b2b; border:1px solid #444444; border-radius:16px; border-collapse:separate;">

                          <!-- Header -->
                          <tr>
                            <td align="center"
                              style="background-color:#111111; padding:36px 30px 32px 30px; border-radius:16px 16px 0 0;">

                              <div style="margin:0; color:#ff9800; font-size:48px; font-weight:bold; letter-spacing:4px;">
                                IRS
                              </div>

                              <p style="margin:10px 0 0 0; color:#8a8a8a; font-size:12px; letter-spacing:4px;">
                                INFRASTRUCTURE REPORTING SYSTEM
                              </p>
                            </td>
                          </tr>

                          <!-- Body -->
                          <tr>
                            <td style="padding:36px 44px; color:#ffffff;">

                              <p style="margin:0 0 16px 0; font-size:22px; font-weight:normal; color:#ffffff;">
                                Hi {UserName},
                              </p>

                              <p style="margin:0; color:#d0d0d0; font-size:15px; line-height:1.8;">
                                We received a request to verify your identity.
                                Use the code below to continue.
                                <br />
                                Do not share this code with anyone.
                              </p>

                              <!-- Verification Box -->
                              <table width="100%" cellpadding="0" cellspacing="0" border="0"
                                style="margin-top:28px; background-color:#1a1a1a; border:1px solid #3a3a3a; border-radius:12px; border-collapse:separate;">

                                <tr>
                                  <td align="center" style="padding:28px 20px 8px 20px;">
                                    <p style="margin:0; color:#9c9c9c; font-size:11px; letter-spacing:4px;">
                                      YOUR VERIFICATION CODE
                                    </p>
                                  </td>
                                </tr>

                                <tr>
                                  <td align="center" style="padding:12px 20px;">
                                    <div style="font-size:48px; color:#ff9800; letter-spacing:14px; font-weight:300;">
                                      {CODE}
                                    </div>
                                  </td>
                                </tr>

                                <tr>
                                  <td align="center" style="padding:6px 20px 24px 20px;">
                                    <p style="margin:0; color:#a0a0a0; font-size:13px;">
                                      Expires in 10 minutes
                                    </p>
                                  </td>
                                </tr>

                              </table>

                              <!-- Warning Box -->
                              <table width="100%" cellpadding="0" cellspacing="0" border="0"
                                style="margin-top:20px; background-color:#3d2e00; border-left:3px solid #ff9800; border-collapse:separate;">

                                <tr>
                                  <td style="padding:16px 20px; color:#ffcc66; font-size:14px; line-height:1.7;">
                                    If you did not request this code, please ignore this email.
                                    Your account remains secure.
                                  </td>
                                </tr>

                              </table>

                              <!-- Divider -->
                              <div style="height:1px; background-color:#3a3a3a; margin:32px 0;"></div>

                              <!-- Footer Text -->
                              <p style="margin:0; color:#9a9a9a; font-size:13px; line-height:1.8;">
                                This is an automated message from the Infrastructure Reporting System.
                                Please do not reply to this email.
                              </p>

                            </td>
                          </tr>

                          <!-- Bottom Footer -->
                          <tr>
                            <td align="center"
                              style="padding:24px 30px; border-top:1px solid #3a3a3a; background-color:#1e1e1e; border-radius:0 0 16px 16px;">

                              <p style="margin:0; color:#6a6a6a; font-size:12px; line-height:1.8;">
                                © 2026 Infrastructure Reporting System — EELU Graduation Project
                                <br />
                                This email was sent to {UserEmail}
                              </p>

                            </td>
                          </tr>

                        </table>

                      </td>
                    </tr>
                  </table>

                </body>
                </html>
                """;

            return template;
        }

        /// <summary>
        /// Generates an email template notifying the report submitter that a worker has taken an action on their report.
        /// </summary>
        /// <param name="UserName">The submitter's display name</param>
        /// <param name="UserEmail">The submitter's email address, displayed in the footer</param>
        /// <param name="ReportId">The report ID number</param>
        /// <param name="Description">The report description text</param>
        /// <param name="CategoryName">The category name of the report</param>
        /// <param name="WorkerName">The name of the worker who performed the action</param>
        /// <param name="StatusLabel">"Pending Confirmation" or "Blocked"</param>
        /// <param name="Reason">The reason for blocking (null for fixed)</param>
        /// <param name="Timestamp">Formatted date/time string of the action</param>
        /// <returns>A formatted HTML email string ready to be sent via SMTP</returns>
        public static string WorkerActionEmailTemplate(
            string UserName, string UserEmail, string ReportId,
            string Description, string CategoryName, string WorkerName,
            string StatusLabel, string? Reason, string Timestamp)
        {
            string template = $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                  <meta charset="UTF-8" />
                  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                  <title>Report Status Update</title>
                </head>

                <body style="margin:0; padding:0; background-color:#000000; font-family:Arial, Helvetica, sans-serif;">

                  <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#000000; padding:40px 20px;">
                    <tr>
                      <td align="center">

                        <table width="600" cellpadding="0" cellspacing="0" border="0"
                          style="background-color:#2b2b2b; border:1px solid #444444; border-radius:16px; border-collapse:separate;">

                          <!-- Header -->
                          <tr>
                            <td align="center"
                              style="background-color:#111111; padding:36px 30px 32px 30px; border-radius:16px 16px 0 0;">

                              <div style="margin:0; color:#ff9800; font-size:48px; font-weight:bold; letter-spacing:4px;">
                                IRS
                              </div>

                              <p style="margin:10px 0 0 0; color:#8a8a8a; font-size:12px; letter-spacing:4px;">
                                INFRASTRUCTURE REPORTING SYSTEM
                              </p>
                            </td>
                          </tr>

                          <!-- Body -->
                          <tr>
                            <td style="padding:36px 44px; color:#ffffff;">

                              <p style="margin:0 0 16px 0; font-size:22px; font-weight:normal; color:#ffffff;">
                                Hi {UserName},
                              </p>

                              <p style="margin:0; color:#d0d0d0; font-size:15px; line-height:1.8;">
                                A worker has updated the status of your report.
                                Details are shown below.
                              </p>

                              <!-- Details Box -->
                              <table width="100%" cellpadding="0" cellspacing="0" border="0"
                                style="margin-top:28px; background-color:#1a1a1a; border:1px solid #3a3a3a; border-radius:12px; border-collapse:separate;">

                                <tr>
                                  <td align="center" style="padding:20px 20px 16px 20px;">
                                    <p style="margin:0; color:#9c9c9c; font-size:11px; letter-spacing:4px;">
                                      REPORT STATUS UPDATE
                                    </p>
                                  </td>
                                </tr>

                                <tr>
                                  <td style="padding:4px 24px 20px 24px;">
                                    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="font-size:14px; color:#d0d0d0; line-height:2;">

                                      <tr>
                                        <td style="padding:4px 0; color:#9c9c9c; width:120px; vertical-align:top;">Report ID</td>
                                        <td style="padding:4px 0; color:#ffffff;">#{ReportId}</td>
                                      </tr>

                                      <tr>
                                        <td style="padding:4px 0; color:#9c9c9c; width:120px; vertical-align:top;">Description</td>
                                        <td style="padding:4px 0; color:#ffffff;">{Description}</td>
                                      </tr>

                                      <tr>
                                        <td style="padding:4px 0; color:#9c9c9c; width:120px; vertical-align:top;">Category</td>
                                        <td style="padding:4px 0; color:#ffffff;">{CategoryName}</td>
                                      </tr>

                                      <tr>
                                        <td style="padding:4px 0; color:#9c9c9c; width:120px; vertical-align:top;">Worker</td>
                                        <td style="padding:4px 0; color:#ffffff;">{WorkerName}</td>
                                      </tr>

                                      <tr>
                                        <td style="padding:4px 0; color:#9c9c9c; width:120px; vertical-align:top;">Status</td>
                                        <td style="padding:4px 0; color:#ff9800; font-weight:bold;">{StatusLabel}</td>
                                      </tr>

                                      <tr>
                                        <td style="padding:4px 0; color:#9c9c9c; width:120px; vertical-align:top;">Date & Time</td>
                                        <td style="padding:4px 0; color:#ffffff;">{Timestamp}</td>
                                      </tr>
            """;

            if (!string.IsNullOrEmpty(Reason))
            {
                template += $"""
                                      <tr>
                                        <td style="padding:4px 0; color:#9c9c9c; width:120px; vertical-align:top;">Reason</td>
                                        <td style="padding:4px 0; color:#ffcc66;">{Reason}</td>
                                      </tr>
                """;
            }

            template += $"""
                                    </table>
                                  </td>
                                </tr>

                              </table>

                              <!-- Warning Box -->
                              <table width="100%" cellpadding="0" cellspacing="0" border="0"
                                style="margin-top:20px; background-color:#3d2e00; border-left:3px solid #ff9800; border-collapse:separate;">

                                <tr>
                                  <td style="padding:16px 20px; color:#ffcc66; font-size:14px; line-height:1.7;">
                                    If you believe this is an error, please contact your local authority.
                                  </td>
                                </tr>

                              </table>

                              <!-- Divider -->
                              <div style="height:1px; background-color:#3a3a3a; margin:32px 0;"></div>

                              <!-- Footer Text -->
                              <p style="margin:0; color:#9a9a9a; font-size:13px; line-height:1.8;">
                                This is an automated message from the Infrastructure Reporting System.
                                Please do not reply to this email.
                              </p>

                            </td>
                          </tr>

                          <!-- Bottom Footer -->
                          <tr>
                            <td align="center"
                              style="padding:24px 30px; border-top:1px solid #3a3a3a; background-color:#1e1e1e; border-radius:0 0 16px 16px;">

                              <p style="margin:0; color:#6a6a6a; font-size:12px; line-height:1.8;">
                                © 2026 Infrastructure Reporting System — EELU Graduation Project
                                <br />
                                This email was sent to {UserEmail}
                              </p>

                            </td>
                          </tr>

                        </table>

                      </td>
                    </tr>
                  </table>

                </body>
                </html>
                """;

            return template;
        }
    }
}