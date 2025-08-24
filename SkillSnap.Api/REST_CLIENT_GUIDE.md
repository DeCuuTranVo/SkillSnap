# SkillSnap API Authentication Testing Guide

This guide explains how to use the `SkillSnap.Api.http` file to test the authentication features of the API.

## Prerequisites

1. **Start the API server:**
   ```bash
   cd /Users/tranvq/code_repos/SkillSnap/SkillSnap.Api
   dotnet run
   ```

2. **Ensure the API is running on:** `http://localhost:5217`

## How to Use the REST Client File

### Using VS Code REST Client Extension

1. **Install the REST Client extension** in VS Code (if not already installed)
2. **Open** `SkillSnap.Api.http` in VS Code
3. **Click "Send Request"** above each HTTP request block

### Testing Workflow

#### 1. **Basic Authentication Tests**

**Register a New User:**
- Click "Send Request" on the register endpoint (line 14)
- Should return `200 OK` with JWT token

**Login with Valid Credentials:**
- Click "Send Request" on the login endpoint (line 93)
- Should return `200 OK` with JWT token

#### 2. **Error Handling Tests**

**Password Mismatch:**
- Test registration with different passwords (line 40)
- Should return `400 Bad Request`

**Weak Password:**
- Test with short password (line 53)
- Should return `400 Bad Request`

**Duplicate Username/Email:**
- Test with existing credentials (lines 66, 79)
- Should return `400 Bad Request`

**Invalid Login:**
- Test with wrong username/password (lines 104, 115)
- Should return `401 Unauthorized`

#### 3. **JWT Token Testing**

**Copy JWT Token:**
1. After successful login, copy the `token` value from the response
2. Replace `YOUR_JWT_TOKEN_HERE` on line 126 with the actual token

**Test Protected Endpoints:**
- Without token (line 133) → Should return `401 Unauthorized`
- With valid token (line 140) → Should return `200 OK`

#### 4. **Complete Workflow Test**

Follow the "SAMPLE WORKFLOW" section (starting line 195):
1. Register a new user
2. Login with those credentials
3. Copy the token from step 2
4. Replace `TOKEN_FROM_STEP_2` with the actual token
5. Make authenticated API calls

## Expected Responses

### Successful Registration/Login:
```json
{
  "success": true,
  "message": "Registration successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userName": "testuser123",
  "email": "testuser123@example.com",
  "portfolioUserId": 0
}
```

### Authentication Error:
```json
{
  "success": false,
  "message": "Username already exists"
}
```

### Unauthorized Access:
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

## JWT Token Structure

The JWT token contains these claims:
- `sub`: User ID
- `name`: Username
- `email`: User email
- `PortfolioUserId`: Portfolio user ID
- `role`: User roles (if any)
- `exp`: Expiration time (60 minutes from issue)

## Tips for Testing

1. **Use different usernames/emails** for each test to avoid conflicts
2. **Copy tokens carefully** - they're long strings
3. **Test token expiration** by waiting 60+ minutes
4. **Check Swagger UI** at `http://localhost:5217/swagger` for interactive testing
5. **Monitor API logs** in the terminal for detailed error information

## Troubleshooting

**401 Unauthorized:**
- Check if token is correctly copied
- Ensure token hasn't expired (60 minutes)
- Verify `Bearer ` prefix is included

**400 Bad Request:**
- Check request body JSON format
- Verify all required fields are provided
- Ensure password meets requirements (8+ characters)

**500 Internal Server Error:**
- Check API server logs
- Ensure database is properly configured
- Verify JWT settings in appsettings.json
