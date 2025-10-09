# Branch Protection Configuration Guide for Security Gate #235

**Purpose:** Configure GitHub branch protection rules to meet Security Gate #235 requirements  
**Date:** January 2025  
**Status:** Configuration Required

---

## 🎯 Overview

This guide provides step-by-step instructions for configuring branch protection rules on the `main` branch to comply with Security Gate #235 requirements.

**Repository:** buffbot88/TheRaProject  
**Branch to Protect:** `main`

---

## 📋 Required Configuration Steps

### Step 1: Navigate to Branch Protection Settings

1. Go to repository: https://github.com/buffbot88/TheRaProject
2. Click **Settings** (top navigation)
3. Click **Branches** (left sidebar)
4. Under "Branch protection rules", click **Add rule** or edit existing rule for `main`

---

### Step 2: Configure Branch Name Pattern

**Field:** Branch name pattern  
**Value:** `main`

---

### Step 3: Protect Matching Branches

Enable the following settings:

#### ✅ Require a pull request before merging

Check this box and configure:

- **Required number of approvals before merging:** `1`
  - Set to at least 1 reviewer approval
  - Recommended: 2 for production releases

- ✅ **Dismiss stale pull request approvals when new commits are pushed**
  - Ensures reviewers see latest changes

- ✅ **Require review from Code Owners**
  - Enforces reviews from designated code owners (`.github/CODEOWNERS`)
  - **Critical for Security Gate #235 compliance**

- [ ] **Restrict who can dismiss pull request reviews**
  - Optional: Can restrict to admins only

- ✅ **Allow specified actors to bypass required pull requests**
  - Do NOT enable this for security compliance
  - Only emergency access if absolutely necessary

#### ✅ Require status checks to pass before merging

Check this box and configure:

- ✅ **Require branches to be up to date before merging**
  - Ensures branch is current with main before merge

- **Status checks that are required:**
  - Select the following checks (from `.github/workflows/security-ci.yml`):
    - ✅ `build-and-test`
    - ✅ `codeql-analysis`
    - ✅ `secret-scanning`
    - ✅ `security-audit`
  
  *Note: These checks will appear after the first workflow run*

#### ✅ Require conversation resolution before merging

- Check this box
- Ensures all PR comments are resolved before merge

#### ✅ Require signed commits (Recommended)

- Check this box (optional but recommended)
- Requires commits to be GPG signed
- Enhances supply chain security

#### ✅ Require linear history (Recommended)

- Check this box (optional)
- Prevents merge commits, requires rebase or squash

#### ⚠️ Do not allow bypassing the above settings

- **Important:** Check this box
- Applies rules to administrators as well
- Critical for security compliance

#### ✅ Restrict who can push to matching branches

- Check this box
- Configure **Restrict pushes that create matching branches**
- Add authorized users/teams who can push directly (should be minimal)
- Typically: Release managers only for emergency hotfixes

#### ⚠️ Allow force pushes (DO NOT ENABLE)

- Leave this UNCHECKED
- Force pushes can overwrite history and break audit trail

#### ⚠️ Allow deletions (DO NOT ENABLE)

- Leave this UNCHECKED
- Branch should not be deletable

---

### Step 4: Configure Rules for Additional Branches (Optional)

Consider protecting these branches as well:

**Branch:** `develop`
- Similar rules but may allow more flexibility
- Require 1 approval
- Require status checks
- Allow merge commits

**Branch pattern:** `release/*`
- Strict protection for release branches
- Require 2 approvals
- Require all status checks
- Do not allow force pushes

**Branch pattern:** `hotfix/*`
- Similar to release branches
- Require 1 approval minimum
- Require status checks

---

## 🔒 Summary of Required Settings

### Minimum Requirements for Security Gate #235

| Setting | Required Value | Status |
|---------|---------------|--------|
| Pull request required | ✅ Enabled | Required |
| Required approvals | ≥ 1 | Required |
| Dismiss stale reviews | ✅ Enabled | Required |
| Require code owner review | ✅ Enabled | **Critical** |
| Status checks required | ✅ Enabled | Required |
| Required status checks | build-and-test, codeql-analysis, secret-scanning, security-audit | Required |
| Conversation resolution | ✅ Enabled | Required |
| Do not bypass settings | ✅ Enabled | **Critical** |
| Restrict pushes | ✅ Enabled | Required |
| Allow force pushes | ❌ Disabled | **Critical** |
| Allow deletions | ❌ Disabled | **Critical** |

---

## ✅ Verification Steps

After configuring branch protection:

1. **Test PR Workflow:**
   ```bash
   # Create test branch
   git checkout -b test/branch-protection
   
   # Make a trivial change
   echo "# Test" >> TEST.md
   git add TEST.md
   git commit -m "Test branch protection"
   git push origin test/branch-protection
   ```

2. **Create Pull Request:**
   - Open PR against `main`
   - Verify required checks appear
   - Verify code owner review is requested
   - Attempt to merge without approval (should fail)
   - Attempt to merge without passing checks (should fail)

3. **Verify Code Owner Reviews:**
   - Make changes to files in `.github/CODEOWNERS`
   - Verify appropriate reviewers are auto-requested
   - Example: Change `RaCore/Program.cs` should request review from @buffbot88

4. **Verify Status Checks:**
   - Ensure CI workflow completes
   - Verify all required checks show as "Required"
   - Check that PR cannot merge until checks pass

5. **Test Protection Rules:**
   - Try to push directly to `main` (should fail):
     ```bash
     git checkout main
     git pull
     echo "Direct push test" >> TEST.md
     git add TEST.md
     git commit -m "Direct push test"
     git push origin main  # Should be rejected
     ```

---

## 🚨 Troubleshooting

### Status checks not appearing

**Problem:** Required status checks not showing up in branch protection settings

**Solution:**
1. Ensure `.github/workflows/security-ci.yml` exists and is committed
2. Trigger workflow manually or wait for first push/PR
3. After first successful run, checks will appear in settings
4. Go back to branch protection and select them

### Code owner reviews not requested

**Problem:** Code owners not auto-requested for review

**Solution:**
1. Verify `.github/CODEOWNERS` file exists
2. Ensure file follows correct syntax
3. Verify code owner usernames are correct
4. Check that "Require review from Code Owners" is enabled

### Unable to merge after approval

**Problem:** Merge button disabled even with approval

**Possible causes:**
1. Status checks haven't passed
2. Conversations not resolved
3. Branch not up to date with main
4. Signed commits required but commit not signed

**Solution:** Check PR status checks section for specific requirement blocking merge

### Workflow fails in CI

**Problem:** Security CI workflow fails

**Solution:**
1. Check workflow logs in Actions tab
2. Common issues:
   - .NET SDK version mismatch
   - Missing dependencies
   - Build errors
3. Fix issues in feature branch before merging

---

## 📚 Additional Resources

- [GitHub Branch Protection Documentation](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/defining-the-mergeability-of-pull-requests/about-protected-branches)
- [CODEOWNERS Documentation](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Security Gate #235](./SECURITY_GATE_235.md)

---

## 🔐 Security Compliance

**This configuration is required for Security Gate #235 compliance.**

Once configured:
- [ ] Update Security Gate #235 document
- [ ] Mark "Branch protection configured" as complete
- [ ] Test configuration thoroughly
- [ ] Document any exceptions or deviations
- [ ] Obtain security team approval

---

**Configuration Guide Version:** 1.0  
**Last Updated:** January 2025  
**Maintained By:** DevOps & Security Team

---

**Copyright © 2025 AGP Studios, INC. All rights reserved.**
