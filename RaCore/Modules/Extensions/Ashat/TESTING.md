# Ashat Test Scenarios

This document outlines test scenarios for validating Ashat's functionality.

## Test Setup

1. Build the RaCore project:
   ```bash
   cd /home/runner/work/TheRaProject/TheRaProject
   dotnet build RaCore/RaCore.csproj
   ```

2. Run RaCore:
   ```bash
   cd RaCore
   dotnet run
   ```

## Test Scenarios

### Scenario 1: Basic Help Command

**Test:** Verify Ashat responds to help command

**Input:**
```
ashat help
```

**Expected Output:**
- Welcome message from Ashat
- Command reference
- Feature list
- Example usage

**Status:** ✅ Pass (if output shows comprehensive help)

---

### Scenario 2: Module Discovery

**Test:** List all available modules

**Input:**
```
ashat modules
```

**Expected Output:**
- Total module count
- Modules grouped by category
- Module names and descriptions

**Validation:**
- Should show categories: core, extensions, etc.
- Should list 40+ modules
- Each module has a description

**Status:** ⏳ Pending

---

### Scenario 3: Module Information

**Test:** Get detailed info about a specific module

**Input:**
```
ashat module info AICodeGen
```

**Expected Output:**
- Module name: AICodeGen
- Category: extensions
- Type: AICodeGenModule
- Description
- Capabilities list

**Validation:**
- All fields populated
- Description is meaningful
- Capabilities are relevant

**Status:** ⏳ Pending

---

### Scenario 4: Session Start

**Test:** Start a new coding session

**Input:**
```
ashat start session test_user Create a weather forecast module
```

**Expected Output:**
- Welcome message
- Understanding of the goal
- Relevant modules identified
- Question about next steps

**Validation:**
- Session created successfully
- Relevant modules listed (e.g., AICodeGen, ModuleSpawner)
- Phase is "Planning"

**Status:** ⏳ Pending

---

### Scenario 5: Session Continue

**Test:** Continue an active session

**Prerequisites:** Active session from Scenario 4

**Input:**
```
ashat continue test_user It should fetch data from OpenWeather API
```

**Expected Output:**
- Acknowledgment of input
- Action plan creation
- Steps listed
- Request for approval

**Validation:**
- Phase changes to "AwaitingApproval"
- Action plan has multiple steps
- Each step has required modules

**Status:** ⏳ Pending

---

### Scenario 6: Session Approval

**Test:** Approve an action plan

**Prerequisites:** Session in "AwaitingApproval" phase

**Input:**
```
ashat approve test_user
```

**Expected Output:**
- Approval confirmation
- Execution begins message
- Implementation notes

**Validation:**
- Phase changes to "Executing"
- ApprovedAt timestamp set
- Clear next steps provided

**Status:** ⏳ Pending

---

### Scenario 7: Session Rejection

**Test:** Reject an action plan and revise

**Prerequisites:** New session in "AwaitingApproval" phase

**Input:**
```
ashat reject test_user Need more focus on error handling
```

**Expected Output:**
- Rejection acknowledged
- Request for guidance
- Phase returns to Planning

**Validation:**
- Phase changes back to "Planning"
- Action plan cleared
- Ready for new input

**Status:** ⏳ Pending

---

### Scenario 8: Session End

**Test:** End an active session

**Prerequisites:** Any active session

**Input:**
```
ashat end session test_user
```

**Expected Output:**
- Session summary
- Goal recap
- Duration
- Interaction count
- Thank you message

**Validation:**
- Session removed from active sessions
- Status changes to "Completed"
- CompletedAt timestamp set

**Status:** ⏳ Pending

---

### Scenario 9: Ask Question

**Test:** Ask Ashat a question

**Input:**
```
ashat ask What modules handle game logic?
```

**Expected Output:**
- Contextual response
- Mention of relevant modules (e.g., LegendaryGameEngine, GameServer)
- Helpful guidance

**Validation:**
- Response is relevant to question
- Mentions appropriate modules
- Friendly and helpful tone

**Status:** ⏳ Pending

---

### Scenario 10: Status Check

**Test:** Check Ashat's current status

