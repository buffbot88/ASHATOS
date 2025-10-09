# Learning Module - SQL Database & Adaptive Assessment System

## Overview

The LegendaryUserLearningModule has been migrated from in-memory storage to a persistent SQL (SQLite) database with full support for end-of-course adaptive assessments.

## Key Features

### 1. SQL Database Persistence
- All courses, lessons, and user progress are now stored in SQLite
- Database location: `Databases/learning.sqlite`
- Automatic schema creation on first run
- Supports concurrent access and data persistence across server restarts

### 2. End-of-Course Assessments
- Each course has a comprehensive final assessment
- Questions are linked to specific lessons for granular tracking
- Multiple question types supported: Multiple Choice, True/False, Short Answer
- Configurable passing score (default: 70%)

### 3. Adaptive Retake System
The system implements intelligent remediation:
- When a user fails an assessment, the system identifies which specific lessons they struggled with
- Users only need to restudy and retake questions from failed lessons
- No need to retake the entire course if only specific sections were failed
- Failed lesson tracking stored in `UserAssessmentResults`

### 4. Progress Tracking
- Real-time course progress tracking
- User assessment history with scores and attempt dates
- Detailed analytics on which topics users find challenging

## Database Schema

```sql
-- Core learning content
Courses (Id, Title, Description, PermissionLevel, Category, LessonCount, ...)
Lessons (Id, CourseId, Title, Content, OrderIndex, EstimatedMinutes, Type, ...)
Assessments (Id, CourseId, Title, Description, PassingScore, ...)
Questions (Id, AssessmentId, LessonId, QuestionText, Type, OrderIndex, Points, ...)
Answers (Id, QuestionId, AnswerText, IsCorrect, OrderIndex)

-- User tracking
CourseProgress (UserId, CourseId, CompletedLessonIds, ProgressPercentage, ...)
UserAssessmentResults (Id, UserId, AssessmentId, Score, Passed, FailedLessonIds, UserAnswers, ...)
```

## Usage

### Console Commands

```bash
# List courses
courses User|Admin|SuperAdmin

# View lessons for a course
lessons <courseId>

# Mark lesson complete
complete <userId> <lessonId>

# Check progress
progress <userId> <courseId>

# View assessment info
test <courseId>

# View assessment results
results <userId> <courseId>
```

### Programmatic API

```csharp
// Get course assessment
var assessment = await learningModule.GetCourseAssessmentAsync(courseId);

// Check if user can take assessment (all lessons completed)
var canTake = await learningModule.CanTakeAssessmentAsync(userId, courseId);

// Submit assessment with user answers
var result = await learningModule.SubmitAssessmentAsync(userId, assessmentId, userAnswers);

// Check results
if (!result.Passed)
{
    // Show which lessons need to be restudied
    foreach (var lessonId in result.FailedLessonIds)
    {
        var lesson = await learningModule.GetLessonByIdAsync(lessonId);
        Console.WriteLine($"Please review: {lesson.Title}");
    }
}

// Get assessment history
var results = await learningModule.GetUserAssessmentResultsAsync(userId, courseId);
```

## Adaptive Assessment Flow

```
1. User completes all lessons in a course
   ↓
2. System presents final assessment covering all lessons
   ↓
3. User submits answers
   ↓
4. System calculates score and identifies failed lessons
   ↓
5a. If PASSED (≥70%): Course complete! Trophy awarded
5b. If FAILED (<70%): System shows which lessons to restudy
   ↓
6. User restudies only the failed lessons
   ↓
7. User retakes assessment (can retake full or partial)
   ↓
8. Repeat until all sections passed
```

## Benefits

### For Learners
- ✅ Efficient learning - only restudy what you need
- ✅ Clear feedback on weak areas
- ✅ Persistent progress across sessions
- ✅ Achievement tracking and trophies

### For Administrators
- ✅ Easy content updates via database
- ✅ Analytics on learning outcomes
- ✅ Scalable course management
- ✅ No code changes needed to add/update courses

### For System
- ✅ Persistent storage (survives restarts)
- ✅ Extensible schema for future features
- ✅ Transaction support for data integrity
- ✅ Standard SQL for reporting and analytics

## Migration Notes

### From In-Memory to Database
- Old in-memory ConcurrentDictionary storage removed
- All data now persists in SQLite
- Backward compatible with existing ILearningModule interface
- Course seeding happens automatically on first run

### Database Location
- Development: `./RaCore/bin/{Configuration}/net9.0/Databases/learning.sqlite`
- Production: `./Databases/learning.sqlite`
- Configurable via `LearningDatabase` constructor

## Future Enhancements

Potential future improvements:
- 🔮 Admin UI for course/assessment management
- 🔮 Question bank and random question selection
- 🔮 Timed assessments
- 🔮 Certification generation
- 🔮 Learning analytics dashboard
- 🔮 Peer review and discussion forums
- 🔮 Skill-based progression trees

## Technical Details

### Dependencies
- Microsoft.Data.Sqlite (9.0.9)
- SQLitePCLRaw.bundle_e_sqlite3 (3.0.2)
- SQLitePCLRaw.core (3.0.2)

### Performance
- Indexes on frequently queried columns
- JSON serialization for complex fields (Tags, FailedLessonIds, UserAnswers)
- Connection pooling via per-operation connections
- Async/await pattern throughout

### Security
- Parameterized queries prevent SQL injection
- User input validation in service layer
- Permission-based course access
- Audit trail via assessment results

## Testing

Run the test suite to verify functionality:
```bash
dotnet fsi test_learning_module.fsx
```

Expected results:
- ✓ Database schema validated
- ✓ Courses and assessments seeded
- ✓ Questions linked to lessons
- ✓ Assessment submission simulated
- ✓ Adaptive retake logic verified

## Support

For issues or questions:
- Check the main RaOS documentation
- Review the LegendaryUserLearningModule source code
- Submit issues on GitHub
