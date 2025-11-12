# Security Policy

## Supported Versions

We release patches for security vulnerabilities. The following versions are currently supported:

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

We take the security of CLI Dashboard seriously. If you have discovered a security vulnerability, please report it to us privately.

### How to Report

**Please do NOT report security vulnerabilities through public GitHub issues.**

Instead, please report them via one of the following methods:

1. **GitHub Security Advisories** (Preferred)
   - Go to the [Security tab](https://github.com/Nicklas185105/CliDashboard/security/advisories) of the repository
   - Click "Report a vulnerability"
   - Fill out the form with details

2. **Email**
   - Send an email to: [your-email@example.com]
   - Use the subject line: "CLI Dashboard Security Vulnerability"

### What to Include

Please include as much of the following information as possible:

- **Type of vulnerability** (e.g., code injection, privilege escalation, etc.)
- **Full paths of source file(s)** related to the vulnerability
- **Location of the affected source code** (tag/branch/commit or direct URL)
- **Step-by-step instructions** to reproduce the issue
- **Proof-of-concept or exploit code** (if available)
- **Impact of the issue**, including how an attacker might exploit it
- **Your assessment** of the severity (Critical, High, Medium, Low)

### What to Expect

After you submit a vulnerability report:

1. **Acknowledgment**: We will acknowledge receipt of your report within **48 hours**
2. **Assessment**: We will assess the vulnerability and its impact
3. **Communication**: We will keep you informed of our progress
4. **Fix**: We will work on a fix and release a security update
5. **Disclosure**: After the fix is released, we will publicly acknowledge your responsible disclosure (if you wish)

### Response Timeline

- **Initial Response**: Within 48 hours
- **Status Update**: Within 7 days
- **Fix Target**: Within 30 days for high/critical issues

## Security Best Practices for Users

### Plugin Security

CLI Dashboard uses a plugin system that executes `.csx` scripts. To stay secure:

1. **Only install plugins from trusted sources**
   - Review plugin code before installation
   - Check the plugin author and community reviews

2. **Be cautious with plugin permissions**
   - Plugins run with your user permissions
   - Avoid plugins that request unnecessary system access

3. **Keep plugins updated**
   - Update plugins regularly to get security fixes

4. **Use plugin sync carefully**
   - Only sync plugins to trusted cloud storage
   - Be aware that synced plugins can be modified by anyone with access to the sync folder

### Configuration Security

1. **Protect sensitive data**
   - Never commit credentials to plugin configuration files
   - Use environment variables for secrets
   - Use Windows Credential Manager or similar for API keys

2. **File permissions**
   - Ensure `%APPDATA%\cli-dashboard` has appropriate permissions
   - Restrict access to configuration files containing sensitive data

3. **Script execution**
   - Be aware that PowerShell scripts (`.ps1`) execute with your user privileges
   - Review scripts before adding them to CLI Dashboard

### General Security

1. **Keep CLI Dashboard updated**
   - Watch for security updates in releases
   - Apply security patches promptly

2. **Review scheduled tasks**
   - Regularly audit scheduled plugins and scripts
   - Remove unnecessary background jobs

3. **Monitor logs**
   - Check logs in `%APPDATA%\cli-dashboard\logs\` for suspicious activity

## Known Security Considerations

### Script Execution Model

CLI Dashboard executes C# scripts (`.csx`) and PowerShell scripts (`.ps1`) with your user permissions. This design:

- **By Design**: Provides flexibility and power for automation
- **Security Implication**: Malicious scripts can perform any action you can
- **Mitigation**: Only use plugins from trusted sources and review code before installation

### Plugin Sync

When using plugin sync:

- **By Design**: Allows sharing plugins across machines via cloud storage
- **Security Implication**: Anyone with access to the sync folder can modify plugins
- **Mitigation**: Only use sync folders you control and trust

## Security Updates

Security updates will be released as patch versions (e.g., 1.0.1, 1.0.2). We will:

- Publish a security advisory on GitHub
- Update CHANGELOG.md with security fixes
- Notify users through release notes

Subscribe to releases on GitHub to be notified of security updates.

## Disclosure Policy

We follow responsible disclosure:

1. Security issues are fixed privately
2. Fixes are released as quickly as possible
3. Public disclosure happens after a fix is available
4. Credit is given to security researchers (with permission)

## Questions?

If you have questions about this security policy, please open a GitHub Discussion or contact the maintainers.

---

**Thank you for helping keep CLI Dashboard and its users safe!**