**Input:**
```
ashat status
```

**Expected Output:**
- Active session count
- Known module count
- AI module connection status
- Chat module connection status
- List of active sessions (if any)

**Validation:**
- All status fields present
- Module count > 0
- Connection statuses accurate

**Status:** ⏳ Pending

---

### Scenario 11: Invalid Command

**Test:** Handle unknown command gracefully

**Input:**
```
ashat invalid_command
```

**Expected Output:**
- Error message
- Suggestion to use 'help'

**Validation:**
- No crash
- Helpful error message
- Points user to help

**Status:** ⏳ Pending

---

### Scenario 12: Concurrent Sessions

**Test:** Multiple users with active sessions

**Input:**
```
ashat start session user1 Create module A
ashat start session user2 Create module B
ashat status
```

**Expected Output:**
- Both sessions active
- Status shows 2 active sessions
- Each session maintains separate state

**Validation:**
- Sessions are independent
- No cross-contamination
- Status correctly shows both

**Status:** ⏳ Pending

---

### Scenario 13: Session Without Approval

**Test:** Try to continue after plan without approving

**Prerequisites:** Session in "AwaitingApproval" phase

**Input:**
```
ashat continue test_user More details here
```

**Expected Output:**
- Message about awaiting approval
- Instructions to approve or reject

**Validation:**
- Phase remains "AwaitingApproval"
- Clear guidance provided

**Status:** ⏳ Pending

---

### Scenario 14: Module Not Found

**Test:** Request info for non-existent module

**Input:**
```
ashat module info NonExistentModule
```

**Expected Output:**
- Module not found message
- Suggestion to use 'ashat modules'

**Validation:**
- Graceful error handling
- Helpful suggestion
- No crash

**Status:** ⏳ Pending

---

### Scenario 15: Integration with Speech Module

**Test:** Verify AI-powered responses (if Speech module available)

**Input:**
```
ashat ask How do I optimize database queries?
```

**Expected Output:**
- Natural language response
- Context-aware answer
- Technical guidance

**Validation:**
- Uses Speech module if available
- Falls back gracefully if not
- Response quality is good

**Status:** ⏳ Pending

---

## Test Results Summary

| Scenario | Status | Notes |
|----------|--------|-------|
| 1. Basic Help | ✅ Pass | Module loads and responds |
| 2. Module Discovery | ⏳ Pending | Need to run |
| 3. Module Info | ⏳ Pending | Need to run |
| 4. Session Start | ⏳ Pending | Need to run |
| 5. Session Continue | ⏳ Pending | Need to run |
| 6. Session Approval | ⏳ Pending | Need to run |
| 7. Session Rejection | ⏳ Pending | Need to run |
| 8. Session End | ⏳ Pending | Need to run |
| 9. Ask Question | ⏳ Pending | Need to run |
| 10. Status Check | ⏳ Pending | Need to run |
| 11. Invalid Command | ⏳ Pending | Need to run |
| 12. Concurrent Sessions | ⏳ Pending | Need to run |
| 13. No Approval | ⏳ Pending | Need to run |
| 14. Module Not Found | ⏳ Pending | Need to run |
| 15. Speech Integration | ⏳ Pending | Need to run |

## Manual Testing Checklist

- [ ] Run all test scenarios
- [ ] Verify error handling
- [ ] Test edge cases
- [ ] Check performance with many modules
- [ ] Validate session cleanup
- [ ] Test with and without Speech module
- [ ] Verify module knowledge accuracy
- [ ] Test approval workflow completely
- [ ] Validate user experience
- [ ] Check documentation accuracy

## Automated Testing (Future)

Create unit tests for:
- Module knowledge building
- Session state management
- Action plan generation
- Command parsing
- Error handling

## Performance Testing

- Test with 100+ modules
- Test with 50+ concurrent sessions
- Measure response times
- Monitor memory usage

## Integration Testing

- Test with Chat module
- Test with AICodeGen module
- Test with ModuleSpawner
- Test with Knowledge module
- Test with Speech module

---

**Last Updated:** 2025-01-07
**Test Status:** Initial test scenarios defined
**Next Steps:** Execute manual tests and document results
