# Security Policy

## Supported versions

Security fixes are provided for the latest released version of
`NzbDav.SharpCompress`. Please upgrade to the newest package before reporting an
issue that may already be resolved.

## Reporting a vulnerability

Do not open a public issue for a suspected vulnerability.

Use GitHub's
[private vulnerability reporting](https://github.com/nzbdav/sharpcompress/security/advisories/new)
to send a description, reproduction steps, affected versions, and any proposed
mitigation. You should receive an acknowledgement within seven days. We will
coordinate validation, remediation, and disclosure through the private
advisory.

If private reporting is unavailable, open a discussion that asks a maintainer
for a private contact channel without including vulnerability details.

## Security expectations

SharpCompress processes untrusted archive bytes. Treat extraction paths, entry
names, and compressed payloads as hostile input. Prefer streaming Reader APIs
for large inputs, and validate destination paths when extracting to disk.
