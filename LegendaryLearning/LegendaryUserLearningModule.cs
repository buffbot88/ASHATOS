using System.Collections.Concurrent;
using Abstractions;
using LegendaryLearning.Services;
using LegendaryLearning.Seed;

namespace LegendaryLearning;

/// <summary>
/// Legendary User Learning Module (LULmodule)
/// Provides self-paced learning courses based on permission levels.
/// Courses auto-update when new features are added to RaOS.
/// Includes trophy and achievement system for completing courses.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class LegendaryUserLearningModule : ModuleBase, ILearningModule
{
    public override string Name => "Learn RaOS";
    
    private readonly CourseService _courseService;
    private readonly LessonService _lessonService;
    private readonly ProgressService _progressService;
    private readonly AchievementService _achievementService;
    private readonly TrophyService _trophyService;
    
    public LegendaryUserLearningModule()
    {
        _courseService = new CourseService();
        _lessonService = new LessonService();
        _achievementService = new AchievementService(Name);
        _trophyService = new TrophyService(Name);
        _progressService = new ProgressService(
            _lessonService,
            _courseService,
            _achievementService,
            _trophyService,
            Name);
    }
    
    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        
        Console.WriteLine($"[{Name}] Initializing Legendary User Learning Module (LULmodule)...");
        Console.WriteLine($"[{Name}] Self-paced learning with real-time updates");
        Console.WriteLine($"[{Name}] Trophy and achievement system enabled");
        
        var seeder = new CourseSeeder(_courseService, _lessonService, Name);
        seeder.SeedInitialCourses();
        
        Console.WriteLine($"[{Name}] Learning Module initialized");
    }
    
    public override string Process(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return GetHelp();
        
        var command = parts[0].ToLowerInvariant();
        
        return command switch
        {
            "courses" when parts.Length >= 2 => ListCourses(parts[1]),
            "lessons" when parts.Length >= 2 => ListLessons(parts[1]),
            "progress" when parts.Length >= 3 => ShowProgress(parts[1], parts[2]),
            "complete" when parts.Length >= 3 => CompleteLesson(parts[1], parts[2]),
            "achievements" when parts.Length >= 2 => ShowAchievements(parts[1]),
            "trophies" when parts.Length >= 2 => ShowTrophies(parts[1]),
            "help" => GetHelp(),
            _ => GetHelp()
        };
    }
    
    private string GetHelp()
    {
        return @"Learn RaOS Module (LULmodule) Commands:
  courses <level> - List courses for permission level (User, Admin, SuperAdmin)
  lessons <courseId> - List lessons for a course
  progress <userId> <courseId> - Show user progress for a course
  complete <userId> <lessonId> - Mark a lesson as completed
  achievements <userId> - Show user achievements
  trophies <userId> - Show user trophies
  help - Show this help message";
    }
    
    private string ListCourses(string permissionLevel)
    {
        var task = _courseService.GetCoursesAsync(permissionLevel);
        task.Wait();
        var courses = task.Result;
        
        if (courses.Count == 0)
        {
            return $"No courses found for {permissionLevel} level.";
        }
        
        var lines = new List<string> { $"Found {courses.Count} courses for {permissionLevel}:" };
        foreach (var course in courses.OrderBy(c => c.Title))
        {
            lines.Add($"  [{course.Id}] {course.Title} - {course.LessonCount} lessons (~{course.EstimatedMinutes} min)");
        }
        
        return string.Join(Environment.NewLine, lines);
    }
    
    private string ListLessons(string courseId)
    {
        var task = _lessonService.GetLessonsAsync(courseId);
        task.Wait();
        var lessons = task.Result;
        
        if (lessons.Count == 0)
        {
            return $"No lessons found for course {courseId}.";
        }
        
        var lines = new List<string> { $"Found {lessons.Count} lessons:" };
        foreach (var lesson in lessons.OrderBy(l => l.OrderIndex))
        {
            lines.Add($"  {lesson.OrderIndex}. [{lesson.Id}] {lesson.Title} ({lesson.Type}) - ~{lesson.EstimatedMinutes} min");
        }
        
        return string.Join(Environment.NewLine, lines);
    }
    
    private string ShowProgress(string userId, string courseId)
    {
        var task = _progressService.GetUserProgressAsync(userId, courseId);
        task.Wait();
        var progress = task.Result;
        
        if (progress == null)
        {
            return $"No progress found for user {userId} in course {courseId}.";
        }
        
        return $"Progress: {progress.ProgressPercentage}% ({progress.CompletedLessonIds.Count} lessons completed)";
    }
    
    private string CompleteLesson(string userId, string lessonId)
    {
        var task = _progressService.CompleteLessonAsync(userId, lessonId);
        task.Wait();
        
        return task.Result 
            ? $"Lesson {lessonId} marked as completed for user {userId}." 
            : $"Failed to complete lesson {lessonId}.";
    }
    
    private string ShowAchievements(string userId)
    {
        var task = _achievementService.GetUserAchievementsAsync(userId);
        task.Wait();
        var achievements = task.Result;
        
        if (achievements.Count == 0)
        {
            return $"No achievements earned yet for user {userId}.";
        }
        
        var lines = new List<string> { $"Earned {achievements.Count} achievements:" };
        foreach (var achievement in achievements.OrderByDescending(a => a.EarnedAt))
        {
            lines.Add($"  🏆 {achievement.Title} - {achievement.Points} pts ({achievement.EarnedAt:d})");
        }
        
        return string.Join(Environment.NewLine, lines);
    }
    
    private string ShowTrophies(string userId)
    {
        var task = _trophyService.GetUserTrophiesAsync(userId);
        task.Wait();
        var trophies = task.Result;
        
        if (trophies.Count == 0)
        {
            return $"No trophies earned yet for user {userId}.";
        }
        
        var lines = new List<string> { $"Earned {trophies.Count} trophies:" };
        foreach (var trophy in trophies.OrderByDescending(t => t.Tier))
        {
            var tierIcon = trophy.Tier switch
            {
                LearningTrophyTier.Diamond => "💎",
                LearningTrophyTier.Platinum => "⭐",
                LearningTrophyTier.Gold => "🥇",
                LearningTrophyTier.Silver => "🥈",
                LearningTrophyTier.Bronze => "🥉",
                _ => "🏆"
            };
            lines.Add($"  {tierIcon} {trophy.Title} ({trophy.Tier}) - {trophy.Description}");
        }
        
        return string.Join(Environment.NewLine, lines);
    }
    
    // ILearningModule interface implementation
    public async Task<List<Course>> GetCoursesAsync(string permissionLevel)
    {
        return await _courseService.GetCoursesAsync(permissionLevel);
    }
    
    public async Task<Course?> GetCourseByIdAsync(string courseId)
    {
        return await _courseService.GetCourseByIdAsync(courseId);
    }
    
    public async Task<List<Lesson>> GetLessonsAsync(string courseId)
    {
        return await _lessonService.GetLessonsAsync(courseId);
    }
    
    public async Task<Lesson?> GetLessonByIdAsync(string lessonId)
    {
        return await _lessonService.GetLessonByIdAsync(lessonId);
    }
    
    public async Task<bool> CompleteLessonAsync(string userId, string lessonId)
    {
        return await _progressService.CompleteLessonAsync(userId, lessonId);
    }
    
    public async Task<CourseProgress?> GetUserProgressAsync(string userId, string courseId)
    {
        return await _progressService.GetUserProgressAsync(userId, courseId);
    }
    
    public async Task<List<LearningAchievement>> GetUserAchievementsAsync(string userId)
    {
        return await _achievementService.GetUserAchievementsAsync(userId);
    }
    
    public async Task<List<LearningTrophy>> GetUserTrophiesAsync(string userId)
    {
        return await _trophyService.GetUserTrophiesAsync(userId);
    }
    
    public async Task<(bool success, string message)> UpdateCourseAsync(Course course)
    {
        return await _courseService.UpdateCourseAsync(course);
    }
    
    public async Task<(bool success, string message)> UpdateLessonAsync(Lesson lesson)
    {
        return await _lessonService.UpdateLessonAsync(lesson);
    }
    
    public async Task<bool> HasCompletedSuperAdminCoursesAsync(string userId)
    {
        return await _progressService.HasCompletedSuperAdminCoursesAsync(userId);
    }
    
    public async Task<bool> MarkSuperAdminCoursesCompletedAsync(string userId)
    {
        return await _progressService.MarkSuperAdminCoursesCompletedAsync(userId);
    }
}
