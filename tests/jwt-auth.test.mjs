import { createHmac, timingSafeEqual } from 'node:crypto';
import { readFileSync } from 'node:fs';
import assert from 'node:assert/strict';

const appSettings = JSON.parse(readFileSync(new URL('../backend/LeaveManagement.API/appsettings.json', import.meta.url), 'utf8'));
const program = readFileSync(new URL('../backend/LeaveManagement.API/Program.cs', import.meta.url), 'utf8');
const tokenService = readFileSync(new URL('../backend/LeaveManagement.API/Services/JwtTokenService.cs', import.meta.url), 'utf8');

const jwtSettings = appSettings.JwtSettings;
assert.ok(jwtSettings, 'JwtSettings section must exist.');
assert.ok(Buffer.byteLength(jwtSettings.Key, 'utf8') >= 32, 'JwtSettings.Key must be at least 32 bytes for HS256.');
assert.match(program, /options\.MapInboundClaims\s*=\s*false/, 'JwtBearer must not remap JWT claim names.');
assert.match(program, /NameClaimType\s*=\s*"nameid"/, 'JwtBearer must use the raw nameid claim as the name identifier.');
assert.match(program, /RoleClaimType\s*=\s*"role"/, 'JwtBearer must use the raw role claim for authorization.');
assert.match(tokenService, /new Claim\("nameid", employeeId\)/, 'Tokens must include a raw nameid claim.');
assert.match(tokenService, /new Claim\("uid", employeeId\)/, 'Tokens must include a uid claim.');
assert.match(tokenService, /new Claim\("role", employee\.Role\)/, 'Tokens must include a raw role claim.');

function base64UrlEncode(value) {
  return Buffer.from(value).toString('base64url');
}

function sign(input, key = jwtSettings.Key) {
  return createHmac('sha256', key).update(input).digest('base64url');
}

function createToken(employee, overrides = {}) {
  const now = Math.floor(Date.now() / 1000);
  const payload = {
    sub: employee.email,
    jti: '00000000-0000-0000-0000-000000000001',
    email: employee.email,
    iat: now,
    uid: String(employee.employeeID),
    nameid: String(employee.employeeID),
    unique_name: employee.email,
    given_name: employee.firstName,
    family_name: employee.lastName,
    role: employee.role,
    iss: jwtSettings.Issuer,
    aud: jwtSettings.Audience,
    exp: now + (jwtSettings.DurationInMinutes * 60),
    ...overrides
  };

  const header = { alg: 'HS256', typ: 'JWT' };
  const unsignedToken = `${base64UrlEncode(JSON.stringify(header))}.${base64UrlEncode(JSON.stringify(payload))}`;
  return `${unsignedToken}.${sign(unsignedToken)}`;
}

function decodePart(part) {
  return JSON.parse(Buffer.from(part, 'base64url').toString('utf8'));
}

function validateToken(token, expectedRole) {
  const parts = token.split('.');
  assert.equal(parts.length, 3, 'JWT must have header, payload, and signature.');

  const [encodedHeader, encodedPayload, signature] = parts;
  const header = decodePart(encodedHeader);
  const payload = decodePart(encodedPayload);
  const expectedSignature = sign(`${encodedHeader}.${encodedPayload}`);

  assert.equal(header.alg, 'HS256', 'JWT must use HS256.');
  assert.equal(header.typ, 'JWT', 'JWT type must be JWT.');
  assert.ok(timingSafeEqual(Buffer.from(signature), Buffer.from(expectedSignature)), 'JWT signature must validate.');
  assert.equal(payload.iss, jwtSettings.Issuer, 'JWT issuer must match configuration.');
  assert.equal(payload.aud, jwtSettings.Audience, 'JWT audience must match configuration.');
  assert.ok(payload.exp > Math.floor(Date.now() / 1000), 'JWT must not be expired.');
  assert.equal(payload.role, expectedRole, 'JWT role must match the signed-in user role.');
  assert.equal(payload.nameid, payload.uid, 'JWT nameid and uid must identify the same employee.');
  return payload;
}

const employeeToken = createToken({ employeeID: 42, email: 'employee@leave.local', firstName: 'Evan', lastName: 'Employee', role: 'Employee' });
const employeePayload = validateToken(employeeToken, 'Employee');
assert.equal(employeePayload.nameid, '42', 'Employee token should carry the employee ID in nameid.');
assert.equal(employeePayload.uid, '42', 'Employee token should carry the employee ID in uid.');

const managerToken = createToken({ employeeID: 7, email: 'manager@leave.local', firstName: 'Mia', lastName: 'Manager', role: 'Manager' });
const managerPayload = validateToken(managerToken, 'Manager');
assert.equal(managerPayload.role, 'Manager', 'Manager token should authorize manager-only endpoints.');
assert.notEqual(managerPayload.role, 'Employee', 'Manager token should not authorize employee-only endpoints.');

const expiredToken = createToken(
  { employeeID: 43, email: 'expired@leave.local', firstName: 'Expired', lastName: 'Employee', role: 'Employee' },
  { exp: Math.floor(Date.now() / 1000) - 1 }
);
assert.throws(() => validateToken(expiredToken, 'Employee'), /must not be expired/, 'Expired tokens must fail validation.');

const wrongAudienceToken = createToken(
  { employeeID: 44, email: 'wrong-audience@leave.local', firstName: 'Wrong', lastName: 'Audience', role: 'Employee' },
  { aud: 'WrongAudience' }
);
assert.throws(() => validateToken(wrongAudienceToken, 'Employee'), /audience must match/, 'Tokens with a wrong audience must fail validation.');

const tamperedParts = employeeToken.split('.');
const tamperedPayload = decodePart(tamperedParts[1]);
tamperedPayload.role = 'Manager';
tamperedParts[1] = base64UrlEncode(JSON.stringify(tamperedPayload));
const tamperedToken = tamperedParts.join('.');
assert.throws(() => validateToken(tamperedToken, 'Manager'), /signature must validate/, 'Tampered tokens must fail signature validation.');

console.log('JWT authentication tests passed.');
