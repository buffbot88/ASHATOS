namespace Abstractions;

/// <summary>
/// Learning module interface for managing courses, lessons, and achievements.
/// Provides permission-based educational content for ASHATOS users.
/// </summary>
public interface ILearningModule
{
    /// <summary>
    /// Get all available courses for a specific permission level.
    /// </summary>
    Task<List<Course>> GetCoursesAsync(string permissionLevel);
    
    /// <summary>
    /// Get a specific course by ID.
    /// </summary>
    Task<Course?> GetCourseByIdAsync(string courseId);
    
    /// <summary>
    /// Get lessons for a specific course.
    /// </summary>
    Task<List<Lesson>> GetLessonsAsync(string courseId);
    
    /// <summary>
    /// Get a specific lesson by ID.
    /// </summary>
    Task<Lesson?> GetLessonByIdAsync(string lessonId);
    
    /// <summary>
    /// Mark a lesson as completed for a user.
    /// </summary>
    Task<bool> CompleteLessonAsync(string userId, string lessonId);
    
    /// <summary>
    /// Get user progress for a course.
    /// </summary>
    Task<CourseProgress?> GetUserProgressAsync(string userId, string courseId);
    
    /// <summary>
    /// Get all achievements for a user.
    /// </summary>
    Task<List<LearningAchievement>> GetUserAchievementsAsync(string userId);
    
    /// <summary>
    /// Get all trophies for a user.
    /// </summary>
    Task<List<LearningTrophy>> GetUserTrophiesAsync(string userId);
    
    /// <summary>
    /// Add or update a course (for real-time updates when new features are added).
    /// </summary>
    Task<(bool success, string message)> UpdateCourseAsync(Course course);
    
    /// <summary>
    /// Add or update a lesson.
    /// </summary>
    Task<(bool success, string message)> UpdateLessonAsync(Lesson lesson);
    
    /// <summary>
    /// Check if a user has completed all required SuperAdmin courses.
    /// </summary>
    Task<bool> HasCompletedSuperAdminCoursesAsync(string userId);
    
    /// <summary>
    /// Mark SuperAdmin courses as completed for a user (for first-time setup).
    /// </summary>
    Task<bool> MarkSuperAdminCoursesCompletedAsync(string userId);
    
    /// <summary>
    /// Get assessment for a course.
    /// </summary>
    Task<Assessment?> GetCourseAssessmentAsync(string courseId);
    
    /// <summary>
    /// Submit assessment and get results.
    /// </summary>
    Task<UserAssessmentResult> SubmitAssessmentAsync(string userId, string assessmentId, Dictionary<string, string> answers);
    
    /// <summary>
    /// Get user's assessment history for a course.
    /// </summary>
    Task<List<UserAssessmentResult>> GetUserAssessmentResultsAsync(string userId, string courseId);
    
    /// <summary>
    /// Check if user can take the final assessment (all lessons completed).
    /// </summary>
    Task<bool> CanTakeAssessmentAsync(string userId, string courseId);
}

/// <summary>
/// Represents a course in the learning system.
/// </summary>
public class Course
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PermissionLevel { get; set; } = "User"; // User, Admin, SuperAdmin
    public string Category { get; set; } = string.Empty;
    public int LessonCount { get; set; }
    public int EstimatedMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? PrerequisiteCourseId { get; set; }
}

/// <summary>
/// Represents a lesson within a course.
/// </summary>
public class Lesson
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int EstimatedMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public LessonType Type { get; set; } = LessonType.Reading;
    public string? VideoUrl { get; set; }
    public string? CodeExample { get; set; }
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Represents user progress in a course.
/// </summary>
public class CourseProgress
{
    public string UserId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public List<string> CompletedLessonIds { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ProgressPercentage { get; set; }
    public DateTime LastAccessedAt { get; set; }
}

/// <summary>
/// Represents a learning achievement earned by a user.
/// </summary>
public class LearningAchievement
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public int Points { get; set; }
    public LearningAchievementType Type { get; set; } = LearningAchievementType.CourseCompletion;
    public DateTime EarnedAt { get; set; }
}

/// <summary>
/// Represents a learning trophy earned by a user.
/// </summary>
public class LearningTrophy
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LearningTrophyTier Tier { get; set; } = LearningTrophyTier.Bronze;
    public string IconUrl { get; set; } = string.Empty;
    public DateTime EarnedAt { get; set; }
    public string? RelatedCourseId { get; set; }
}

/// <summary>
/// Types of lessons available.
/// </summary>
public enum LessonType
{
    Reading,
    Video,
    Interactive,
    CodeExample,
    Quiz
}

/// <summary>
/// Types of learning achievements.
/// </summary>
public enum LearningAchievementType
{
    CourseCompletion,
    LessonCompletion,
    FirstLesson,
    FastLearner,
    Dedicated,
    MasterClass
}

/// <summary>
/// Learning trophy tiers.
/// </summary>
public enum LearningTrophyTier
{
    Bronze,
    Silver,
    Gold,
    Platinum,
    Diamond
}

/// <summary>
/// Represents an end-of-course assessment.
/// </summary>
public class Assessment
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PassingScore { get; set; } = 70; // Percentage required to pass
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Represents a question in an assessment.
/// </summary>
public class Question
{
    public string Id { get; set; } = string.Empty;
    public string AssessmentId { get; set; } = string.Empty;
    public string LessonId { get; set; } = string.Empty; // Links question to specific lesson for targeted remediation
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;
    public int OrderIndex { get; set; }
    public int Points { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Represents an answer option for a question.
/// </summary>
public class Answer
{
    public string Id { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }
}

/// <summary>
/// Represents a user's assessment attempt and results.
/// </summary>
public class UserAssessmentResult
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string AssessmentId { get; set; } = string.Empty;
    public int Score { get; set; } // Percentage score
    public bool Passed { get; set; }
    public DateTime AttemptedAt { get; set; }
    public List<string> FailedLessonIds { get; set; } = new(); // Lessons that need retaking
    public Dictionary<string, string> UseASHATnswers { get; set; } = new(); // QuestionId -> AnswerId
}

/// <summary>
/// Types of assessment questions.
/// </summary>
public enum QuestionType
{
    MultipleChoice,
    TrueFalse,
    ShortAnswer
}
